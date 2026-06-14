using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

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
}