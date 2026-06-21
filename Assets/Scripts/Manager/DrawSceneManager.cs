using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DrawSceneManager : MonoBehaviour {
    [Header("References")]
    public Transform ringsContainer;
    public Transform overlayContainer;

    [Header("Brush")]
    public int brushRadius = 25;
    [Range(0f, 1f)]
    public float completionThreshold = 0.9f;

    [Header("Opacity")]
    [Range(0f, 1f)]
    public float ringOpacity = 0.25f;

    public DrawThumbnailRenderer thumbnailRenderer;
    public DrawCompletionPanel completionPanel;
    public DrawGuideAnimator guideAnimator;

    private RawImage _currentOverlay;
    private Texture2D _ringTexture;
    private Texture2D _currentRingSourceTexture;
    private int _currentRingIndex = 0;
    private MandalaData _data;
    private int _totalErasablePixels;
    private int _erasedPixels;
    private bool _ringComplete = false;


    void Start() {
        _data = GameManager.Instance.SelectedMandala;
        completionPanel.Initialize();
        LoadRing(_currentRingIndex);
    }

    void LoadRing(int index) {
        _ringComplete = false;
        _erasedPixels = 0;

        GameObject go = new GameObject("Ring_" + index);
        go.transform.SetParent(ringsContainer, false);
        RawImage img = go.AddComponent<RawImage>();
        img.texture = _data.rings[index].texture;
        img.color = new Color(1, 1, 1, ringOpacity);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Texture2D original = _data.rings[index].texture;
        _currentRingSourceTexture = original;
        Color[] ringPixels = original.GetPixels();

        _totalErasablePixels = 0;
        foreach (Color p in ringPixels)
            if (p.a > 0.1f)
                _totalErasablePixels++;

        Texture2D overlay = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        Color[] overlayPixels = new Color[ringPixels.Length];
        for (int i = 0; i < ringPixels.Length; i++)
            overlayPixels[i] = ringPixels[i].a > 0.1f ? new Color(1, 1, 1, 1f) : Color.clear;
        overlay.SetPixels(overlayPixels);
        overlay.Apply();
        _ringTexture = overlay;

        GameObject overlayGo = new GameObject("Overlay_" + index);
        overlayGo.transform.SetParent(overlayContainer, false);
        _currentOverlay = overlayGo.AddComponent<RawImage>();
        _currentOverlay.texture = _ringTexture;
        _currentOverlay.color = Color.white;
        RectTransform overlayRt = overlayGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        thumbnailRenderer?.InitRing(_ringTexture, original);
        if (_data.ringPaths != null && index < _data.ringPaths.Length)
            guideAnimator?.SetPath(_data.ringPaths[index]);
    }

    void Update() {
        if (_ringComplete) return;
        if (!TryGetPointerPosition(out Vector2 pointerPos )) return;
        guideAnimator?.NotifyInput();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _currentOverlay.GetComponent<RectTransform>(), pointerPos, Camera.main, out localPoint
        );

        RectTransform rect = _currentOverlay.GetComponent<RectTransform>();
        float u = (localPoint.x + rect.rect.width * 0.5f) / rect.rect.width;
        float v = (localPoint.y + rect.rect.height * 0.5f) / rect.rect.height;

        if (u < 0 || u > 1 || v < 0 || v > 1) return;

        int px = Mathf.RoundToInt(u * (_ringTexture.width - 1));
        int py = Mathf.RoundToInt(v * (_ringTexture.height - 1));

        for (int x = -brushRadius; x <= brushRadius; x++) {
            for (int y = -brushRadius; y <= brushRadius; y++) {
                if (x * x + y * y > brushRadius * brushRadius) continue;
                int tx = px + x;
                int ty = py + y;
                if (tx < 0 || tx >= _ringTexture.width || ty < 0 || ty >= _ringTexture.height) continue;

                Color pixel = _ringTexture.GetPixel(tx, ty);
                if (pixel.a > 0.1f) {
                    _ringTexture.SetPixel(tx, ty, Color.clear);
                    _erasedPixels++;
                    thumbnailRenderer?.NotifyErased(tx, ty);
                }
            }
        }

        _ringTexture.Apply();
        CheckCompletion();
    }

    bool TryGetPointerPosition(out Vector2 position) {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed) {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.isPressed) {
            position = Mouse.current.position.ReadValue();
            return true;
        }
        position = Vector2.zero;
        return false;
    }

    void CheckCompletion() {
        if (_totalErasablePixels == 0) return;
        float progress = (float)_erasedPixels / _totalErasablePixels;
        if (progress < completionThreshold) return;

        _ringComplete = true;
        thumbnailRenderer?.CompleteRing(_data.completedRings[_currentRingIndex].texture);

        _currentRingIndex++;
        if (_currentRingIndex >= _data.rings.Length) {
            completionPanel.Show(_data.thumbnail, _data.mandalaBWWatermark);
            return;
        }

        LoadRing(_currentRingIndex);
    }

    public void OnBackPressed() {
        GameManager.Instance.LoadMainMenu();
    }
}