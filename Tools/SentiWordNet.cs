using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public class SentiWordNet
    {
        private readonly Dictionary<string, double> sentilexicon = new Dictionary<string, double>();

        public SentiWordNet(String sourceFile)
        {
            if (!File.Exists(sourceFile))
                throw new IOException(string.Format("SentiWordNet file: '{0}' doesn't exist", sourceFile));

            Dictionary<string, Dictionary<int, double>> tempDictionary = new Dictionary<string, Dictionary<int, double>>();
            IStemmer stemmer = new StemmersNetStemmer(Languages.English);

            using (StreamReader reader = new StreamReader(sourceFile))
            {
                String line;
                int lineNumber = 0;

                // Read every line
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    if (!line.Trim().StartsWith("#"))
                    {
                        String[] lineData = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                        // Is it a valid line? Otherwise, through exception.
                        if (lineData.Length != 6) throw new Exception(string.Format("Incorrect tabulation format in file, line: {0}", lineNumber));

                        String posType = lineData[0];
                        //Calculate synset score as score = PosS - NegS
                        double synsetScore = double.Parse(lineData[2], CultureInfo.InvariantCulture) - double.Parse(lineData[3], CultureInfo.InvariantCulture);

                        // Get all Synset terms
                        String[] synTermsSplit = lineData[4].Split(' ');

                        // Go through all terms of current synset.
                        foreach (String synTermSplit in synTermsSplit)
                        {
                            // Get synterm and synterm rank
                            String[] synTermAndRank = synTermSplit.Split('#');
                            String synTerm = synTermAndRank[0].ToLower() + "#" + posType;

                            int synTermRank = int.Parse(synTermAndRank[1]);
                            // What we get here is a map of the type:
                            // term -> {score of synset#1, score of synset#2...}

                            // Add map to term if it doesn't have one
                            if (!tempDictionary.ContainsKey(synTerm))
                            {
                                tempDictionary.Add(synTerm, new Dictionary<int, double>());
                            }

                            // Add synset link to synterm
                            tempDictionary[synTerm].Add(synTermRank, synsetScore);
                        }
                    }
                }

                // Go through all the terms.
                foreach (KeyValuePair<string, Dictionary<int, double>> entry in tempDictionary)
                {

                    String word = entry.Key;

                    Dictionary<int, double> synSetScoreMap = entry.Value;

                    // Calculate weighted average. Weigh the synsets according to
                    // their rank.
                    // Score= 1/2*first + 1/3*second + 1/4*third ..... etc.
                    // Sum = 1/1 + 1/2 + 1/3 ...
                    double score = 0.0;
                    double sum = 0.0;
                    foreach (KeyValuePair<int, double> setScore in synSetScoreMap)
                    {

                        score += setScore.Value / (double)setScore.Key;
                        sum += 1.0 / (double)setScore.Key;
                    }
                    score /= sum;

                    sentilexicon.Add(word, score);
                }
            }
        }

        public double? GetPolarity(string term, string pos)
        {
            if (sentilexicon.ContainsKey(term + "#" + pos))
                return sentilexicon[term + "#" + pos];
            else
                return null;
        }
    }
}
