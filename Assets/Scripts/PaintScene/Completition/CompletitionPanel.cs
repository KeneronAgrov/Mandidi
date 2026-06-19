using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Controls the post-completion panel: shows the finished mandala and handles all exit actions.
// Activates itself by subscribing to CompletionTracker.OnCompleted.
public class CompletionPanel : MonoBehaviour {
    public GameObject panelRoot;
    public RawImage fullMandalaDisplay; // shows the completed mandala sprite
    public Button btnRepeat;
    public Button btnRandom;
    public Button btnGallery;
    public Button btnDownload;
    public Button btnExit;

    private Sprite _completionSprite; // thumbnail shown on the panel when complete
    private Sprite _watermarkSprite;  // artist's original — saved on download (not the painted version)

    public void Initialize(CompletionTracker tracker, Sprite completionSprite, Sprite watermarkSprite) {
        _completionSprite = completionSprite;
        _watermarkSprite = watermarkSprite;

        panelRoot.SetActive(false);

        if (btnRepeat) btnRepeat.onClick.AddListener(OnRepeat);
        if (btnRandom) btnRandom.onClick.AddListener(OnRandom);
        if (btnGallery) btnGallery.onClick.AddListener(OnGallery);
        if (btnDownload) btnDownload.onClick.AddListener(OnDownload);
        if (btnExit) btnExit.onClick.AddListener(OnExit);

        tracker.OnCompleted += Show;
    }

    private void Show() {
        if (fullMandalaDisplay != null && _completionSprite != null)
            fullMandalaDisplay.texture = _completionSprite.texture;
        panelRoot.SetActive(true);
    }

    private void OnRepeat() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    private void OnRandom() => GameManager.Instance.LoadRandomPaint();
    private void OnGallery() => GameManager.Instance.LoadGallery();
    private void OnExit() => GameManager.Instance.LoadMainMenu();

    private void OnDownload() {
        if (_watermarkSprite == null) {
            Debug.LogWarning("mandalaColorWatermark no asignado en MandalaData.");
            return;
        }

        byte[] png = _watermarkSprite.texture.EncodeToPNG();
        string fileName = "mandidi_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllBytes(path, png);

#if UNITY_ANDROID && !UNITY_EDITOR
        // Notify the Android media scanner so the file appears in the gallery immediately
        using (var sc  = new AndroidJavaClass("android.media.MediaScannerConnection"))
        using (var up  = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var ctx = up.GetStatic<AndroidJavaObject>("currentActivity"))
            sc.CallStatic("scanFile", ctx, new[] { path }, null, null);
#elif UNITY_IOS && !UNITY_EDITOR
        Debug.Log("iOS: guardado en " + path);
#else
        Debug.Log("Editor: guardado en " + path);
#endif
    }
}