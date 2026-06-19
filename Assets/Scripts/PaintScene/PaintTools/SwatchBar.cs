using UnityEngine;
using UnityEngine.UI;

// Instantiates one color swatch per mandala color and wires each to BrushPainter.SelectColor.
public class SwatchBar : MonoBehaviour {
    public Transform swatchContent; // parent container for the swatch buttons
    public GameObject swatchPrefab;  // prefab with Image + Button components

    public void Initialize(Color[] colors, BrushPainter painter) {
        foreach (Color color in colors) {
            GameObject swatch = Instantiate(swatchPrefab, swatchContent);
            Image img = swatch.GetComponent<Image>();
            Button btn = swatch.GetComponent<Button>();
            img.color = color;
            Color captured = color; // capture by value — required for correct closure in loop
            btn.onClick.AddListener(() => painter.SelectColor(captured));
        }
    }
}