using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JavaLibraries;
using Tools;

namespace AspectMining.Core.Database
{
    public partial class Sentence
    {
        private ChunkedText chunkedSentence = null;
        public ChunkedText ChunkedSentence
        {
            get
            {
                if (chunkedSentence == null && !String.IsNullOrEmpty(TextPOS))
                {
                    chunkedSentence = TextPOS.FromXmlString<ChunkedText>();
                }

                return chunkedSentence;
            }

            set
            {
                chunkedSentence = value;
                chunkedSentence.MakeSerializable();
                TextPOS = value.ToXmlString();
            }
        }

        public void RefreshChunkedSentence()
        {
            if (chunkedSentence != null)
                TextPOS = chunkedSentence.ToXmlString();
        }
    }

    public partial class PhraseProductMapping
    {
        public double GetFrequencyPercent(int sentenceCount)
        {
            return (Frequency * 100) / (double)sentenceCount;
        }
    }
}
