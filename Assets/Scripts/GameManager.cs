using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;

    [Header("All Mandalas")]
    public MandalaData[] allMandalas;

    // Selected mandala carried into Draw/Paint scene
    public MandalaData SelectedMandala { get; private set; }
    public string SelectedMode { get; private set; }

    public void LoadRandomDraw() => LoadRandom("Draw");
    public void LoadRandomPaint() => LoadRandom("Paint");

    public void LoadMainMenu() => SceneManager.LoadScene("MainMenu");

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplySavedSettings();
    }

    private void ApplySavedSettings() {


        // Idioma - esto necesita coroutine
        StartCoroutine(ApplySavedLocale());
        // High Contrast
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

    // Called from ModePopup (Gallery)
    public void LoadMandala(MandalaData data, string mode) {
        SelectedMandala = data;
        SelectedMode = mode;
        SceneManager.LoadScene(mode == "Draw" ? "Draw" : "Paint");
    }

    // Called from Main Menu Draw/Paint buttons
    public void LoadRandom(string mode) {
        Debug.Log("LoadRandom called: " + mode);
        if (allMandalas.Length == 0) { Debug.LogError("No mandalas assigned to GameManager."); return; }
        SelectedMandala = allMandalas[Random.Range(0, allMandalas.Length)];
        SelectedMode = mode;
        SceneManager.LoadScene(mode == "Draw" ? "Draw" : "Paint");
    }

    // Scene loaders for plain buttons
    public void LoadGallery() => SceneManager.LoadScene("Gallery");
    public void ExitApp() => Application.Quit();

    void Start() {
        float volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float db = Mathf.Log10(Mathf.Max(volume, 0.001f)) * 20f;
        audioMixer.SetFloat("MasterVolume", db);
    }
}