using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MandalaCard : MonoBehaviour {
    public Image thumbnail;
    public TextMeshProUGUI mandalaName;
    public ModePopup popup;
    public GameObject completedBadge;

    private MandalaData _data;

    public void Setup(MandalaData data, ModePopup modePopup) {
        _data = data;
        popup = modePopup;
        thumbnail.sprite = data.thumbnail;
        mandalaName.text = data.mandalaName;

        if (completedBadge != null)
            completedBadge.SetActive(ProgressManager.Instance.IsCompleted(data.mandalaName));

        GetComponent<Button>().onClick.AddListener(OnCardTapped);
    }

    private void OnCardTapped() {
        popup.Show(_data);
    }
}