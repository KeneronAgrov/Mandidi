using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MandalaCard : MonoBehaviour {
    public Image thumbnail;
    public TextMeshProUGUI mandalaName;
    public ModePopup popup;

    private MandalaData _data;

    public void Setup(MandalaData data, ModePopup modePopup) {
        _data = data;
        popup = modePopup;
        thumbnail.sprite = data.thumbnail;
        mandalaName.text = data.mandalaName;
        GetComponent<Button>().onClick.AddListener(OnCardTapped);
    }

    private void OnCardTapped() {
        // open the Draw/Paint popup and pass _data to it
        // we'll wire this up when we build the popup
        popup.Show(_data);
    }
}