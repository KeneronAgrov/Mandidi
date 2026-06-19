using UnityEngine;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour {
    public MandalaData[] allMandalas;
    public GameObject cardPrefab;
    public Transform gridContent;
    public ModePopup popup;

    private Difficulty _currentDifficulty = Difficulty.Easy;

    void Start() {
        ShowDifficulty(Difficulty.Easy);
    }

    public void ShowEasy() => ShowDifficulty(Difficulty.Easy);
    public void ShowMedium() => ShowDifficulty(Difficulty.Medium);
    public void ShowHard() => ShowDifficulty(Difficulty.Hard);

    public void OnBackPressed() => GameManager.Instance.LoadMainMenu();

    private void ShowDifficulty(Difficulty difficulty) {
        _currentDifficulty = difficulty;

        // Clear existing cards
        foreach (Transform child in gridContent)
            Destroy(child.gameObject);

        // Spawn a card for each mandala matching the selected difficulty
        foreach (MandalaData data in allMandalas) {
            if (data.difficulty != difficulty) continue;

            GameObject card = Instantiate(cardPrefab, gridContent);
            card.GetComponent<MandalaCard>().Setup(data, popup);
        }
    }
}