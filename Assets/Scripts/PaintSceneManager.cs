using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PaintSceneManager : MonoBehaviour {
    [Header("References")]
    public Image mandalaImage;
    public Image mandalaReference;
    public Transform swatchContent;
    public GameObject swatchPrefab;

    private Color _selectedColor = Color.clear;
    private Texture2D _refTexture;
    private RectTransform _mandalaRect;

    void Start() {
        MandalaData data = GameManager.Instance.SelectedMandala;

        mandalaImage.sprite = data.mandalaBW;
        mandalaReference.sprite = data.mandalaColor;
        mandalaReference.color = new Color(1, 1, 1, 0);

        _refTexture = data.mandalaColor.texture;
        _mandalaRect = mandalaImage.GetComponent<RectTransform>();

        foreach (Color color in data.colors) {
            GameObject swatch = Instantiate(swatchPrefab, swatchContent);
            swatch.GetComponent<Image>().color = color;
            Color captured = color;
            swatch.GetComponent<Button>().onClick.AddListener(() => SelectColor(captured));
        }
    }

    public void SelectColor(Color color) {
        _selectedColor = color;
        Debug.Log("Selected: " + color);
    }

    void Update() {
        if (_selectedColor == Color.clear) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _mandalaRect, mousePos, null, out localPoint
        );

        float u = (localPoint.x + _mandalaRect.rect.width * 0.5f) / _mandalaRect.rect.width;
        float v = (localPoint.y + _mandalaRect.rect.height * 0.5f) / _mandalaRect.rect.height;

        if (u < 0 || u > 1 || v < 0 || v > 1) return;

        int px = Mathf.RoundToInt(u * (_refTexture.width - 1));
        int py = Mathf.RoundToInt(v * (_refTexture.height - 1));

        Color regionColor = _refTexture.GetPixel(px, py);
        regionColor.a = 1f;

        Debug.Log($"Tapped region color: {regionColor}, Selected: {_selectedColor}");

        if (ColorsMatch(regionColor, _selectedColor))
            Debug.Log("CORRECT!");
        else
            Debug.Log("WRONG");
    }

    private bool ColorsMatch(Color a, Color b, float threshold = 0.1f) {
        return Mathf.Abs(a.r - b.r) < threshold &&
               Mathf.Abs(a.g - b.g) < threshold &&
               Mathf.Abs(a.b - b.b) < threshold;
    }

    public void OnBackPressed() {
        GameManager.Instance.LoadMainMenu();
    }
}