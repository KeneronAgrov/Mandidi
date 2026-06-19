using UnityEngine;

// Handles all brush painting logic: stroke lifecycle, ink simulation, and pixel writing.
// Reads/writes PaintCanvas.BynPixels directly for performance.
public class BrushPainter : MonoBehaviour {
    [Header("Brush")]
    public int brushRadius = 15;
    [Range(0f, 0.95f)] public float brushHardness = 0.25f;  // soft <-> hard edge
    [Range(0f, 0.3f)] public float sizeVariation = 0.12f; // random width variation per segment
    [Range(0.5f, 1f)] public float baseOpacity = 1.0f;
    [Range(0f, 5f)] public float stillThreshold = 2f;     // min pixel distance to register movement
    [Range(10f, 200f)] public float remaskDistance = 40f;    // distance before a pixel can be repainted
    [Range(0f, 1f)] public float dirRemaskAngle = 0.4f;   // how much direction change tightens remask

    [Header("Ink")]
    [Range(500f, 5000f)] public float inkDistance = 1500f; // pixels traveled to empty the ink
    [Range(0f, 1f)] public float inkFullThreshold = 0.35f; // ink level below which opacity starts dropping

    public float InkLevel { get; private set; } = 1f;
    public bool HasColorSelected => _selectedColor != Color.clear;

    public event System.Action<float> OnInkChanged;
    public event System.Action<int> OnPixelFullyPainted; // fires when a pixel's alpha drops below threshold

    private PaintCanvas _canvas;
    private Color _selectedColor = Color.clear;

    // --- Stroke state ---
    private int[] _strokeMaskPos;   // packed (x,y) of last centerline point that painted each pixel
    private int[] _strokeMaskStamp; // stroke ID when each pixel was last painted
    private int _strokeId = 1;    // increments each new stroke instead of clearing the mask array
    private bool[] _fullyPainted;    // tracks pixels that already fired OnPixelFullyPainted
    private Vector2 _strokeDir = Vector2.right; // smoothed current direction
    private Vector2 _strokeStartDir = Vector2.right; // direction at stroke start, for dirFactor
    private bool _strokeStartDirSet;
    private float _prevWidEnd = -1f; // width at end of last segment, for seamless joins
    private float _bristleOffset;   // per-stroke random offset for bristle hash

    public void Initialize(PaintCanvas canvas) {
        _canvas = canvas;
        int size = canvas.Width * canvas.Height;

        // Stroke mask packs x and y into 12 bits each — max texture size 4095
        Debug.Assert(canvas.Width <= 4095 && canvas.Height <= 4095,
            $"Texture ({canvas.Width}x{canvas.Height}) excede el límite de 12 bits del strokeMask.");

        _strokeMaskPos = new int[size];
        _strokeMaskStamp = new int[size];
        _fullyPainted = new bool[size];
    }

    // Resets ink and segment width so the new color starts fresh
    public void SelectColor(Color color) {
        _selectedColor = color;
        InkLevel = 1f;
        _prevWidEnd = -1f;
        OnInkChanged?.Invoke(InkLevel);
    }

    // Starts a new stroke: bumps ID (avoids clearing the full mask array) and randomizes bristles
    public void BeginStroke() {
        _strokeId++;
        if (_strokeId == 0) _strokeId = 1; // overflow guard
        _strokeStartDirSet = false;
        _bristleOffset = Random.Range(0f, 500f);
    }

    // Called each frame the finger moves. Depletes ink, tracks direction, then paints.
    public void ContinueStroke(Vector2Int from, Vector2Int to, float dist) {
        if (InkLevel > 0f) {
            InkLevel = Mathf.Max(0f, InkLevel - dist / inkDistance);
            OnInkChanged?.Invoke(InkLevel);
        }

        Vector2 dir = ((Vector2)(to - from)).normalized;

        // Capture actual start direction on first movement of this stroke
        if (!_strokeStartDirSet) {
            _strokeStartDir = dir;
            _strokeDir = dir;
            _strokeStartDirSet = true;
        } else {
            _strokeDir = Vector2.Lerp(_strokeDir, dir, 0.25f).normalized;
        }

        // dirFactor: how much the stroke has turned since start (0 = straight, 1 = sharp turn)
        float dirDrift = 1f - Vector2.Dot(_strokeDir, _strokeStartDir);
        float dirFactor = Mathf.Clamp01(dirDrift / dirRemaskAngle);

        PaintSegment(from, to, dirFactor);
    }

    // Resets segment continuity so the next stroke starts with a fresh width
    public void EndStroke() => _prevWidEnd = -1f;

