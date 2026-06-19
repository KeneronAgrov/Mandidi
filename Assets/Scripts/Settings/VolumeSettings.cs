using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    private const string PrefKey = "MasterVolume";
    private const string MixerParam = "MasterVolume"; // must match your exposed parameter name

    private void Start()
    {
        float saved = PlayerPrefs.GetFloat(PrefKey, 1f);
        volumeSlider.value = saved;
        ApplyVolume(saved);

        volumeSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    private void OnSliderChanged(float value)
    {
        ApplyVolume(value);
        PlayerPrefs.SetFloat(PrefKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyVolume(float value)
    {
        // Clamp to avoid log(0) = -infinity
        float db = Mathf.Log10(Mathf.Max(value, 0.001f)) * 20f;
        audioMixer.SetFloat(MixerParam, db);
    }

    private void OnDestroy()
    {
        volumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }
}
