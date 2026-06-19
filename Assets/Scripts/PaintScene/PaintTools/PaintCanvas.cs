using UnityEngine;
using UnityEngine.UI;
using System;

// Source of truth for the mandala's texture and pixel data.
// Other systems read/write BynPixels directly, then call Flush() to sync to GPU.
public class PaintCanvas : MonoBehaviour {
    public RawImage mandalaImage;

    // CPU pixel arrays — direct access for performance in tight loops
    public Color32[] BynPixels { get; private set; } // working paint layer (alpha gets erased)
    public Color32[] RefPixels { get; private set; } // color reference, read-only
    public int Width { get; private set; }
    public int Height { get; private set; }
    public RectTransform MandalaRect { get; private set; } // cached for screen-to-pixel conversion

    // Fired after each GPU upload. bool = forced (e.g. on finger lift)
    public event Action<bool> OnFlushed;

    private Texture2D _bynTexture; // GPU-side texture displayed on screen
    private bool _dirty;

    // bwSource = black-and-white ink layer | colorRef = full-color reference
    public void Initialize(Texture2D bwSource, Texture2D colorRef) {
        Width = bwSource.width;
        Height = bwSource.height;

        // Runtime copy of the BW texture — original assets can't be modified
        _bynTexture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
        _bynTexture.SetPixels(bwSource.GetPixels());
        _bynTexture.Apply();

        mandalaImage.texture = _bynTexture;
        MandalaRect = mandalaImage.GetComponent<RectTransform>();

        BynPixels = _bynTexture.GetPixels32();
        RefPixels = colorRef.GetPixels32();
    }

    public void MarkDirty() => _dirty = true;

    // Uploads CPU pixels to GPU. forced = true skips thumbnail throttle
    public void Flush(bool forced = false) {
        if (!_dirty) return;
        _bynTexture.SetPixels32(BynPixels);
        _bynTexture.Apply();
        _dirty = false;
        OnFlushed?.Invoke(forced);
    }

    // new Texture2D() is not GC'd — must be destroyed manually to avoid VRAM leaks
    void OnDestroy() {
        if (_bynTexture != null) Destroy(_bynTexture);
    }
}