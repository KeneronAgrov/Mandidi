using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawGuideAnimator : MonoBehaviour {
    [Header("References")]
    public Image handIcon;
    public RectTransform canvasRect;
    public RawImage trailDisplay; // RawImage fullscreen transparente en el Canvas

    [Header("Behaviour")]
    public float idleDelay = 3f;
    public float moveSpeed = 0.4f;
    public float pauseAtWaypoint = 0.1f;

    [Header("Pulse")]
    public float pulseSpeed = 2f;
    public float pulseMin = 0.6f;
    public float pulseMax = 1f;

    [Header("Trail Brush")]
    public float trailSpacing = 15f;
    public int brushRadius = 18;
    [Range(0f, 1f)] public float brushHardness = 0.2f;
    public int texWidth = 270;
    public int texHeight = 480;

    private DrawRingPath _currentPath;
    private float _idleTimer = 0f;
    private bool _guideActive = false;
    private Coroutine _guideCoroutine;
    private Vector2 _lastDotPos;
    private Texture2D _trailTex;
    private Color32[] _trailPixels;
    private bool _trailDirty = false;

    void Awake() {
        Debug.Log($"trailDisplay: {trailDisplay}");
        if (handIcon != null) handIcon.gameObject.SetActive(false);
        _trailTex = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        _trailTex.filterMode = FilterMode.Bilinear;
        _trailPixels = new Color32[texWidth * texHeight];
        ClearTrail();
        if (trailDisplay != null)
            trailDisplay.texture = _trailTex;
        else
            Debug.LogError("trailDisplay es NULL");
    }

    public void SetPath(DrawRingPath path) {
        _currentPath = path;
        StopGuide();
        _idleTimer = 0f;
    }

    public void NotifyInput() {
        _idleTimer = 0f;
        if (_guideActive) StopGuide();
    }

    void Update() {
        if (_trailDirty) {
            _trailTex.SetPixels32(_trailPixels);
            _trailTex.Apply(false);
            _trailDirty = false;
        }

        if (_currentPath == null || _currentPath.waypoints == null || _currentPath.waypoints.Length < 2) return;
        if (_guideActive) return;

        _idleTimer += Time.deltaTime;
        if (_idleTimer >= idleDelay)
            StartGuide();
    }

    void StartGuide() {
        _guideActive = true;
        if (handIcon != null) handIcon.gameObject.SetActive(true);
        _guideCoroutine = StartCoroutine(GuideLoop());
    }

    void StopGuide() {
        _guideActive = false;
        if (_guideCoroutine != null) StopCoroutine(_guideCoroutine);
        if (handIcon != null) handIcon.gameObject.SetActive(false);
        ClearTrail();
    }

    IEnumerator GuideLoop() {
        _lastDotPos = UVToCanvas(_currentPath.waypoints[0]);

        while (true) {
            for (int i = 0; i < _currentPath.waypoints.Length - 1; i++) {
                Vector2 startPos = UVToCanvas(_currentPath.waypoints[i]);
                Vector2 endPos = UVToCanvas(_currentPath.waypoints[i + 1]);

                float elapsed = 0f;
                while (elapsed < moveSpeed) {
                    elapsed += Time.deltaTime;
                    float t = elapsed / moveSpeed;
                    Vector2 currentPos = Vector2.Lerp(startPos, endPos, t);
                    handIcon.rectTransform.anchoredPosition = currentPos;

                    float alpha = Mathf.Lerp(pulseMin, pulseMax, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
                    handIcon.color = new Color(1f, 1f, 1f, alpha);

                    if (Vector2.Distance(currentPos, _lastDotPos) >= trailSpacing) {
                        PaintBrushDot(currentPos);
                        _lastDotPos = currentPos;
                    }

                    yield return null;
                }

                if (pauseAtWaypoint > 0f) yield return new WaitForSeconds(pauseAtWaypoint);
            }

            yield return FadeOutAll(0.4f);
            yield return new WaitForSeconds(0.5f);
            ClearTrail();
            handIcon.rectTransform.anchoredPosition = UVToCanvas(_currentPath.waypoints[0]);
            yield return FadeIn(0.4f);
        }
    }

    void PaintBrushDot(Vector2 anchoredPos) {
        Vector2 size = canvasRect.rect.size;
        float u = (anchoredPos.x + size.x * 0.5f) / size.x;
        float v = (anchoredPos.y + size.y * 0.5f) / size.y;

        int cx = Mathf.RoundToInt(u * (texWidth - 1));
        int cy = Mathf.RoundToInt(v * (texHeight - 1));
        Debug.Log($"PaintBrushDot — anchoredPos:{anchoredPos} u:{u:F2} v:{v:F2} cx:{cx} cy:{cy}");
        for (int x = -brushRadius; x <= brushRadius; x++) {
            for (int y = -brushRadius; y <= brushRadius; y++) {
                int tx = cx + x;
                int ty = cy + y;
                if (tx < 0 || tx >= texWidth || ty < 0 || ty >= texHeight) continue;

                float dist = Mathf.Sqrt(x * x + y * y) / brushRadius;
                if (dist > 1f) continue;

                float falloff = 1f - Mathf.SmoothStep(brushHardness, 1f, dist);
                int idx = ty * texWidth + tx;
                byte newAlpha = (byte)Mathf.Min(255, _trailPixels[idx].a + (int)(falloff * 200));
                _trailPixels[idx] = new Color32(0, 0, 0, newAlpha);
            }
        }
        _trailDirty = true;
    }

    void ClearTrail() {
        System.Array.Clear(_trailPixels, 0, _trailPixels.Length);
        _trailTex.SetPixels32(_trailPixels);
        _trailTex.Apply(false);
    }

    IEnumerator FadeOutAll(float duration) {
        float elapsed = 0f;
        Color handStart = handIcon.color;
        Color trailStart = trailDisplay != null ? trailDisplay.color : Color.white;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            handIcon.color = new Color(handStart.r, handStart.g, handStart.b, Mathf.Lerp(handStart.a, 0f, t));
            if (trailDisplay != null)
                trailDisplay.color = new Color(1f, 1f, 1f, Mathf.Lerp(trailStart.a, 0f, t));
            yield return null;
        }
        handIcon.color = new Color(handStart.r, handStart.g, handStart.b, 0f);
        if (trailDisplay != null) trailDisplay.color = new Color(1f, 1f, 1f, 0f);
    }

    IEnumerator FadeIn(float duration) {
        if (trailDisplay != null) trailDisplay.color = Color.white;
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            handIcon.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, elapsed / duration));
            yield return null;
        }
        handIcon.color = Color.white;
    }

    Vector2 UVToCanvas(Vector2 uv) {
        Vector2 size = canvasRect.rect.size;
        return new Vector2(
            uv.x * size.x - size.x * 0.5f,
            uv.y * size.y - size.y * 0.5f
        );
    }
}