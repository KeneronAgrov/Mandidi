using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSettings : MonoBehaviour {
    [SerializeField] private TMP_Dropdown languageDropdown;

    private const string PrefKey = "SelectedLanguage";
    private List<Locale> _locales;

    private IEnumerator Start() {
        // Localization initializes asynchronously — must wait
        yield return LocalizationSettings.InitializationOperation;

        _locales = LocalizationSettings.AvailableLocales.Locales;

        Debug.Log($"Found {_locales.Count} locales");

        var options = new List<string>();
        foreach (var locale in _locales)
            options.Add(locale.LocaleName);

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);

        int saved = PlayerPrefs.GetInt(PrefKey, 0);
        languageDropdown.value = saved;
        languageDropdown.RefreshShownValue();

        // Apply saved locale on panel open
        LocalizationSettings.SelectedLocale = _locales[saved];

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private void OnLanguageChanged(int index) {
        StartCoroutine(SetLocale(index));
    }

    private IEnumerator SetLocale(int index) {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = _locales[index];
        PlayerPrefs.SetInt(PrefKey, index);
        PlayerPrefs.Save();
    }

    private void OnDestroy() {
        languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged);
    }
}