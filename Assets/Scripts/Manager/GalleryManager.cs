using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {
    public MandalaData[] allMandalas;
    public GameObject cardPrefab;
    public Transform gridContent;
    public ModePopup popup;
    public Toggle completedToggle;

    private Difficulty _currentDifficulty = Difficulty.Easy;

    void Start() {
        completedToggle.onValueChanged.AddListener(_ => Refresh());
        ShowDifficulty(Difficulty.Easy);
    }

    public void ShowEasy() => ShowDifficulty(Difficulty.Easy);
    public void ShowMedium() => ShowDifficulty(Difficulty.Medium);
    public void ShowHard() => ShowDifficulty(Difficulty.Hard);

    public void OnBackPressed() => GameManager.Instance.LoadMainMenu();

    private void ShowDifficulty(Difficulty difficulty) {
        _currentDifficulty = difficulty;
        Refresh();
    }

    private void Refresh() {
        foreach (Transform child in gridContent)
            Destroy(child.gameObject);

        foreach (MandalaData data in allMandalas) {
            if (data.difficulty != _currentDifficulty) continue;
            if (completedToggle.isOn && ProgressManager.Instance.IsCompleted(data.mandalaName)) continue;

            GameObject card = Instantiate(cardPrefab, gridContent);
            card.GetComponent<MandalaCard>().Setup(data, popup);
        }
    }
}