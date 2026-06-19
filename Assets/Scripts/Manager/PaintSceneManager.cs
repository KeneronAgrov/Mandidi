using UnityEngine;
using UnityEngine.UI;

// Scene coordinator — loads MandalaData and initializes all systems in dependency order.
// Contains no game logic; delegates everything to its child components.
public class PaintSceneManager : MonoBehaviour {
    [Header("Components")]
    public PaintCanvas paintCanvas;
    public BrushPainter brushPainter;
    public PaintInputHandler inputHandler;
    public ThumbnailRenderer thumbnailRenderer;
    public CompletionTracker completionTracker;
    public CompletionPanel completionPanel;
    public SwatchBar swatchBar;

    [Header("References")]
    public Image mandalaReference; // color reference image shown alongside the canvas

    void Start() {
        MandalaData data = GameManager.Instance.SelectedMandala;

        mandalaReference.sprite = data.mandalaColor;
        mandalaReference.color = Color.white;

        // Initialize in dependency order — each system may depend on the ones above it
        paintCanvas.Initialize(data.mandalaBW.texture, data.mandalaColor.texture);
        brushPainter.Initialize(paintCanvas);
        thumbnailRenderer.Initialize(paintCanvas);
        completionTracker.Initialize(data, paintCanvas, brushPainter);
        inputHandler.Initialize(brushPainter, paintCanvas, completionTracker);
        completionPanel.Initialize(completionTracker, data.thumbnail, data.mandalaColorWatermark);
        swatchBar.Initialize(data.colors, brushPainter);
    }

    public void OnBackPressed() => GameManager.Instance.LoadMainMenu();
}