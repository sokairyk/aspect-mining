using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Iveonik.Stemmers;

namespace Tools
{
    public class StemmersNetStemmer : IStemmer
    {
        private Languages language;
        public Languages Language
        {
            get { return language; }
            set
            {
                language = value;
                switch (language)
                {
                    //Add more languages from the DLL if you like
                    case Languages.English:
                        stemmer = new EnglishStemmer();
                        break;
                    default:
                        stemmer = null;
                        break;
                }
            }
        }

        private Iveonik.Stemmers.IStemmer stemmer;

        public StemmersNetStemmer(Languages lang)
        {
            Language = lang;
        }

        public string Stem(string word, string POS = null)
        {
            return stemmer.Stem(word);
        }
    }
}
