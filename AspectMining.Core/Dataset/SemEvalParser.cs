using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AspectMining.Core.Database;
using Tools;

namespace AspectMining.Core.Dataset
{
    public class SemEvalParser : IDatasetParser
    {
        List<string> aspects = new List<string>();

        public Product ParseDataset(string datasetFile)
        {
            Product productSummary = null;

            if (File.Exists(datasetFile))
            {
                SemEvalDataset dataset = XmlSerializationExtensions.FromXmlFile<SemEvalDataset>(datasetFile);

                FileInfo datasetFileInfo = new FileInfo(datasetFile);
                productSummary = new Product();
                productSummary.Title = datasetFileInfo.Name;

                Review defaultReview = new Review();
                defaultReview.Title = string.Format("Batch sentences for dataset: {0}", datasetFileInfo.Name);

                foreach (SemEvalDatasetSentence datasetSentence in dataset.Sentences)
                {
                    Sentence sentence = new Sentence();
                    sentence.DatasetTypeId = (int)DatasetParserType.SemEval;
                    sentence.Text = datasetSentence.Sentence;

                    ManualResults calculatedResults = new ManualResults();

                    foreach (SemEvalDatasetAspect datasetAspect in datasetSentence.Aspects)
                    {
                        SemEvalResultAspect resultAspect = new SemEvalResultAspect();
                        resultAspect.AspectTermText = datasetAspect.Term;

                        if (!aspects.Contains(datasetAspect.Term.ToLower()))
                            aspects.Add(datasetAspect.Term.ToLower());

                        resultAspect.Polarity = datasetAspect.Polarity;
                        resultAspect.OpinionStrengthFrom = datasetAspect.From;
                        resultAspect.OpinionStrengthTo = datasetAspect.To;

                        calculatedResults.SemEvalResultAspects.Add(resultAspect);
                    }

                    if (calculatedResults.SemEvalResultAspects.Count > 0)
                        sentence.ManualResults = XmlSerializationExtensions.ToXmlString(calculatedResults);

                    defaultReview.Sentence.Add(sentence);
                }

                productSummary.Review.Add(defaultReview);
            }

            //RunTest();

            return productSummary;
        }

        private void RunTest()
        {
            for (int i = 0; i < aspects.Count; i++)
            {
                for (int j = 0; j < aspects.Count; j++)
                {
                    if (i != j)
                    {
                        if (aspects[i].NormalizedLevenshteinDistance(aspects[j]) < GetSimilarityLimit(aspects[i], aspects[j]))
                            throw new Exception(string.Format("Manual results variation: {0} / {1}", aspects[i], aspects[j]));
                    }
                }
            }
        }

        private double GetSimilarityLimit(string termA, string termB)
        {
            int termLength = Math.Max(termA.Length, termB.Length);

            if (termLength < 8)
                return 0.15;
            if (termLength < 10)
                return 0.11;
            else
                return 0.1;
        }
    }

    [XmlRoot("dataset")]
    public class SemEvalDataset
    {
        [XmlArray("sentences")]
        [XmlArrayItem(ElementName = "sentence", Type = typeof(SemEvalDatasetSentence))]
        public List<SemEvalDatasetSentence> Sentences = new List<SemEvalDatasetSentence>();
    }

    [Serializable]
    [XmlRoot("sentence")]
    public class SemEvalDatasetSentence
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlElement("text")]
        public string Sentence;
        [XmlArray("aspectTerms")]
        [XmlArrayItem(ElementName = "aspectTerm", Type = typeof(SemEvalDatasetAspect))]
        public List<SemEvalDatasetAspect> Aspects = new List<SemEvalDatasetAspect>();
    }

    [Serializable]
    [XmlRoot("aspect")]
    public class SemEvalDatasetAspect
    {
        [XmlAttribute("term")]
        public string Term;
        [XmlAttribute("polarity")]
        public string Polarity;
        [XmlAttribute("from")]
        public int From;
        [XmlAttribute("to")]
        public int To;
    }
}
