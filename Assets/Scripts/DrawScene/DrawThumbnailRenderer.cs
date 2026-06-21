using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawThumbnailRenderer : MonoBehaviour {
    [Header("Display")]
    public RawImage display;
    public int thumbnailSize = 256;

    [Header("Source Mapping")]
    public float sourceSquareSize = 1080f;
    public Vector2 sourceCenterNormalized = new Vector2(0.5f, 0.5f);

    private Texture2D _output;
    private Color32[] _outputPixels;
    private List<Color32[]> _completedLayers = new List<Color32[]>();

    void Awake() {
        _output = new Texture2D(thumbnailSize, thumbnailSize, TextureFormat.RGBA32, false);
        _output.filterMode = FilterMode.Bilinear;
        _outputPixels = new Color32[thumbnailSize * thumbnailSize];
        display.texture = _output;
    }

    // Llamar al cargar cada ring — ya no necesita overlay
    public void InitRing(Texture2D overlayTexture, Texture2D ringTexture) {
        // nada que hacer para el thumbnail en live
    }

    public void NotifyErased(int x, int y) {
        // nada — no mostramos progreso live
    }

    // Cuando se completa el ring: samplear el PNG completo al thumbnail
    public void CompleteRing(Texture2D ringTexture) {
        _completedLayers.Add(SampleToThumbnail(ringTexture));
        Render();
    }

    Color32[] SampleToThumbnail(Texture2D src) {
        var result = new Color32[thumbnailSize * thumbnailSize];
        Color32[] srcPixels = src.GetPixels32();
        int srcW = src.width;
        int srcH = src.height;
        float srcCx = sourceCenterNormalized.x * srcW;
        float srcCy = sourceCenterNormalized.y * srcH;
        float scale = sourceSquareSize / thumbnailSize;
        float half = thumbnailSize * 0.5f;

        for (int oy = 0; oy < thumbnailSize; oy++)
            for (int ox = 0; ox < thumbnailSize; ox++) {
                int sx = Mathf.RoundToInt(srcCx + (ox - half) * scale);
                int sy = Mathf.RoundToInt(srcCy + (oy - half) * scale);
                if ((uint)sx >= (uint)srcW || (uint)sy >= (uint)srcH) continue;
                Color32 p = srcPixels[sy * srcW + sx];
                if (p.a < 10) continue;
                result[oy * thumbnailSize + ox] = p;
            }
        return result;
    }

    void Render() {
        System.Array.Clear(_outputPixels, 0, _outputPixels.Length);
        foreach (var layer in _completedLayers)
            BlendOver(_outputPixels, layer);
        _output.SetPixels32(_outputPixels);
        _output.Apply(false);
    }

    static void BlendOver(Color32[] dst, Color32[] src) {
        for (int i = 0; i < dst.Length; i++)
            if (src[i].a > 10) dst[i] = src[i];
    }
}