using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public interface IStemmer
    {
        Languages Language { get; set; } //Use ISO 639-1 Codes on implementations
        string Stem(string word, string POS = null);
    }
}
