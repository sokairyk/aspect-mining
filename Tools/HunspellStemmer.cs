using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class HunspellStemmer : IStemmer
    {
        public Languages Language { get; set; }

        public HunspellStemmer(Languages lang)
        {
            Language = lang;
        }

        public string Stem(string word, string POS = null)
        {
            List<string> stemResults = HunspellChecker.Stem(word, Language.StringValue());
            return stemResults != null && stemResults.Count > 0 ? stemResults.First() : word;
        }
    }
}
