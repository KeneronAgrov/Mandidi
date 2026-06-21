using UnityEngine;
using UnityEngine.UI;

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
    public Image mandalaReference;

    void Start() {
        MandalaData data = GameManager.Instance.SelectedMandala;

        mandalaReference.sprite = data.mandalaColor;
        mandalaReference.color = Color.white;

        paintCanvas.Initialize(data.mandalaBW.texture, data.mandalaColor.texture);
        brushPainter.Initialize(paintCanvas);
        thumbnailRenderer.Initialize(paintCanvas);
        completionTracker.Initialize(data, paintCanvas, brushPainter);
        inputHandler.Initialize(brushPainter, paintCanvas, completionTracker);
        completionPanel.Initialize(completionTracker, data.thumbnail, data.mandalaColorWatermark, data.mandalaName);
        swatchBar.Initialize(data.colors, brushPainter);
    }

    public void OnBackPressed() => GameManager.Instance.LoadMainMenu();
}