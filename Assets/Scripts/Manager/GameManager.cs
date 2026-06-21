using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;

    [Header("All Mandalas")]
    public MandalaData[] allMandalas;

    [Header("Fade")]
    public float fadeDuration = 0.4f;

    public MandalaData SelectedMandala { get; private set; }
    public string SelectedMode { get; private set; }

    private Image _fadePanel;

    public void LoadRandomDraw() => LoadRandom("Draw");
    public void LoadRandomPaint() => LoadRandom("Paint");
    public void LoadMainMenu() => StartCoroutine(FadeAndLoad("MainMenu"));
    public void LoadGallery() => StartCoroutine(FadeAndLoad("Gallery"));
    public void ExitApp() => Application.Quit();

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFadePanel();
        ApplySavedSettings();
    }

    void Start() {
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float db = Mathf.Log10(Mathf.Max(volume, 0.001f)) * 20f;
        audioMixer.SetFloat("MasterVolume", db);
    }

    private void CreateFadePanel() {
        GameObject canvasGo = new GameObject("FadeCanvas");
        DontDestroyOnLoad(canvasGo);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        canvasGo.AddComponent<CanvasScaler>();

        GameObject panelGo = new GameObject("FadePanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        _fadePanel = panelGo.AddComponent<Image>();
        _fadePanel.color = new Color(0, 0, 0, 0);
        _fadePanel.raycastTarget = false;
        RectTransform rt = _fadePanel.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private IEnumerator FadeAndLoad(string sceneName) {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
        yield return Fade(1f, 0f);
    }

    private IEnumerator Fade(float from, float to) {
        float elapsed = 0f;
        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            _fadePanel.color = new Color(0, 0, 0, a);
            yield return null;
        }
        _fadePanel.color = new Color(0, 0, 0, to);
    }

    public void LoadMandala(MandalaData data, string mode) {
        SelectedMandala = data;
        SelectedMode = mode;
        StartCoroutine(FadeAndLoad(mode == "Draw" ? "Draw" : "Paint"));
    }

    public void LoadRandom(string mode) {
        Debug.Log("LoadRandom called: " + mode);
        if (allMandalas.Length == 0) { Debug.LogError("No mandalas assigned to GameManager."); return; }
        SelectedMandala = allMandalas[Random.Range(0, allMandalas.Length)];
        SelectedMode = mode;
        StartCoroutine(FadeAndLoad(mode == "Draw" ? "Draw" : "Paint"));
    }

    private void ApplySavedSettings() {
        StartCoroutine(ApplySavedLocale());
        bool highContrast = PlayerPrefs.GetInt("HighContrast", 0) == 1;
        Volume globalVolume = FindFirstObjectByType<Volume>();
        if (globalVolume != null && globalVolume.profile.TryGet(out ColorAdjustments ca)) {
            ca.contrast.value = highContrast ? 60f : 0f;
            ca.saturation.value = highContrast ? 40f : 0f;
        }
    }

    private IEnumerator ApplySavedLocale() {
        yield return LocalizationSettings.InitializationOperation;
        int savedIndex = PlayerPrefs.GetInt("SelectedLanguage", 0);
        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (savedIndex < locales.Count)
            LocalizationSettings.SelectedLocale = locales[savedIndex];
    }
}