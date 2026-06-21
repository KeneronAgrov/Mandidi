using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CompletionPanel : MonoBehaviour {
    public GameObject panelRoot;
    public RawImage fullMandalaDisplay;
    public Button btnRepeat;
    public Button btnRandom;
    public Button btnGallery;
    public Button btnDownload;
    public Button btnExit;

    private Sprite _completionSprite;
    private Sprite _watermarkSprite;
    private string _mandalaName;

    public void Initialize(CompletionTracker tracker, Sprite completionSprite, Sprite watermarkSprite, string mandalaName) {
        _completionSprite = completionSprite;
        _watermarkSprite = watermarkSprite;
        _mandalaName = mandalaName;

        panelRoot.SetActive(false);

        if (btnRepeat) btnRepeat.onClick.AddListener(OnRepeat);
        if (btnRandom) btnRandom.onClick.AddListener(OnRandom);
        if (btnGallery) btnGallery.onClick.AddListener(OnGallery);
        if (btnDownload) btnDownload.onClick.AddListener(OnDownload);
        if (btnExit) btnExit.onClick.AddListener(OnExit);

        tracker.OnCompleted += Show;
    }

    private void Show() {
        ProgressManager.Instance.CompleteMandala(_mandalaName);
        if (fullMandalaDisplay != null && _completionSprite != null)
            fullMandalaDisplay.texture = _completionSprite.texture;
        panelRoot.SetActive(true);
    }

    private void OnRepeat() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    private void OnRandom() => GameManager.Instance.LoadRandomPaint();
    private void OnGallery() => GameManager.Instance.LoadGallery();
    private void OnExit() => GameManager.Instance.LoadMainMenu();

    private void OnDownload() {
        if (_watermarkSprite == null) { Debug.LogWarning("mandalaColorWatermark no asignado."); return; }
        byte[] png = _watermarkSprite.texture.EncodeToPNG();
        string fileName = "mandidi_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        System.IO.File.WriteAllBytes(path, png);
#if UNITY_ANDROID && !UNITY_EDITOR
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