using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.process;
using Tools;

namespace JavaLibraries.StanfordNLP
{
    public class StanfordStemmer : IStemmer
    {
        public Languages Language { get; set; }

        public String Stem(String word, string POS)
        {
            return Morphology.stemStatic(word.ToLower(), POS).word();
        }
    }
}
