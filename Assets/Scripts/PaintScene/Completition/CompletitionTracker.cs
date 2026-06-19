using UnityEngine;
using System;

// Tracks paint progress per color zone and fires OnCompleted when all zones reach the threshold.
// Subscribes to BrushPainter.OnPixelFullyPainted — no polling, purely event-driven.
public class CompletionTracker : MonoBehaviour {
    [Range(0.7f, 1f)] public float completionThreshold = 0.9f; // % of pixels needed per color

    public bool IsCompleted { get; private set; }
    public event Action OnCompleted;

    private int[] _pixelColorIndex;  // maps each pixel index to its color zone (-1 = not tracked)
    private int[] _totalPerColor;    // total paintable pixels per color zone
    private int[] _paintedPerColor;  // how many have been fully painted per color zone

    public void Initialize(MandalaData data, PaintCanvas canvas, BrushPainter painter) {
        int size = canvas.Width * canvas.Height;
        _pixelColorIndex = new int[size];
        _totalPerColor = new int[data.colors.Length];
        _paintedPerColor = new int[data.colors.Length];

        Color32[] byn = canvas.BynPixels;
        Color32[] col = canvas.RefPixels;

        // Assign each paintable pixel to a color zone based on the color reference
        for (int i = 0; i < size; i++) {
            _pixelColorIndex[i] = -1;
            if (byn[i].a == 0 || byn[i].r < 25) continue; // skip transparent and outline pixels
            for (int c = 0; c < data.colors.Length; c++) {
                if (painter.ColorsMatch32(col[i], data.colors[c])) {
                    _pixelColorIndex[i] = c;
                    _totalPerColor[c]++;
                    break;
                }
            }
        }

        painter.OnPixelFullyPainted += OnPixelFullyPainted;
    }

    private void OnPixelFullyPainted(int idx) {
        if (IsCompleted) return;

        int ci = _pixelColorIndex[idx];
        if (ci < 0) return; // pixel doesn't belong to any tracked color zone

        _paintedPerColor[ci]++;

        // Check if every color zone has reached the completion threshold
        for (int c = 0; c < _totalPerColor.Length; c++) {
            if (_totalPerColor[c] == 0) continue;
            if ((float)_paintedPerColor[c] / _totalPerColor[c] < completionThreshold) return;
        }

        IsCompleted = true;
        OnCompleted?.Invoke();
    }
}