    // Core paint loop. Iterates pixels inside the stroke's bounding box and erases
    // BW alpha where color matches, simulating bristle texture and ink dryness.
    private void PaintSegment(Vector2Int from, Vector2Int to, float dirFactor) {
        float dx = to.x - from.x;
        float dy = to.y - from.y;
        float len = Mathf.Sqrt(dx * dx + dy * dy);
        if (len < stillThreshold) return;

        int W = _canvas.Width;
        int H = _canvas.Height;
        Color32[] byn = _canvas.BynPixels;
        Color32[] col = _canvas.RefPixels;

        float dirX = dx / len;
        float dirY = dy / len;
        float sideX = -_strokeDir.y; // perpendicular to stroke, used for bristle IDs
        float sideY = _strokeDir.x;

        // Width tapers continuously between segments for a natural stroke join
        bool hasHistory = _prevWidEnd > 0f;
        float widStart = hasHistory ? _prevWidEnd
                         : brushRadius * (1f + Random.Range(-sizeVariation, sizeVariation));
        float widEnd = brushRadius * (1f + Random.Range(-sizeVariation, sizeVariation));
        _prevWidEnd = widEnd;

        float dryness = 1f - InkLevel;
        float remaskDistSq = remaskDistance * remaskDistance;
        float inkOpacity = InkLevel > inkFullThreshold ? 1f : InkLevel / inkFullThreshold;

        // Bounding box of the stroke segment — avoids iterating the whole texture
        int pad = Mathf.CeilToInt(Mathf.Max(widStart, widEnd)) + 1;
        int minX = Mathf.Max(0, Mathf.Min(from.x, to.x) - pad);
        int maxX = Mathf.Min(W - 1, Mathf.Max(from.x, to.x) + pad);
        int minY = Mathf.Max(0, Mathf.Min(from.y, to.y) - pad);
        int maxY = Mathf.Min(H - 1, Mathf.Max(from.y, to.y) + pad);

        for (int x = minX; x <= maxX; x++) {
            for (int y = minY; y <= maxY; y++) {
                float px = x - from.x;
                float py = y - from.y;
                float tRaw = px * dirX + py * dirY; // projection onto stroke axis
                bool isBackCap = tRaw < 0f;
                if (isBackCap && !hasHistory) continue; // skip back cap on first segment
                if (tRaw > len) continue;

                float t = Mathf.Max(0f, tRaw);
                float tNorm = t / len;
                float closestX = from.x + t * dirX; // nearest point on stroke centerline
                float closestY = from.y + t * dirY;
                float perpDx = x - closestX;
                float perpDy = y - closestY;
                float perpDist = Mathf.Sqrt(perpDx * perpDx + perpDy * perpDy);

                // Brush width at this point, narrowed as ink runs out
                float widAtT = Mathf.Lerp(widStart, widEnd, tNorm) * (0.6f + 0.4f * InkLevel);
                if (perpDist > widAtT) continue;

                // Hash-based bristle simulation — deterministic per lateral position, no Random calls
                float across = perpDx * sideX + perpDy * sideY;
                int bristleId = Mathf.FloorToInt((across + _bristleOffset) * 2f);
                int bHash = (bristleId * 7919) ^ ((bristleId + 13) * 1000003);
                float bristleRand = (bHash & 0xFF) / 255f;
                float opacRand = (bHash >> 8 & 0xFF) / 255f;
                float bristleVar = 0.15f + dryness * 0.55f; // drier = more ragged bristles
                float bristleReach = Mathf.Clamp01(
                    (1f - dryness * 0.4f) + (bristleRand - 0.5f) * bristleVar * 2f);
                if (perpDist > widAtT * bristleReach) continue;

                int idx = y * W + x;
                bool paintedThisStroke = _strokeMaskStamp[idx] == _strokeId;

                // Remask: reduce opacity when repainting a pixel in the same stroke.
                // Prevents double-darkening; loosened on sharp turns so curves fill cleanly.
                float remaskFactor = 1f;
                if (paintedThisStroke) {
                    if (isBackCap) continue;
                    int packed = _strokeMaskPos[idx];
                    int lastX = (packed >> 12) - 1;
                    int lastY = (packed & 0xFFF) - 1;
                    float ddx = closestX - lastX;
                    float ddy = closestY - lastY;
                    float distSq = ddx * ddx + ddy * ddy;
                    float effSq = remaskDistSq * Mathf.Lerp(1f, 0.15f, dirFactor);
                    remaskFactor = Mathf.SmoothStep(0f, 1f, distSq / effSq);
                }

                // Only paint pixels that belong to the selected color zone
                if (!ColorsMatch32(col[idx], _selectedColor)) continue;
                Color32 p = byn[idx];
                if (p.r < 25 || p.a == 0) continue; // skip black outlines and already-clear pixels

                float bristleOpacity = Mathf.Clamp01(1f - dryness * 0.5f * (1f - opacRand));
                float opacity = baseOpacity * inkOpacity * bristleOpacity;
                float normDist = perpDist / (widAtT * bristleReach);
                float falloff = 1f - Mathf.SmoothStep(brushHardness, 1f, normDist * normDist);
                int eraseAmount = Mathf.RoundToInt(falloff * opacity * remaskFactor * 255f);
                if (eraseAmount == 0) continue;

                // Reduce alpha — erasing the BW layer reveals the color reference beneath
                int newAlpha = Mathf.Max(0, p.a - eraseAmount);
                byn[idx] = new Color32(p.r, p.g, p.b, (byte)newAlpha);
                _strokeMaskStamp[idx] = _strokeId;
                _strokeMaskPos[idx] = (((int)closestX + 1) << 12) | ((int)closestY + 1);
                _canvas.MarkDirty();

                // Notify CompletionTracker once per pixel when it crosses the "painted" threshold
                if (!_fullyPainted[idx] && newAlpha < 30) {
                    _fullyPainted[idx] = true;
                    OnPixelFullyPainted?.Invoke(idx);
                }
            }
        }
    }

    // Loose RGB match — threshold accounts for compression artifacts in the reference texture
    public bool ColorsMatch32(Color32 c, Color selected, float threshold = 0.3f) =>
        Mathf.Abs(c.r / 255f - selected.r) < threshold &&
        Mathf.Abs(c.g / 255f - selected.g) < threshold &&
        Mathf.Abs(c.b / 255f - selected.b) < threshold;
}