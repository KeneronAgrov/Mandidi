using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DrawSceneManager : MonoBehaviour {
    [Header("References")]
    public Image mandalaSlice;
    public Image previewThumbnail;

    void Start() {
        MandalaData data = GameManager.Instance.SelectedMandala;

        // Placeholder — will be replaced with actual slice sprite later
        mandalaSlice.sprite = data.thumbnail;
        previewThumbnail.sprite = data.thumbnail;
    }

    public void OnBackPressed() {
        SceneManager.LoadScene("MainMenu");
    }
}