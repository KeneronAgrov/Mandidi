using UnityEngine;

public class MainMenuManager : MonoBehaviour {
    public void OnDrawPressed() => GameManager.Instance.LoadRandomDraw();
    public void OnPaintPressed() => GameManager.Instance.LoadRandomPaint();
    public void OnGalleryPressed() => GameManager.Instance.LoadGallery();
    public void OnExitPressed() => GameManager.Instance.ExitApp();
    public void OnSettingsPressed() { }
    public void OnArtistPressed() { }
}