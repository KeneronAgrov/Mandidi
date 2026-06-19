using UnityEngine;
using UnityEngine.UI;

// Renders a low-resolution preview of the current paint state.
// Subscribes to PaintCanvas.OnFlushed and throttles updates to ~10fps.
public class ThumbnailRenderer : MonoBehaviour {
    public RawImage thumbnailImage;
    public int resolution = 128;

    private PaintCanvas _canvas;
    private Texture2D _texture;
    private Color32[] _pixels;
    private float _lastUpdateTime = -1f;
    private const float UpdateInterval = 0.1f; // seconds between thumbnail renders

    public void Initialize(PaintCanvas canvas) {
        _canvas = canvas;
        _texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Bilinear;
        _pixels = new Color32[resolution * resolution];

        if (thumbnailImage != null) {
            thumbnailImage.texture = _texture;
            Render();
        }

        _canvas.OnFlushed += OnCanvasFlushed;
    }

    void OnDestroy() {
        if (_canvas != null) _canvas.OnFlushed -= OnCanvasFlushed; // avoid dangling subscription
        if (_texture != null) Destroy(_texture);
    }

    private void OnCanvasFlushed(bool forced) {
        // forced = true on finger lift, bypasses throttle for a final sync
        if (!forced && Time.time - _lastUpdateTime < UpdateInterval) return;
        Render();
        _lastUpdateTime = Time.time;
    }

    private void Render() {
        int tr = resolution;
        int srcW = _canvas.Width;
        int srcH = _canvas.Height;
        Color32[] byn = _canvas.BynPixels;
        Color32[] col = _canvas.RefPixels;

        for (int ty = 0; ty < tr; ty++) {
            for (int tx = 0; tx < tr; tx++) {
                float u = (tx + 0.5f) / tr;
                float v = (ty + 0.5f) / tr;
                float srcU, srcV;

                // Left half samples normally; right half mirrors at 180° to exploit mandala symmetry
                if (u < 0.5f) { srcU = u * 2f; srcV = v; } else { srcU = (1f - u) * 2f; srcV = 1f - v; }

                int srcX = Mathf.Clamp(Mathf.RoundToInt(srcU * (srcW - 1)), 0, srcW - 1);
                int srcY = Mathf.Clamp(Mathf.RoundToInt(srcV * (srcH - 1)), 0, srcH - 1);
                int srcIdx = srcY * srcW + srcX;

                Color32 b = byn[srcIdx];
                Color32 c = col[srcIdx];
                float a = b.a / 255f;

                // Blend BW layer over color reference using remaining alpha
                // As BW alpha decreases (user paints), the color shows through
                _pixels[ty * tr + tx] = new Color32(
                    (byte)(c.r + (b.r - c.r) * a),
                    (byte)(c.g + (b.g - c.g) * a),
                    (byte)(c.b + (b.b - c.b) * a),
                    255);
            }
        }

        _texture.SetPixels32(_pixels);
        _texture.Apply();
    }
}