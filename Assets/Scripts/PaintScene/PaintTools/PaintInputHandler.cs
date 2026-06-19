using UnityEngine;
using UnityEngine.InputSystem;

// Reads touch/mouse input, converts screen position to pixel coordinates,
// and drives the BrushPainter stroke lifecycle each frame.
public class PaintInputHandler : MonoBehaviour {
    private BrushPainter _painter;
    private PaintCanvas _canvas;
    private RectTransform _mandalaRect;
    private Vector2Int? _lastPixelPos; // null = no active stroke

    public void Initialize(BrushPainter painter, PaintCanvas canvas, CompletionTracker completion) {
        _painter = painter;
        _canvas = canvas;
        _mandalaRect = canvas.MandalaRect;

        // Disable self on completion — no polling needed
        completion.OnCompleted += () => enabled = false;
    }

    void Update() {
        if (!_painter.HasColorSelected) return;

        bool pressing = false;
        Vector2 screenPos = Vector2.zero;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            pressing = true;
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        } else if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
            pressing = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (!pressing) {
            if (_lastPixelPos != null) {
                // Finger lifted — end stroke and force a final thumbnail sync
                _painter.EndStroke();
                _canvas.Flush(forced: true);
            }
            _lastPixelPos = null;
            return;
        }

        if (!TryGetPixelPosition(screenPos, out Vector2Int currentPos)) {
            _lastPixelPos = null;
            return;
        }

        float dist = _lastPixelPos == null ? 0f :
                            Vector2.Distance((Vector2)_lastPixelPos.Value, (Vector2)currentPos);
        bool fingerMoved = dist > _painter.stillThreshold;

        if (_lastPixelPos == null)
            _painter.BeginStroke();
        else if (fingerMoved)
            _painter.ContinueStroke(_lastPixelPos.Value, currentPos, dist);

        _lastPixelPos = currentPos;
        _canvas.Flush();
    }

    // Converts a screen position to the corresponding pixel coordinate on the mandala texture.
    // Returns false if the position is outside the mandala rect.
    private bool TryGetPixelPosition(Vector2 screenPos, out Vector2Int pixelPos) {
        pixelPos = Vector2Int.zero;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _mandalaRect, screenPos, Camera.main, out Vector2 local)) return false;

        // Normalize local position to [0,1] UV space
        float u = (local.x + _mandalaRect.rect.width * 0.5f) / _mandalaRect.rect.width;
        float v = (local.y + _mandalaRect.rect.height * 0.5f) / _mandalaRect.rect.height;
        if (u < 0f || u > 1f || v < 0f || v > 1f) return false;

        pixelPos = new Vector2Int(
            Mathf.RoundToInt(u * (_canvas.Width - 1)),
            Mathf.RoundToInt(v * (_canvas.Height - 1)));
        return true;
    }
}