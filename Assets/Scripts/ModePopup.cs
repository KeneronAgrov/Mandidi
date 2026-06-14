using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModePopup : MonoBehaviour {
    public Image thumbnail;
    public TextMeshProUGUI mandalaName;

    private MandalaData _data;

    public void Show(MandalaData data) {
        _data = data;
        thumbnail.sprite = data.thumbnail;
        mandalaName.text = data.mandalaName;
        gameObject.SetActive(true);
    }

    public void OnDrawPressed() {
        GameManager.Instance.LoadMandala(_data, "Draw");
        gameObject.SetActive(false);
    }

    public void OnPaintPressed() {
        GameManager.Instance.LoadMandala(_data, "Paint");
        gameObject.SetActive(false);
    }

    public void OnBackPressed() {
        gameObject.SetActive(false);
    }
}