﻿using ModLibrary;
using ModLibrary.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternalModBot
{
    /// <summary>
    /// Handles localization of string added in Mod-Bot
    /// </summary>
    public static class ModBotLocalizationManager
    {
        static List<Tuple<string, string>> _localizedStringsToAdd = new List<Tuple<string, string>>();
        static bool _isAddStringsCoroutineRunning;

        const char ID_STRING_SEPARATOR = ':';

        const string LANGUAGE_ID_ENGLISH = "en";
        const string LANGUAGE_ID_ITALIAN = "it";
        const string LANGUAGE_ID_SPANISH_LATIN_AMERICA = "es-419"; // Currently normal Spanish, Google Translate does not support this variant of Spanish
        const string LANGUAGE_ID_RUSSIAN = "ru";
        const string LANGUAGE_ID_GERMAN = "de";
        const string LANGUAGE_ID_FRENCH = "fr";
        const string LANGUAGE_ID_SPANISH_SPAIN = "es-ES";
        const string LANGUAGE_ID_SIMPLIFIED_CHINESE = "zh-CN";
        const string LANGUAGE_ID_BRAZILIAN_PORTUGUESE = "pt-BR"; // Currently normal Portuguese, Google Translate does not support Brazilian Portuguese

        static Queue<string> _localizationIDsToLog;
        static bool _isLogQueueCoroutineRunning;

        static string getLocalizationFileContentsForCurrentLanguage()
        {
            string languageID = SettingsManager.Instance.GetCurrentLanguageID();

            switch (languageID)
            {
                case LANGUAGE_ID_ENGLISH:
                    return Resources.ModBot_English;
                case LANGUAGE_ID_ITALIAN:
                    return Resources.ModBot_Italian;
                case LANGUAGE_ID_SPANISH_LATIN_AMERICA:
                    return Resources.ModBot_Spanish_LatinAmerica;
                case LANGUAGE_ID_RUSSIAN:
                    return Resources.ModBot_Russian;
                case LANGUAGE_ID_GERMAN:
                    return Resources.ModBot_German;
                case LANGUAGE_ID_FRENCH:
                    return Resources.ModBot_French;
                case LANGUAGE_ID_SPANISH_SPAIN:
                    return Resources.ModBot_Spanish_Spain;
                case LANGUAGE_ID_SIMPLIFIED_CHINESE:
                    return Resources.ModBot_Simplified_Chinese;
                case LANGUAGE_ID_BRAZILIAN_PORTUGUESE:
                    return Resources.ModBot_Brazilian_Portuguese;
                default:
                    throw new ArgumentException("Unknown language ID: " + languageID);
            }
        }

        /// <summary>
        /// Gets the translated string via <see cref="LocalizationManager.GetTranslatedString(string)"/> and formats the returned <see langword="string"/> with the given arguments
        /// </summary>
        /// <param name="ID">The localization ID to get the translated string of</param>
        /// <param name="arguments">The arguments to format into the string</param>
        /// <returns>The translated and formatted string</returns>
        public static string FormatLocalizedStringFromID(string ID, params object[] arguments)
        {
            return string.Format(LocalizationManager.Instance.GetTranslatedString(ID), arguments);
        }

        /// <summary>
        /// Adds all Mod-Bot localization IDs and translated text for the current language into the given dictionary
        /// </summary>
        /// <param name="languageDictionary"></param>
        public static void AddAllLocalizationStringsToDictionary(Dictionary<string, string> languageDictionary)
        {
            if (languageDictionary == null)
                throw new ArgumentNullException(nameof(languageDictionary));

            string[] translations = getLocalizationFileContentsForCurrentLanguage().Split('\n');
            foreach (string translationLine in translations)
            {
                if (string.IsNullOrWhiteSpace(translationLine))
                    continue;

                string[] splitLine = translationLine.Split(new char[] { ID_STRING_SEPARATOR }, 2, StringSplitOptions.RemoveEmptyEntries); // This will split the string by the first occurence of the ID_STRING_SEPARATOR character, just in case there are some in the translated string

                if (splitLine.Length != 2)
                    continue;

                string id = splitLine[0];
                string translatedText = splitLine[1].Replace("\\n", "\n"); // Replace "\n" with an actual newline

                languageDictionary.Add(id, translatedText);
            }
        }

        internal static void TryAddLocalizationStringToDictionary(string ID, string text)
        {
            if (!LocalizationManager.Instance.IsInitialized())
            {
                if (!_isAddStringsCoroutineRunning)
                    StaticCoroutineRunner.StartStaticCoroutine(waitForLocalizationManagerThenAddAllStrings());

                _localizedStringsToAdd.Add(new Tuple<string, string>(ID, text));
                return;
            }

            Dictionary<string, string> localizationDictionary = Accessor.GetPrivateField<LocalizationManager, Dictionary<string, string>>("_translatedStringsDictionary", LocalizationManager.Instance);

            if (!localizationDictionary.ContainsKey(ID))
                localizationDictionary.Add(ID, text);
        }

        /// <summary>
        /// Passes the output of <see cref="LocalizationManager.GetTranslatedString(string)"/> into <see cref="debug.Log(string)"/> once the <see cref="LocalizationManager"/> is initialized
        /// </summary>
        /// <param name="localizationID"></param>
        public static void LogLocalizedStringOnceLocalizationManagerInitialized(string localizationID)
        {
            if (LocalizationManager.Instance.IsInitialized())
            {
                debug.Log(LocalizationManager.Instance.GetTranslatedString(localizationID));
                return;
            }

            if (_localizationIDsToLog == null)
                _localizationIDsToLog = new Queue<string>();

            _localizationIDsToLog.Enqueue(localizationID);

            if (!_isLogQueueCoroutineRunning)
                StaticCoroutineRunner.StartStaticCoroutine(waitForLocalizationManagerThenLogQueue());
        }

        static IEnumerator waitForLocalizationManagerThenLogQueue()
        {
            _isLogQueueCoroutineRunning = true;

            yield return new UnityEngine.WaitUntil(LocalizationManager.Instance.IsInitialized);
            while(_localizationIDsToLog.Count > 0)
            {
                string text = LocalizationManager.Instance.GetTranslatedString(_localizationIDsToLog.Dequeue());
                debug.Log(text);
            }

            _isLogQueueCoroutineRunning = false;
        }

        static IEnumerator waitForLocalizationManagerThenAddAllStrings()
        {
            _isAddStringsCoroutineRunning = true;

            yield return new UnityEngine.WaitUntil(LocalizationManager.Instance.IsInitialized);

            Dictionary<string, string> localizationDictionary = Accessor.GetPrivateField<LocalizationManager, Dictionary<string, string>>("_translatedStringsDictionary", LocalizationManager.Instance);
            foreach (Tuple<string, string> idAndLocalizedString in _localizedStringsToAdd)
            {
                if (localizationDictionary.ContainsKey(idAndLocalizedString.Item1))
                    continue;

                localizationDictionary.Add(idAndLocalizedString.Item1, idAndLocalizedString.Item2);
            }

            _localizedStringsToAdd.Clear();
            _isAddStringsCoroutineRunning = false;
        }
    }
}