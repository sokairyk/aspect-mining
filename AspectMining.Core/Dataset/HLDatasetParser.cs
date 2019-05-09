using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspectMining.Core.Database;
using Tools;

namespace AspectMining.Core.Dataset
{
    public class HLDatasetParser : IDatasetParser
    {
        private const String ignoreLine = "*";
        private const String titleTag = "[t]";
        private const String reviewLineTag = "##";
        private const String apectDelimiter = ",";
        private const String aspectFeatureNotAppeared = "[u]";
        private const String aspectFeatureNotAppearedPronounResolutionNeeded = "[p]";
        private const String resultsRegexPattern = "(.+)\\[((\\+|-)[0-9])+\\](\\[u\\]|\\[p\\])?";


        public Product ParseDataset(String datasetFile)
        {
            Product productSummary = null;

            if (File.Exists(datasetFile))
            {
                using (StreamReader reader = new StreamReader(datasetFile))
                {
                    // Read every line
                    String line;
                    FileInfo datasetFileInfo = new FileInfo(datasetFile);
                    productSummary = new Product();
                    productSummary.Title = datasetFileInfo.Name;
                    Review review = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        // Ignore comments
                        if (!line.StartsWith(ignoreLine))
                        {
                        /*
                         * If it starts with a title tag we should add the current
                         * review item to the list (if any) and create a new one.
                         */
                            if (line.StartsWith(titleTag))
                            {
                                if (review != null)
                                {
                                    productSummary.Review.Add(review);
                                }

                                // Create a new review and set the title
                                review = new Review();
                                review.Title = line.Replace(titleTag, String.Empty);
                            }
                            else
                            {
                                // Parse the review line and add it to the current
                                // review item
                                Sentence sentence = ParseSentence(line);
                                if (review != null && sentence != null)
                                {
                                    review.Sentence.Add(sentence);
                                }
                            }
                        }
                    }

                    // Add the last review item (if any)
                    if (review != null)
                    {
                        productSummary.Review.Add(review);
                    }
                }
            }
            return productSummary;
        }

        private Sentence ParseSentence(string line)
        {
            Sentence sentence = null;
            ManualResults calculatedResults = new ManualResults();
            /*
             * If we have 2 parts the first contains results information and the
             * second the actual line. If there is only one (the first is empty) 
             * then we have only the review sentence available
             */
            String[] reviewLineParts = line.Split(reviewLineTag.ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
            if (reviewLineParts.Length > 0)
            {
                sentence = new Sentence();
                sentence.DatasetTypeId = (int)DatasetParserType.HuLiu;
                sentence.Text = reviewLineParts.Length == 2 ? reviewLineParts[1] : reviewLineParts[0];

                // Split to the aspect result delimiter
                String[] aspects = (reviewLineParts.Length == 2 ? reviewLineParts[0] : String.Empty).Split(apectDelimiter.ToCharArray());

                /*
                 * Pass each aspect to the regular expression matcher to extract
                 * information results from the matching groups
                 */
                foreach (String aspect in aspects)
                {
                    Regex aspectInfo = new Regex(resultsRegexPattern);
                    Match aspectMatch = aspectInfo.Match(aspect);

                    if (aspectMatch.Success)
                    {
                        HLResultAspect resultAspect = new HLResultAspect();
                        resultAspect.AspectTermText = aspectMatch.Groups[1].Value.Trim();
                        resultAspect.OpinionStrength = int.Parse(aspectMatch.Groups[2].Value);

                        if (aspectMatch.Groups[4].Value != String.Empty)
                        {
                            switch (aspectMatch.Groups[4].Value)
                            {
                                case aspectFeatureNotAppeared:
                                    resultAspect.NotAppeared = true;
                                    break;
                                case aspectFeatureNotAppearedPronounResolutionNeeded:
                                    resultAspect.PronounResolutionNeeded = true;
                                    break;
                            }
                        }

                        calculatedResults.HLResultAspects.Add(resultAspect);
                    }
                }

                if (calculatedResults.HLResultAspects.Count > 0)
                {
                    sentence.ManualResults = XmlSerializationExtensions.ToXmlString(calculatedResults);
                }
            }

            return sentence;
        }
    }
}
