using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PaintSceneManager : MonoBehaviour {
    [Header("References")]
    public Image mandalaImage;
    public Transform swatchContent;
    public GameObject swatchPrefab;

    void Start() {
        MandalaData data = GameManager.Instance.SelectedMandala;

        // Set mandala
        mandalaImage.sprite = data.thumbnail;

        // Populate color swatches
        foreach (Color color in data.colors) {
            GameObject swatch = Instantiate(swatchPrefab, swatchContent);
            swatch.GetComponent<Image>().color = color;
            Color captured = color;
            swatch.GetComponent<Button>().onClick.AddListener(() => SelectColor(captured));
        }
    }

    private Color _selectedColor;

    public void SelectColor(Color color) {
        _selectedColor = color;
        Debug.Log("Selected color: " + color);
    }

    public void OnBackPressed() {
        SceneManager.LoadScene("MainMenu");
    }
}