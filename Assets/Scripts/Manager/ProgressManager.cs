using System.Collections.Generic;
using UnityEngine;

public class ProgressManager : MonoBehaviour {
    public static ProgressManager Instance { get; private set; }

    private const string PrefsKey = "CompletedMandalas";
    private HashSet<string> _completed = new HashSet<string>();

    void Awake() {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void CompleteMandala(string mandalaName) {
        if (_completed.Add(mandalaName))
            Save();
    }

    public bool IsCompleted(string mandalaName) => _completed.Contains(mandalaName);

    private void Save() {
        PlayerPrefs.SetString(PrefsKey, string.Join(",", _completed));
        PlayerPrefs.Save();
    }

    private void Load() {
        string raw = PlayerPrefs.GetString(PrefsKey, "");
        _completed.Clear();
        if (!string.IsNullOrEmpty(raw))
            foreach (var name in raw.Split(','))
                _completed.Add(name);
    }
}