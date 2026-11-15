using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace FoglightIsles
{
    /// <summary>
    /// Runtime string table for Foglight Isles.
    /// Loads Resources/Localization/strings.json once, falls back to zh-CN for missing keys,
    /// remembers the chosen language in PlayerPrefs and never throws on a missing entry.
    /// </summary>
    public static class Localizer
    {
        public const string DefaultLanguage = "zh-CN";
        private const string ResourcePath = "Localization/strings";
        private const string LanguagePrefKey = "foglightisles.localization.language.v1";

        private static Dictionary<string, Dictionary<string, string>> _tables;
        private static string _language;
        private static readonly HashSet<string> WarnedKeys = new HashSet<string>();

        public static event Action LanguageChanged;

        public static string Language
        {
            get
            {
                if (_language != null) return _language;
                _language = PlayerPrefs.GetString(LanguagePrefKey, DefaultLanguage);
                if (string.IsNullOrEmpty(_language)) _language = DefaultLanguage;
                return _language;
            }
            set
            {
                string next = string.IsNullOrEmpty(value) ? DefaultLanguage : value;
                if (next == Language) return;
                _language = next;
                PlayerPrefs.SetString(LanguagePrefKey, next);
                PlayerPrefs.Save();
                WarnedKeys.Clear();
                LanguageChanged?.Invoke();
            }
        }

        public static bool Has(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            EnsureLoaded();
            return _tables.TryGetValue(Language, out var table) && table.ContainsKey(key);
        }

        /// <summary>Plain lookup: current language, then zh-CN, then the key itself.</summary>
        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            EnsureLoaded();
            if (_tables.TryGetValue(Language, out var table) && table.TryGetValue(key, out string value))
                return value;
            if (_tables.TryGetValue(DefaultLanguage, out var fallback) && fallback.TryGetValue(key, out string original))
            {
                WarnOnce(key, Language);
                return original;
            }
            WarnOnce(key, "MISSING");
            return key;
        }

        /// <summary>Formatted lookup. Template uses {0}, {1}, ... like string.Format.</summary>
        public static string Get(string key, params object[] args)
        {
            string template = Get(key);
            if (args == null || args.Length == 0) return template;
            try
            {
                return string.Format(CultureInfo.InvariantCulture, template, args);
            }
            catch (FormatException)
            {
                Debug.LogWarning($"[Localizer] Bad format string for key '{key}': {template}");
                return template;
            }
        }

        /// <summary>Looks up a table for an explicit language without touching the saved setting.</summary>
        public static string GetIn(string language, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            EnsureLoaded();
            if (_tables.TryGetValue(language, out var table) && table.TryGetValue(key, out string value))
                return value;
            if (_tables.TryGetValue(DefaultLanguage, out var fallback) && fallback.TryGetValue(key, out string original))
                return original;
            return key;
        }

        public static IReadOnlyCollection<string> AvailableLanguages
        {
            get
            {
                EnsureLoaded();
                return _tables.Keys;
            }
        }

        public static void Reload()
        {
            _tables = null;
            WarnedKeys.Clear();
            EnsureLoaded();
            LanguageChanged?.Invoke();
        }

        private static void WarnOnce(string key, string language)
        {
            if (!WarnedKeys.Add(language + "|" + key)) return;
            Debug.LogWarning($"[Localizer] No entry for '{key}' in '{language}'.");
        }

        private static void EnsureLoaded()
        {
            if (_tables != null) return;
            _tables = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            TextAsset asset = Resources.Load<TextAsset>(ResourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[Localizer] Resources/{ResourcePath}.json is missing from this build.");
                return;
            }
            try
            {
                ParseRoot(asset.text, _tables);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Localizer] Failed to parse strings.json: " + exception.Message);
            }
        }

        // Minimal parser for the fixed shape {"lang": {"key": "value", ...}, ...}.
        // Avoids a JSON dependency while keeping strings.json human-editable.
        private static void ParseRoot(string json, Dictionary<string, Dictionary<string, string>> target)
        {
            int index = 0;
            SkipWhitespace(json, ref index);
            Expect(json, ref index, '{');
            SkipWhitespace(json, ref index);
            if (Peek(json, index) == '}')
            {
                index++;
                return;
            }
            while (true)
            {
                string language = ReadString(json, ref index);
                SkipWhitespace(json, ref index);
                Expect(json, ref index, ':');
                SkipWhitespace(json, ref index);
                var table = new Dictionary<string, string>(StringComparer.Ordinal);
                Expect(json, ref index, '{');
                SkipWhitespace(json, ref index);
                if (Peek(json, index) == '}')
                {
                    index++;
                }
                else
                {
                    while (true)
                    {
                        string key = ReadString(json, ref index);
                        SkipWhitespace(json, ref index);
                        Expect(json, ref index, ':');
                        SkipWhitespace(json, ref index);
                        table[key] = ReadString(json, ref index);
                        SkipWhitespace(json, ref index);
                        char next = json[index++];
                        if (next == '}') break;
                        if (next != ',') throw new FormatException("Expected ',' or '}' in language table.");
                        SkipWhitespace(json, ref index);
                    }
                }
                target[language] = table;
                SkipWhitespace(json, ref index);
                char separator = json[index++];
                if (separator == '}') break;
                if (separator != ',') throw new FormatException("Expected ',' or '}' after language table.");
                SkipWhitespace(json, ref index);
            }
        }

        private static char Peek(string json, int index) => index < json.Length ? json[index] : '\0';

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index])) index++;
        }

        private static void Expect(string json, ref int index, char expected)
        {
            if (index >= json.Length || json[index] != expected)
                throw new FormatException($"Expected '{expected}' at {index}.");
            index++;
        }

        private static string ReadString(string json, ref int index)
        {
            Expect(json, ref index, '"');
            var builder = new StringBuilder();
            while (index < json.Length)
            {
                char character = json[index++];
                if (character == '"') return builder.ToString();
                if (character != '\\')
                {
                    builder.Append(character);
                    continue;
                }
                if (index >= json.Length) break;
                char escape = json[index++];
                switch (escape)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u':
                        if (index + 4 <= json.Length &&
                            ushort.TryParse(json.Substring(index, 4), NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture, out ushort code))
                        {
                            builder.Append((char)code);
                            index += 4;
                        }
                        break;
                    default: builder.Append(escape); break;
                }
            }
            throw new FormatException("Unterminated string in strings.json.");
        }
    }
}
