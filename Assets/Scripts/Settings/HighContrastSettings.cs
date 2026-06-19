using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class HighContrastSettings : MonoBehaviour {
    [SerializeField] private Toggle contrastToggle;

    private const string PrefKey = "HighContrast";
    private const float ContrastOn = 60f;
    private const float SaturationOn = 40f;
    private const float ContrastOff = 0f;
    private const float SaturationOff = 0f;

    private ColorAdjustments _colorAdjustments;

    private void Start() {
        Volume globalVolume = FindFirstObjectByType<Volume>();

        if (globalVolume == null || !globalVolume.profile.TryGet(out _colorAdjustments)) {
            Debug.LogError("HighContrastSettings: Global Volume or ColorAdjustments not found.");
            return;
        }

        bool saved = PlayerPrefs.GetInt(PrefKey, 0) == 1;
        contrastToggle.isOn = saved;
        ApplyContrast(saved);

        contrastToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool value) {
        ApplyContrast(value);
        PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
    }

    private void ApplyContrast(bool high) {
        _colorAdjustments.contrast.value = high ? ContrastOn : ContrastOff;
        _colorAdjustments.saturation.value = high ? SaturationOn : SaturationOff;
    }

    private void OnDestroy() {
        contrastToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}