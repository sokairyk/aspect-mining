using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHunspell;

namespace Tools
{
    public static class HunspellChecker
    {
        private static SpellEngine spellEngine;
        private static Dictionary<String, String> languages = new Dictionary<string, string> { { "en", "en_us" } }; //Key should be ISO 639-1 values

        static HunspellChecker()
        {
            try
            {
                string dictionaryPath = Hunspell.NativeDllPath;

                spellEngine = new SpellEngine();
                foreach (String lang in languages.Keys)
                {
                    LanguageConfig newConfig = new LanguageConfig();
                    newConfig.LanguageCode = lang;
                    newConfig.HunspellAffFile = Path.Combine(dictionaryPath, String.Format("{0}.aff", languages[lang]));
                    newConfig.HunspellDictFile = Path.Combine(dictionaryPath, String.Format("{0}.dic", languages[lang]));
                    newConfig.HunspellKey = "";
                    newConfig.HyphenDictFile = Path.Combine(dictionaryPath, String.Format("hyph_{0}.dic", languages[lang]));
                    newConfig.MyThesDatFile = Path.Combine(dictionaryPath, String.Format("th_{0}_new.dat", languages[lang]));
                    spellEngine.AddLanguage(newConfig);
                }
            }
            catch (Exception ex)
            {
                if (spellEngine != null)
                    spellEngine.Dispose();
                throw new Exception(ex.Message);
            }
        }

        public static bool IsCorrect(string word, string language)
        {
            if (!languages.ContainsKey(language)) throw new Exception("Language: " + language + " is not supported.");
            return spellEngine[language].Spell(word);
        }

        public static List<String> GetSuggentions(string word, string language)
        {
            String meaning = String.Empty;
            if (!languages.ContainsKey(language)) throw new Exception("Language: " + language + " is not supported.");

            return spellEngine["en"].Suggest(word);
        }

        public static List<String> GetSynonyms(string word, string language)
        {
            List<String> synonyms = new List<string>();
            if (!languages.ContainsKey(language)) throw new Exception("Language: " + language + " is not supported.");

            ThesResult meanings = spellEngine[language].LookupSynonyms(word, true);
            foreach (ThesMeaning meaning in meanings.Meanings)
            {
                synonyms.AddRange(meaning.Synonyms);
            }

            return synonyms.Distinct().ToList();
        }

        public static List<String> GetSuggestions(string word, string language)
        {
            List<String> synonyms = new List<string>();
            if (!languages.ContainsKey(language)) throw new Exception("Language: " + language + " is not supported.");

            return spellEngine[language].Suggest(word).ToList();
        }

        public static List<String> Stem(String word, String language)
        {
            if (!languages.ContainsKey(language)) throw new Exception("Language: " + language + " is not supported.");

            return spellEngine[language].Stem(word);
        }
    }
}
