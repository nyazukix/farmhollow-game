using System.Collections.Generic;
using UnityEngine;

namespace Farmhollow
{
// ============================================================
//  LocalizationManager
//  Loads language files (JSON) from Resources/Localization and
//  returns localized text by key.
//
//  - Files live in: Assets/Resources/Localization/<code>.json
//    (e.g. en-US.json, de-DE.json)
//  - Default language is English (en-US).
//  - Code never hardcodes player-facing text: it asks for a KEY
//    (e.g. "ui.quit") and gets the text for the active language.
// ============================================================
public static class LocalizationManager
{
    // The language that is loaded by default when the game starts.
    public const string DefaultLanguage = "en-US";

    // The currently active language code (e.g. "en-US", "de-DE").
    public static string CurrentLanguage { get; private set; } = DefaultLanguage;

    // Lookup table: key -> text, for the language that is currently loaded.
    private static Dictionary<string, string> entries = new Dictionary<string, string>();

    // True once a language file has been successfully loaded.
    private static bool isLoaded = false;

    // --- JSON shape (matches the .json files) ---
    // We use a small array of key/value pairs so Unity's built-in
    // JsonUtility can read it without any extra packages.
    [System.Serializable]
    private class LocalizationEntry
    {
        public string key;
        public string value;
    }

    [System.Serializable]
    private class LocalizationData
    {
        public string language;          // human-readable name, e.g. "English"
        public LocalizationEntry[] entries;
    }

    // Loads a language file. Call this to switch languages at runtime.
    // Example: LocalizationManager.SetLanguage("de-DE");
    public static void SetLanguage(string languageCode)
    {
        // Resources.Load finds the file by name WITHOUT the .json extension.
        TextAsset file = Resources.Load<TextAsset>("Localization/" + languageCode);
        if (file == null)
        {
            Debug.LogWarning($"[Localization] Language file '{languageCode}' not found. " +
                             $"Falling back to '{DefaultLanguage}'.");
            // Avoid an endless loop if even the default is missing.
            if (languageCode != DefaultLanguage)
                SetLanguage(DefaultLanguage);
            return;
        }

        // Turn the JSON text into our data object, then fill the lookup table.
        LocalizationData data = JsonUtility.FromJson<LocalizationData>(file.text);
        entries = new Dictionary<string, string>();
        if (data != null && data.entries != null)
        {
            foreach (LocalizationEntry entry in data.entries)
                entries[entry.key] = entry.value;
        }

        CurrentLanguage = languageCode;
        isLoaded = true;
    }

    // Returns the localized text for a key.
    // If the key is missing, it returns the key itself so the problem
    // is easy to spot directly in the game.
    public static string Get(string key)
    {
        // Load the default language the first time someone asks for text.
        if (!isLoaded)
            SetLanguage(CurrentLanguage);

        if (entries.TryGetValue(key, out string value))
            return value;

        Debug.LogWarning($"[Localization] Missing key '{key}' in language '{CurrentLanguage}'.");
        return key;
    }
}
}
