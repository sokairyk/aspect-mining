using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AspectMining.Core.Database;
using JavaLibraries;
using JavaLibraries.ApacheCommons;
using JavaLibraries.OpenNLP;
using JavaLibraries.StanfordNLP;
using Microsoft.SqlServer.Server;
using Tools;
using Tools.Combinations;

namespace AspectMining.Core
{
    public class AspectHandler
    {
        //NOTE: In almost every function certain null reference checks were intentionally left
        //      unimplemented in order to test the logical consistency of the database records.

        //String list for fuzzy matching
        private List<String> allTerms;
        //Class params
        private DatabaseHandler dbHandler;
        private AspectMiningContext context;
        private List<Sentence> allSentences;
        private OpenNLPChunker chunker;
        private IStemmer stemmer = new StemmersNetStemmer(Languages.English); /* StemmersNet implementation is prefered as it stems
                                                                                 to the root of the word and allows better matching */
        private SentiWordNet sentiWordNet;
        private int productId;
        public int ProductId
        {
            get { return productId; }
            set
            {
                productId = value;
                allSentences = (from u in context.Sentence where u.Review.ProductId == value select u).ToList();
                allTerms = (from u in context.Term select u.Text).ToList();
            }
        }

        //Algorithm params
        private int? _maxAspectSize;
        private int? _maxAspectDistance;
        private int? _minAspectPSupport;
        private int? _minAspectCompactness;
        private double? _minFrequency;
        private double? _maxExperimentalFrequency;
        private int? _maxOpinionWordDistance;
        private int? maxPolarityWordDistance;

        public int? MaxAspectSize { get { return _maxAspectSize ?? MAX_ASPECT_SIZE; } set { _maxAspectSize = value; } }
        public int? MaxAspectDistance { get { return _maxAspectDistance ?? MAX_ASPECT_DISTANCE; } set { _maxAspectDistance = value; } }
        public int? MinAspectPSupport { get { return _minAspectPSupport ?? MIN_ASPECT_PSUPPORT; } set { _minAspectPSupport = value; } }
        public int? MinAspectCompactness { get { return _minAspectCompactness ?? MIN_ASPECT_COMPACTNESS; } set { _minAspectCompactness = value; } }
        public double? MinFrequency { get { return _minFrequency ?? MIN_FREQUENCY; } set { _minFrequency = value; } }
        public double? MaxExperimentalFrequency { get { return _maxExperimentalFrequency ?? MAX_EXPERIMENTAL_FREQUENCY; } set { _maxExperimentalFrequency = value; } }
        public int? MaxOpinionWordDistance { get { return _maxOpinionWordDistance ?? MAX_OPINION_WORD_DISTANCE; } set { _maxOpinionWordDistance = value; } }
        public int? MaxPolarityWordDistance { get { return maxPolarityWordDistance ?? MAX_POLARITY_WORD_DISTANCE; } set { maxPolarityWordDistance = value; } }

        private const int MAX_ASPECT_SIZE = 3;
        private const int MAX_ASPECT_DISTANCE = 3;
        private const int MIN_ASPECT_PSUPPORT = 2;
        private const int MIN_ASPECT_COMPACTNESS = 2;
        private const double MIN_FREQUENCY = 1;
        private const double MAX_EXPERIMENTAL_FREQUENCY = 1.5;
        private const int MAX_OPINION_WORD_DISTANCE = 4;
        private const int MAX_POLARITY_WORD_DISTANCE = 7777;

        public AspectHandler(DatabaseHandler handler, int id)
        {
            dbHandler = handler;
            context = handler.Context;
            ProductId = id;
        }

        #region Main Actions

        public void ApplyPOSTagging(string tokenizerModelPath, string posModelPath, string chunkerModelPath)
        {
            chunker = new OpenNLPChunker(tokenizerModelPath, posModelPath, chunkerModelPath);

            foreach (Sentence sentence in allSentences)
                sentence.ChunkedSentence = chunker.Process(sentence.Text);

            context.SubmitChanges();
        }

        public void ExtractAspects()
        {
            foreach (Sentence sentence in allSentences)
            {
                //For each sentence loop all group tags
                foreach (ChunkedGroup chunkedGroup in sentence.ChunkedSentence.Groups)
                {
                    //If the group tag is phrase of the type we specified
                    if (chunkedGroup.Tag.ParseAsStringEnum<DistinctChunkTags>() == DistinctChunkTags.NounPhrase)
                    {
                        //Get all the related valid terms from the phrase (e.g. nouns from a noun phrase)
                        List<ChunkedTerm> targetTermChunks = chunkedGroup.Terms.FindAll(a => StaticMappings.ChunkPOSMapping[DistinctChunkTags.NounPhrase].Contains(a.POS)
                                                                                             || StaticMappings.ChunkPOSMapping[DistinctChunkTags.AdjectivePhrase].Contains(a.POS));

                        //Extract the phrase
                        ExtractPhrases(sentence, targetTermChunks, chunkedGroup, DistinctChunkTags.NounPhrase, MaxAspectSize);
                    }
                }
            }
        }

        //This method uses the ManualResults class to add the manual aspect results to the phrase - product mapping table
        //in order to compare
        public void ExtractOptimalResults()
        {
            foreach (Sentence sentence in allSentences)
            {
                if (sentence.ManualResults != null)
                {
                    ManualResults results = XmlSerializationExtensions.FromXmlString<ManualResults>(sentence.ManualResults);

                    switch ((DatasetParserType)Enum.ToObject(typeof(DatasetParserType), sentence.DatasetTypeId))
                    {
                        case DatasetParserType.HuLiu:
                            foreach (HLResultAspect resultAspect in results.HLResultAspects)
                            {
                                List<ChunkedTerm> chunkedTerms = new List<ChunkedTerm>();

                                ChunkedText chunkedText = chunker.Process(resultAspect.AspectTermText);
                                chunkedText.MakeSerializable();
                                foreach (ChunkedGroup group in chunkedText.Groups)
                                    chunkedTerms.AddRange(group.Terms);

                                bool? polarity = resultAspect.OpinionStrength == 0
                                                 ? (bool?)null
                                                 : resultAspect.OpinionStrength > 0;

                                Phrase processedPhrase = ProcessPhrase(ProcessTerms(chunkedTerms), null, true, polarity);
                                if (processedPhrase != null)
                                    CreatePolaritySentenceMapping(processedPhrase.Id, sentence.Id, polarity, true, true);
                            }
                            break;
                        case DatasetParserType.SemEval:
                            foreach (SemEvalResultAspect resultAspect in results.SemEvalResultAspects)
                            {
                                List<ChunkedTerm> chunkedTerms = new List<ChunkedTerm>();

                                ChunkedText chunkedText = chunker.Process(resultAspect.AspectTermText);
                                chunkedText.MakeSerializable();
                                foreach (ChunkedGroup group in chunkedText.Groups)
                                    chunkedTerms.AddRange(group.Terms);

                                bool? polarity = resultAspect.Polarity == "neutral"
                                                 ? (bool?)null
                                                 : resultAspect.Polarity == "positive";

                                Phrase processedPhrase = ProcessPhrase(ProcessTerms(chunkedTerms), null, true, polarity);
                                if (processedPhrase != null)
                                    CreatePolaritySentenceMapping(processedPhrase.Id, sentence.Id, polarity, true, true);
                            }
                            break;
                        default:
                            throw new Exception("Unimplemented or undefined dataset parser type");
                    }
                }
            }
        }

        //Generate additional aspects
        public void GenerateAspects()
        {
            //Get the most frequent aspects usring the Apriori rule assosiation mining algorithm
            List<List<Phrase>> frequentAspectCombinations = Apriori();

            //Generate 2 ... n term aspects (2-term and 3-term in our case)
            for (int i = 2; i <= MaxAspectSize; i++)
            {
                //Loop all sentences
                foreach (Sentence sentence in allSentences)
                    GenerateAspects(sentence, i, frequentAspectCombinations);
            }
        }

        public void ExtractOpinionWords()
        {
            //Iterate all sentences
            foreach (Sentence sentence in allSentences)
            {
                //Get every aspect in each sentence
                List<Phrase> sentenceAspects = sentence.SentencePhraseMapping
                                                       .Select(a => a.Phrase)
                                                       .Where(a => a.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                                   && a.PhraseProductMapping.SingleOrDefault(b => b.ProductId == productId
                                                                                                                  && !b.IsManual) != null)
                                                       .ToList();

                foreach (Phrase aspect in sentenceAspects)
                {
                    //Check if its frequent
                    var isFrequent = aspect.PhraseProductMapping
                                           .SingleOrDefault(b => b.ProductId == productId && !b.IsManual)
                                           .GetFrequencyPercent(allSentences.Count) > MinFrequency;

                    //Get the nearest ajective group / adjective on the frequent aspect
                    List<ChunkedGroup> opinionGroups = sentence.ChunkedSentence.GetNearestGroupById(aspect.Id, aspect.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.AdjectivePhrase, (int)MaxOpinionWordDistance);

                    //For adjective group we found
                    foreach (ChunkedGroup chunkedGroup in opinionGroups)
                    {
                        //Get all the related valid terms from the phrase (e.g. nouns from a noun phrase)
                        List<ChunkedTerm> targetTermChunks = chunkedGroup.Terms.FindAll(a => StaticMappings.ChunkPOSMapping[DistinctChunkTags.AdjectivePhrase].Contains(a.POS));

                        //Extract the phrases
                        List<Phrase> opinionPhrases = ExtractPhrases(sentence, targetTermChunks, chunkedGroup, DistinctChunkTags.AdjectivePhrase);
                        //Mark those that are frequent to be used later in the infrequent aspect extraction
                        foreach (Phrase opinionPhrase in opinionPhrases)
                            if (isFrequent)
                                opinionPhrase.PhraseProductMapping.SingleOrDefault(a => a.ProductId == productId && !a.IsManual).IsFrequent = true;
                    }
                }
            }
        }

        //This calculates the term order in multi-term phrases
        //based on frequency of appearance
        public void CalculateTermOrder(DistinctChunkTags phraseType)
        {
            //Get all phrases for this product
            List<PhraseProductMapping> allPhraseProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == productId && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(phraseType)
                                                                   select u).ToList();

            foreach (PhraseProductMapping phraseProductMapping in allPhraseProductMappings)
            {
                //A single term aspect or a manual result needs no calculation
                if (phraseProductMapping.IsManual || phraseProductMapping.Phrase.PhraseTermMapping.Count == 1)
                {
                    phraseProductMapping.PhraseText = phraseProductMapping.Phrase.Text;
                    phraseProductMapping.PhrasePreprocessedText = phraseProductMapping.Phrase.PreprocessedText;
                }
                else
                {
                    //Set a dictionary with the different form of appearances of this aspect
                    Dictionary<string, int> phraseTermAppearances = new Dictionary<string, int>();
                    Dictionary<string, int> preprocessedPhraseTermAppearances = new Dictionary<string, int>();

                    //Select all sentence-phrase-term mapping for this aspect on current product
                    List<SentencePhraseTermMapping> sentencePhraseTermMappings = (from u in context.SentencePhraseTermMapping
                                                                                  where u.SentencePhraseMapping.Sentence.Review.ProductId == productId &&
                                                                                        phraseProductMapping.Phrase.PhraseTermMapping.Select(a => a.Id).Contains(u.PhraseTermId)
                                                                                  select u).ToList();
                    //Get all distinct sentence ids
                    List<int> sentenceIds = sentencePhraseTermMappings.Select(a => a.SentencePhraseMapping.SentenceId).Distinct().ToList();

                    foreach (int sentenceId in sentenceIds)
                    {
                        //Get a dictionary with the term-term order pair per sentence
                        var phraseTerms = (from u in sentencePhraseTermMappings.Where(a => a.SentencePhraseMapping.SentenceId == sentenceId)
                                           select new { u.PhraseTermMapping.Term.Text, u.PhraseTermMapping.Term.PreprocessedText, u.TermOrder });

                        //Sort by term order
                        phraseTerms = phraseTerms.OrderBy(a => a.TermOrder);

                        //Form the phrase by term order and count its frequency in the dictionary
                        string aspectAppearence = string.Join(" ", phraseTerms.Select(a => a.Text));
                        string aspectPreprocessedAppearence = string.Join(" ", phraseTerms.Select(a => a.PreprocessedText));

                        if (phraseTermAppearances.ContainsKey(aspectAppearence))
                        {
                            phraseTermAppearances[aspectAppearence]++;
                            preprocessedPhraseTermAppearances[aspectPreprocessedAppearence]++;
                        }
                        else
                        {
                            phraseTermAppearances.Add(aspectAppearence, 1);
                            preprocessedPhraseTermAppearances.Add(aspectPreprocessedAppearence, 1);
                        }
                    }

                    //Finally set the most frequent form of appearance
                    phraseProductMapping.PhraseText = phraseTermAppearances.OrderByDescending(a => a.Value).FirstOrDefault().Key;
                    phraseProductMapping.PhrasePreprocessedText = preprocessedPhraseTermAppearances.OrderByDescending(a => a.Value).FirstOrDefault().Key;
                }

                //Save
                context.SubmitChanges();
            }
        }

        //Apply fuzzy matching on aspects
        public void ApplyAspectFuzzyMatching(DistinctChunkTags phraseType)
        {
            //Get all phrase - product mappings
            List<PhraseProductMapping> allPhraseProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == productId && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(phraseType)
                                                                   select u).ToList();

            //Create a merge/remove mapping: Key is the mapping to be removed, Value is the mapping that is merged with the removed one
            Dictionary<PhraseProductMapping, PhraseProductMapping> mergeRemoveMapping = new Dictionary<PhraseProductMapping, PhraseProductMapping>();

            //Iterate the mappings in such order to compare every object in the list with each other
            for (int i = 0; i < allPhraseProductMappings.Count; i++)
            {
                //Skip the last one
                if (i != allPhraseProductMappings.Count - 1)
                {
                    for (int j = i + 1; j < allPhraseProductMappings.Count; j++)
                    {
                        // Check the Levenshtein distance of phrases with different term count to get cases such as auto-focus --> auto focus
                        // NOTE: The PhraseText/PhrasePreprocessedText field we check here is the one in the ProductPhraseMapping table which is set after the CalculateTermOrder execution.
                        //       If we skip that step we could check the PhraseText/PhrasePreprocessedText field in the Phrase entity.
                        if (allPhraseProductMappings[i].Phrase.PhraseTermMapping.Count != allPhraseProductMappings[j].Phrase.PhraseTermMapping.Count &&
                            (allPhraseProductMappings[i].PhrasePreprocessedText.ToLower()).NormalizedLevenshteinDistance(allPhraseProductMappings[j].PhrasePreprocessedText.ToLower()) <= GetSimilarityLimit(allPhraseProductMappings[i].PhrasePreprocessedText.ToLower(), allPhraseProductMappings[j].PhrasePreprocessedText.ToLower()))
                        {
                            PhraseProductMapping mergedMapping, removedMapping;

                            //If 2 manual results are matched...
                            if (allPhraseProductMappings[i].IsManual && allPhraseProductMappings[j].IsManual)
                                throw new Exception(String.Format("Phrases '{0}' and '{1}' from manual results are matched. This shouldn't happen", allPhraseProductMappings[i].PhrasePreprocessedText, allPhraseProductMappings[j].PhrasePreprocessedText));

                            //Select the phrase version to keep and phrase version to discard based on the frequency of appearance,
                            //and/or the state of deletion
                            if (!allPhraseProductMappings[i].IsManual && !allPhraseProductMappings[j].IsManual)
                            {
                                if (allPhraseProductMappings[i].Frequency > allPhraseProductMappings[j].Frequency)
                                {
                                    mergedMapping = allPhraseProductMappings[i];
                                    removedMapping = allPhraseProductMappings[j];
                                }
                                else
                                {
                                    mergedMapping = allPhraseProductMappings[j];
                                    removedMapping = allPhraseProductMappings[i];
                                }
                            }
                            else
                            {
                                if (allPhraseProductMappings[i].IsManual)
                                {
                                    mergedMapping = allPhraseProductMappings[j];
                                    removedMapping = allPhraseProductMappings[i];
                                }
                                else
                                {
                                    mergedMapping = allPhraseProductMappings[i];
                                    removedMapping = allPhraseProductMappings[j];
                                }
                            }

                            //If the removed mapping has already been merged (meaning it's pending to be deleted) skip to the next
                            if (mergeRemoveMapping.ContainsKey(removedMapping))
                                continue;

                            //While the merged mapping is previously removed get the replaced one from the mapping dictionary
                            //This way we catch chain replacements
                            while (mergeRemoveMapping.ContainsKey(mergedMapping))
                                mergedMapping = mergeRemoveMapping[mergedMapping];

                            //Add the mapping to the dictionary
                            mergeRemoveMapping.Add(removedMapping, mergedMapping);

                            //Merge the frequency ONLY if they're BOTH NON-manual entries
                            if (!mergedMapping.IsManual && !removedMapping.IsManual)
                                mergedMapping.Frequency += removedMapping.Frequency;

                            //Change the sentence aspect mapping of the removed aspect version to the merged one
                            List<SentencePhraseMapping> sentenceAspectMappings = (from u in context.SentencePhraseMapping
                                                                                  where u.Sentence.Review.ProductId == productId
                                                                                        && u.Phrase == removedMapping.Phrase
                                                                                  select u).ToList();
                            List<int> existingPhraseTermMappingIds = new List<int>();
                            foreach (SentencePhraseMapping sentenceAspectMapping in sentenceAspectMappings)
                            {
                                SentencePhraseMapping existingPhraseMapping = sentenceAspectMapping.Sentence.SentencePhraseMapping.SingleOrDefault(a => a.Phrase == mergedMapping.Phrase);

                                if (existingPhraseMapping != null)
                                {
                                    existingPhraseTermMappingIds.AddRange(sentenceAspectMapping.SentencePhraseTermMapping.Select(a => a.Id));
                                    context.SentencePhraseTermMapping.DeleteAllOnSubmit(sentenceAspectMapping.SentencePhraseTermMapping);
                                    context.SentencePhraseMapping.DeleteOnSubmit(sentenceAspectMapping);
                                }
                                else
                                {
                                    sentenceAspectMapping.Phrase = mergedMapping.Phrase;
                                }
                            }

                            //Change the sentence aspect term mapping of the removed aspect version to the merged one
                            for (int z = 0; z < removedMapping.Phrase.PhraseTermMapping.Count; z++)
                            {
                                List<SentencePhraseTermMapping> sentenceAspectTermMappings = (from u in context.SentencePhraseTermMapping
                                                                                              where u.SentencePhraseMapping.Sentence.Review.ProductId == productId
                                                                                                    && !existingPhraseTermMappingIds.Contains(u.Id)
                                                                                                    && u.PhraseTermMapping == removedMapping.Phrase.PhraseTermMapping[z]
                                                                                              select u).ToList();

                                foreach (SentencePhraseTermMapping sentenceAspectTermMapping in sentenceAspectTermMappings)
                                {
                                    if (z < mergedMapping.Phrase.PhraseTermMapping.Count)
                                    {
                                        sentenceAspectTermMapping.PhraseTermMapping = mergedMapping.Phrase.PhraseTermMapping[z];
                                    }
                                    else
                                    {
                                        context.SentencePhraseTermMapping.DeleteOnSubmit(sentenceAspectTermMapping);
                                    }
                                }
                            }

                            for (int z = mergedMapping.Phrase.PhraseTermMapping.Count - 1; z >= removedMapping.Phrase.PhraseTermMapping.Count; z--)
                            {
                                List<SentencePhraseTermMapping> sentenceAspectTermMappings = (from u in context.SentencePhraseTermMapping
                                                                                              where u.SentencePhraseMapping.Sentence.Review.ProductId == productId
                                                                                                    && !existingPhraseTermMappingIds.Contains(u.Id)
                                                                                                    && u.PhraseTermMapping == removedMapping.Phrase.PhraseTermMapping.LastOrDefault()
                                                                                              select u).ToList();

                                foreach (SentencePhraseTermMapping sentenceAspectTermMapping in sentenceAspectTermMappings)
                                {
                                    SentencePhraseTermMapping newSentenceAspectTermMapping = new SentencePhraseTermMapping();
                                    newSentenceAspectTermMapping.SentencePhraseMapping = sentenceAspectTermMapping.SentencePhraseMapping;
                                    newSentenceAspectTermMapping.TermOrder = sentenceAspectTermMapping.TermOrder + (mergedMapping.Phrase.PhraseTermMapping.Count - z);
                                    newSentenceAspectTermMapping.PhraseTermMapping = mergedMapping.Phrase.PhraseTermMapping[z];
                                    context.SentencePhraseTermMapping.InsertOnSubmit(newSentenceAspectTermMapping);
                                }
                            }

                            //If the removed mapping is manual do NOT delete it
                            if (!removedMapping.IsManual)
                                context.PhraseProductMapping.DeleteOnSubmit(removedMapping);
                        }
                    }
                }
            }

            context.SubmitChanges();
        }

        //Calculate the compactness of every aspect in each sentence
        public void CalculateCompactness()
        {
            //Iterate all sentences
            foreach (Sentence sentence in allSentences)
            {
                //Get all aspects (noun phrases) that have a term count > 1 and are not manual
                List<Phrase> sentenceAspects = sentence.SentencePhraseMapping
                                                       .Select(a => a.Phrase)
                                                       .Where(a => a.PhraseTermMapping.Count > 1
                                                              && a.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                              && !a.PhraseProductMapping.FirstOrDefault(b => b.ProductId == productId).IsManual)
                                                       .ToList();

                foreach (Phrase sentenceAspect in sentenceAspects)
                {
                    //Get the terms order for this aspect in the specific sentence
                    List<int?> termOrder;
                    termOrder = sentence.SentencePhraseMapping
                                        .SingleOrDefault(a => a.Phrase == sentenceAspect)
                                        .SentencePhraseTermMapping
                                        .Select(a => a.TermOrder)
                                        .ToList();

                    bool exeedsTermDistance = false;

                    //Remove nulls and sort the list
                    termOrder.RemoveAll(a => a == null);
                    termOrder = termOrder.OrderBy(a => a).ToList();

                    //Check the distance between every sequential term pair
                    for (int i = 0; i < termOrder.Count - 1; i++)
                        if (Math.Abs((int)termOrder[i] - (int)termOrder[i + 1]) > MaxAspectDistance)
                        {
                            exeedsTermDistance = true;
                            break;
                        }

                    //If the distance is valid
                    if (!exeedsTermDistance)
                    {
                        //Get the aspect - product mapping
                        PhraseProductMapping aspectProductMapping = (from u in context.PhraseProductMapping
                                                                     where u.Phrase == sentenceAspect
                                                                           && u.ProductId == productId
                                                                           && !u.IsManual
                                                                     select u).SingleOrDefault();
                        if (aspectProductMapping != null)
                            //and increase the compactness
                            aspectProductMapping.Compactness = aspectProductMapping.Compactness == null ? 1 : aspectProductMapping.Compactness + 1;
                        else
                            throw new Exception(string.Format("Phrase - Product mapping not found for phrase: {0}", sentenceAspect.Text));
                    }
                }
            }
            context.SubmitChanges();
        }

        //Calculate P-Support
        public void CalculatePSupport()
        {
            //Get all non-manual aspects terms
            List<Phrase> allAspectPhrases = (from u in context.PhraseProductMapping
                                             where u.ProductId == ProductId
                                                   && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                   && !u.IsManual
                                             select u.Phrase).ToList();

            //Use a single connnection to access temp tables
            using (SqlConnection cnn = new SqlConnection(dbHandler.ConnectionString))
            {
                cnn.Open();
                string tempTableQuery = @"
IF OBJECT_ID('tempdb..#TempPhrase') IS NOT NULL 
DROP TABLE #TempPhrase;

CREATE TABLE #TempPhrase
(
	[Id] [int],
	[TermCount] [int]
);

INSERT INTO #TempPhrase
SELECT p.Id, COUNT(m.Id) AS TermCount
FROM PhraseTermMapping m
     LEFT JOIN Phrase p ON m.PhraseId = p.Id
     LEFT JOIN PhraseProductMapping ppm ON ppm.PhraseId = p.Id
WHERE p.POSId = {1}
      AND ppm.IsManual = 0 AND ppm.ProductId = {0}
GROUP BY p.Id;";

                tempTableQuery = string.Format(tempTableQuery, productId, StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase));

                dbHandler.ExecuteNonQuery(tempTableQuery, null, cnn);

                foreach (Phrase aspectPhrase in allAspectPhrases)
                {
                    //Get all the sentences that contain the current aspect
                    int pSupport = 0;
                    foreach (
                        Sentence sentence in
                            aspectPhrase.SentencePhraseMapping.Where(a => a.Sentence.Review.ProductId == ProductId)
                                .Select(a => a.Sentence)
                                .Distinct())
                    {
                        //The following query checks if the specified aspect phrase is
                        //contained in a superset aspect phrase of the same sentence
                        String aspectQuery = @"
SELECT p.id
FROM Phrase p
     LEFT JOIN PhraseTermMapping m ON p.Id = m.PhraseId
	 LEFT JOIN SentencePhraseMapping s ON p.Id = s.PhraseId
     LEFT JOIN #TempPhrase tp ON tp.Id = m.PhraseId
	 LEFT JOIN Term t ON t.Id = m.TermId
WHERE p.POSId = {3} AND t.POSId = {4}
      AND t.Id IN ({1}) AND tp.TermCount >= {2} AND s.SentenceId = {0}
GROUP BY p.Id HAVING COUNT(p.Id) >= {2}";

                        aspectQuery = String.Format(aspectQuery, sentence.Id,
                            String.Join(", ", aspectPhrase.PhraseTermMapping.Select(a => a.TermId)),
                            aspectPhrase.PhraseTermMapping.Count,
                            StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase),
                            StaticMappings.GetDBPOSTagId(POSTags.NounSingularOrMass));

                        DataTable result = dbHandler.ExecuteDataTable(aspectQuery, null, cnn);

                        //If we have a single result it means that the aspect is not
                        //contained (the aspect id returned is the one we currently check)
                        if (result.Rows.Count == 1) pSupport++;
                    }

                    //Update the aspect - product mapping psupport value
                    PhraseProductMapping aspectProductMapping = (from u in context.PhraseProductMapping
                                                                 where u.Phrase == aspectPhrase && u.ProductId == ProductId && !u.IsManual
                                                                 select u).SingleOrDefault();

                    if (aspectProductMapping != null)
                    {
                        aspectProductMapping.PSupport = pSupport;
                        context.SubmitChanges();
                    }
                    else
                        throw new Exception("Aspect - Product mapping not found.");
                }

                cnn.Close();
            }
        }

        #region Pruning Functions

        public void PruneAspects()
        {
            PruneByPSupport();
            PruneByCompactness();
            ExtractInfrequentAspects();
        }

        public void PruneByFrequency()
        {
            //Get all non manual noun phrases
            List<PhraseProductMapping> allAspectProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == ProductId
                                                                   && !u.IsManual && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                                   select u).ToList();

            foreach (PhraseProductMapping aspectProductMapping in allAspectProductMappings)
            {
                //Prune aspects by frequency
                if (aspectProductMapping.GetFrequencyPercent(allSentences.Count) < MinFrequency)
                    aspectProductMapping.IsPruned = true;
            }

            context.SubmitChanges();
        }

        public void PruneByCompactness()
        {
            //Get all non manual noun phrases
            List<PhraseProductMapping> allAspectProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == ProductId
                                                                   && !u.IsManual && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                                   select u).ToList();

            foreach (PhraseProductMapping aspectProductMapping in allAspectProductMappings)
            {
                //Prune multi terms aspects by compactness
                if (aspectProductMapping.Phrase.PhraseTermMapping.Count > 1 && aspectProductMapping.Compactness < MinAspectCompactness)
                    aspectProductMapping.IsPruned = true;
            }

            context.SubmitChanges();
        }

        public void PruneByPSupport()
        {
            //Get all non manual noun phrases
            List<PhraseProductMapping> allAspectProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == ProductId
                                                                   && !u.IsManual && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                                   select u).ToList();

            foreach (PhraseProductMapping aspectProductMapping in allAspectProductMappings)
            {
                //Prune aspects by p-support
                if (aspectProductMapping.PSupport < MinAspectPSupport && aspectProductMapping.GetFrequencyPercent(allSentences.Count) < MaxExperimentalFrequency)
                    aspectProductMapping.IsPruned = true;
            }

            context.SubmitChanges();
        }

        public void ResetPrunning()
        {
            //Get all non manual noun phrases
            List<PhraseProductMapping> allAspectProductMappings = (from u in context.PhraseProductMapping
                                                                   where u.ProductId == ProductId
                                                                   && !u.IsManual && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                                   select u).ToList();

            foreach (PhraseProductMapping aspectProductMapping in allAspectProductMappings)
                aspectProductMapping.IsPruned = false;

            context.SubmitChanges();
        }

        #endregion

        public void ExtractInfrequentAspects()
        {
            List<Term> allOpinionWords = (from u in context.PhraseProductMapping
                                          where u.ProductId == productId && !u.IsManual && u.IsFrequent
                                          select u.Phrase.PhraseTermMapping
                                                         .Select(a => a.Term)
                                                         .Where(a => a.POSId == StaticMappings.GetDBPOSTagId(POSTags.Adjective)))
                                         .Where(a => a.Count() == 1)
                                         .ToList()
                                         .Select(a => a.ElementAt(0))
                                         .ToList();

            //Iterate all sentences
            foreach (Sentence sentence in allSentences)
            {
                //If the sentence doesn't contain any frequent aspects
                if (sentence.SentencePhraseMapping
                            .Where(a => a.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                        && a.Phrase.PhraseProductMapping.SingleOrDefault(b => b.ProductId == productId && !b.IsManual) != null)
                            .Select(a => a.Phrase)
                            .All(a => a.PhraseProductMapping
                                       .SingleOrDefault(b => b.ProductId == productId && !b.IsManual)
                                       .GetFrequencyPercent(allSentences.Count) <= MinFrequency))
                {
                    //But contains any opinion words
                    List<Phrase> opinionPhrases = sentence.SentencePhraseMapping
                                                          .Select(a => a.Phrase)
                                                          .Where(a => a.PhraseTermMapping.Any(b => allOpinionWords.Contains(b.Term)))
                                                          .ToList();

                    //Iterate all opinion words and extract the nearby nouns/noun phrases
                    foreach (Phrase opinionPhrase in opinionPhrases)
                    {
                        List<ChunkedGroup> opinionGroups = sentence.ChunkedSentence.GetNearestGroupById(opinionPhrase.Id, opinionPhrase.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.NounPhrase, (int)MaxOpinionWordDistance);

                        //For adjective group we found
                        foreach (ChunkedGroup chunkedGroup in opinionGroups)
                        {
                            //Get all the related valid terms from the phrase (e.g. nouns from a noun phrase)
                            List<ChunkedTerm> targetTermChunks = chunkedGroup.Terms.FindAll(a => StaticMappings.ChunkPOSMapping[DistinctChunkTags.NounPhrase].Contains(a.POS));

                            //Extract the phrase
                            List<Phrase> extractetedPhrases = ExtractPhrases(sentence, targetTermChunks, chunkedGroup, DistinctChunkTags.NounPhrase, MaxAspectSize);

                            //Unprune extracted aspects
                            foreach (Phrase extractetedPhrase in extractetedPhrases)
                                extractetedPhrase.PhraseProductMapping.SingleOrDefault(a => a.ProductId == productId && !a.IsManual).IsPruned = false;
                        }
                    }
                }
            }
        }

        public void CalculatePolarity()
        {
            if (sentiWordNet == null) throw new Exception("SentiWordNet is not assigned");

            //Get all valid calculated aspects
            List<PhraseProductMapping> validAspects = (from u in context.PhraseProductMapping
                                                       where u.ProductId == productId
                                                             && !u.IsManual
                                                             && u.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase)
                                                       select u).ToList();

            foreach (PhraseProductMapping aspectProductMapping in validAspects)
            {
                foreach (SentencePhraseMapping sentencePhraseMapping in aspectProductMapping.Phrase.SentencePhraseMapping.Where(a => a.Sentence.Review.ProductId == productId))
                {
                    List<ChunkedGroup> opinionGroups = sentencePhraseMapping.Sentence.ChunkedSentence.GetNearestGroupById(aspectProductMapping.PhraseId, aspectProductMapping.Phrase.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.AdjectivePhrase, (int)MaxPolarityWordDistance);
                    opinionGroups.AddRange(sentencePhraseMapping.Sentence.ChunkedSentence.GetNearestGroupById(aspectProductMapping.PhraseId, aspectProductMapping.Phrase.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.NounPhrase, (int)MaxPolarityWordDistance));
                    opinionGroups.AddRange(sentencePhraseMapping.Sentence.ChunkedSentence.GetNearestGroupById(aspectProductMapping.PhraseId, aspectProductMapping.Phrase.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.VerbPhrase, (int)MaxPolarityWordDistance));
                    opinionGroups.AddRange(sentencePhraseMapping.Sentence.ChunkedSentence.GetNearestGroupById(aspectProductMapping.PhraseId, aspectProductMapping.Phrase.PhraseTermMapping.Select(a => a.TermId).ToArray(), DistinctChunkTags.AdverbialPhrase, (int)MaxPolarityWordDistance));

                    opinionGroups = opinionGroups.Distinct().ToList();

                    double? polarityScore = null;

                    foreach (ChunkedGroup opinionGroup in opinionGroups)
                    {
                        double? termScore;
                        string sentiWordNetPOS;

                        //First check if whole phrase is found
                        sentiWordNetPOS = StaticMappings.SentiWordNetPOSMapping.ContainsKey(opinionGroup.Tag)
                                ? StaticMappings.SentiWordNetPOSMapping[opinionGroup.Tag].StringValue()
                                : String.Empty;
                        termScore = sentiWordNet.GetPolarity(string.Join("_", opinionGroup.Terms.Select(a => a.Text)).ToLower(), sentiWordNetPOS);

                        if (termScore != null)
                        {
                            if (polarityScore == null) polarityScore = termScore;
                            else polarityScore += termScore;
                        }
                        else
                        {
                            //Else check each term
                            foreach (ChunkedTerm opinionTerm in opinionGroup.Terms)
                            {
                                sentiWordNetPOS = StaticMappings.SentiWordNetPOSMapping.ContainsKey(opinionTerm.POS)
                                    ? StaticMappings.SentiWordNetPOSMapping[opinionTerm.POS].StringValue()
                                    : String.Empty;

                                termScore = sentiWordNet.GetPolarity(opinionTerm.Text.ToLower(), sentiWordNetPOS);

                                if (termScore != null)
                                {
                                    if (polarityScore == null) polarityScore = termScore;
                                    else polarityScore += termScore;
                                }
                            }
                        }
                    }

                    if (polarityScore != null)
                    {
                        //Revert polarity if negation is found in the sentence
                        //and polarity isn't neutral
                        string negationRegexPattern = "(no|not|isnt|isn\'t|isn;t|don\'t|won\'t|couldn\'t|can\'t|didn\'t|not|didnt|didn;t'|dont|don;t|don t|won t|can;t|cant|can t|doesn\'t|doesnt|doesn t|doesn;t|cannot|did not|not)\b";
                        foreach (ChunkedGroup opinionGroup in opinionGroups)
                            if (Regex.IsMatch(string.Join(" ", opinionGroup.Terms.Select(a => a.Text)), negationRegexPattern))
                            {
                                polarityScore *= -1;
                                break;
                            }

                        if (polarityScore == 0)
                            aspectProductMapping.NeutralReferences++;
                        else if (polarityScore > 0)
                            aspectProductMapping.PositiveReferences++;
                        else
                            aspectProductMapping.NegativeReferences++;

                        bool? polarity = polarityScore == 0 ? (bool?)null : polarityScore > 0;
                        CreatePolaritySentenceMapping(aspectProductMapping.PhraseId, sentencePhraseMapping.SentenceId, polarity, false);
                    }
                }
            }

            context.SubmitChanges();
        }

        #endregion

        #region Helper Functions

        //Extract phrases from the current product sentences based on the specified phrase type
        private List<Phrase> ExtractPhrases(Sentence sentence, List<ChunkedTerm> targetTermChunks, ChunkedGroup chunkedGroup, DistinctChunkTags chunkTag, int? maxPhraseSize = null)
        {
            List<Phrase> extractedPhrases = new List<Phrase>();

            //If the parameters are valid
            if (sentence != null && targetTermChunks != null && targetTermChunks.Count > 0 && chunkedGroup != null)
            {
                List<Term> targetTerms = ProcessTerms(targetTermChunks, StaticMappings.ChunkPOSMapping[chunkTag].First().ParseAsStringEnum<POSTags>());

                // If the term size of the phrase is 1 or bigger than the max phrase size
                // create a single term aspect for each term
                if (maxPhraseSize != null && (targetTerms.Count() == 1 || targetTerms.Count() > maxPhraseSize))
                {
                    foreach (Term term in targetTerms)
                    {
                        Phrase extractedPhrase = ProcessPhrase(new List<Term>() { term }, chunkedGroup);
                        if (extractedPhrase != null) extractedPhrases.Add(extractedPhrase);
                        CreateSentenceMapping(sentence, extractedPhrase, chunkedGroup);
                    }
                }
                else if (targetTerms.Any())
                {
                    Phrase extractedPhrase = ProcessPhrase(targetTerms, chunkedGroup);
                    if (extractedPhrase != null) extractedPhrases.Add(extractedPhrase);
                    CreateSentenceMapping(sentence, extractedPhrase, chunkedGroup);
                }
            }

            return extractedPhrases;
        }

        //Get/Insert the term ids from/to the database for each noun in the group
        private List<Term> ProcessTerms(List<ChunkedTerm> termChunks, POSTags posTag = POSTags.NounSingularOrMass)
        {
            List<Term> resultTerms = new List<Term>();

            for (int i = 0; i < termChunks.Count; i++)
            {
                //Bypass stopwords, one letter words and words that do not contain any character
                if (termChunks[i].Text.Length == 1 || !termChunks[i].Text.Any(char.IsLetter) || StaticPreprocess.StopWords.Contains(termChunks[i].Text.ToLower())) continue;

                //Remove symobol characters in the beginning and the end of the term
                String cleanedTerm = termChunks[i].Text;
                if (!char.IsLetterOrDigit(cleanedTerm.ToArray()[0]))
                    cleanedTerm = cleanedTerm.Substring(1);
                if (!char.IsLetterOrDigit(cleanedTerm.ToArray()[cleanedTerm.Length - 1]))
                    cleanedTerm = cleanedTerm.Substring(0, cleanedTerm.Length - 1);

                //Stem the term if its not misspelled
                //This way we avoid a common stemming function of removing the 's' at the end of the words
                //which are not common e.g.. the term 'GPRS' would be stemmed to 'GRP' which looses its
                //meaning
                bool isCorrect = HunspellChecker.IsCorrect(cleanedTerm, Languages.English.StringValue());

                String stemmedTerm = isCorrect ? stemmer.Stem(cleanedTerm, termChunks[i].POS).ToLower() : cleanedTerm.ToLower();
                String searchTerm = String.Empty;

                //If the term is not recognized from the English dictionary or has any non alphanumeric character
                if (!isCorrect || stemmedTerm.Any(a => !char.IsLetterOrDigit(a)))
                {
                    //Use fuzzy matching (Levenshtein Distance) to check for misspelled terms or terms
                    //that appear slightly different eg. 'auto-focus' and 'autofocus'
                    //NOTE: The case of 'auto focus' is not handled yet as those terms are considered
                    //      to be a phrase. Later we handle that specific scenario

                    foreach (String term in allTerms)
                    {
                        if (term.NormalizedLevenshteinDistance(stemmedTerm) < GetSimilarityLimit(term, stemmedTerm))
                        {
                            searchTerm = term;
                            break;
                        }
                    }
                }

                if (String.IsNullOrEmpty(searchTerm)) searchTerm = stemmedTerm;

                //Force the manual terms (nouns) to the Noun DB POSTagId value due to
                //false term POS tagging...
                short posTagId = StaticMappings.GetDBPOSTagId(posTag);
                //Search for the term
                Term currentTerm = CompiledLINQ.FindTerm(context, searchTerm, posTagId).SingleOrDefault();

                //If the current term is not saved in the database insert it
                if (currentTerm == null)
                {
                    currentTerm = new Term();
                    currentTerm.Text = termChunks[i].Text;
                    currentTerm.PreprocessedText = searchTerm;
                    currentTerm.POSId = posTagId;
                    context.Term.InsertOnSubmit(currentTerm);
                    context.SubmitChanges();

                    //Add the word to the list used for fuzzy matching
                    allTerms.Add(searchTerm);
                }

                resultTerms.Add(currentTerm);
                //Save term id to the XML structure
                termChunks[i].Id = currentTerm.Id;
            }

            return resultTerms;
        }

        //Get/Insert the phrase from/to the database based on the term ids
        private Phrase ProcessPhrase(List<Term> terms, ChunkedGroup group = null, bool isManual = false, bool? polarity = null)
        {
            Phrase resultPhrase = null;

            if (terms != null && terms.Count > 0)
            {
                //Ensure that all terms are of the same type
                if (terms.Any(a => a.POSType != terms.First().POSType))
                    throw new Exception("The term list that was specified had multiple POS types.");

                if (terms.Any(a => a.Text.Contains("masala")))
                    resultPhrase = null;

                //The following query is used to check if the specified terms
                //are saved as an phrase. Superset phrases are ignored
                String phraseQuery = @"
SELECT p.Id
FROM Phrase p
     LEFT JOIN PhraseTermMapping m ON p.Id = m.PhraseId
     LEFT JOIN
	 (SELECT p.Id, COUNT(m.Id) AS TermCount
      FROM Phrase p
		   INNER JOIN PhraseTermMapping m ON m.PhraseId = p.Id
	  WHERE p.POSId = {2}
      GROUP BY p.Id) pc
	  ON pc.Id = m.PhraseId
	  LEFT JOIN Term t ON t.Id = m.TermId
WHERE t.POSId = {3} AND p.POSId = {2}
      AND t.Id IN ({0}) AND pc.TermCount = {1}
GROUP BY p.Id HAVING COUNT(p.Id) = {1}";

                phraseQuery = String.Format(phraseQuery, String.Join(", ", terms.Select(a => a.Id)), terms.Count, StaticMappings.GetDBPOSTagId(terms.First().POSId), terms.First().POSId);

                DataTable result = dbHandler.ExecuteDataTable(phraseQuery);

                bool isMatch = result.Rows.Count > 0;

                //If we found a match get the aspect
                if (isMatch)
                {
                    //Validate the terms in the phrase
                    foreach (DataRow dataTableRow in result.Rows)
                    {
                        resultPhrase = CompiledLINQ.FindPhrase(context, int.Parse(result.Rows[0]["Id"].ToString())).SingleOrDefault();

                        if (terms.Count == 1)
                            isMatch = true;
                        else
                            foreach (Term term in terms)
                                if (!resultPhrase.PhraseTermMapping.Any(a => a.Term == term))
                                    isMatch = false;

                        if (isMatch) break;
                    }
                }

                if (!isMatch)
                {
                    //Else insert a new record
                    resultPhrase = new Phrase();
                    resultPhrase.Text = String.Join(" ", terms.Select(a => a.Text));
                    resultPhrase.PreprocessedText = String.Join(" ", terms.Select(a => a.PreprocessedText));
                    resultPhrase.POSId = StaticMappings.GetDBPOSTagId(terms.First().POSId);
                    resultPhrase.PhraseTermMapping = new EntitySet<PhraseTermMapping>();

                    foreach (Term term in terms)
                    {
                        PhraseTermMapping newMapping = new PhraseTermMapping();
                        newMapping.Term = term;
                        resultPhrase.PhraseTermMapping.Add(newMapping);
                    }

                    context.Phrase.InsertOnSubmit(resultPhrase);
                    context.SubmitChanges();
                }


                //Save aspect id to the XML structure
                if (group != null && group.Id == 0) group.Id = resultPhrase.Id;

                //Also add a mapping for the aspect to the specific product if
                //none exists
                PhraseProductMapping phraseProductMapping = CompiledLINQ.FindPhraseProductMapping(context, resultPhrase.Id, productId, isManual).SingleOrDefault();

                if (phraseProductMapping == null)
                {
                    phraseProductMapping = new PhraseProductMapping();
                    phraseProductMapping.Phrase = resultPhrase;
                    phraseProductMapping.ProductId = ProductId;
                    phraseProductMapping.IsManual = isManual;
                    context.PhraseProductMapping.InsertOnSubmit(phraseProductMapping);
                }

                //Update the aspects frequency
                phraseProductMapping.Frequency++;

                //Update the polarity for manual reults
                if (isManual)
                {
                    if (polarity == null) phraseProductMapping.NeutralReferences++;
                    else if (polarity == true) phraseProductMapping.PositiveReferences++;
                    else phraseProductMapping.NegativeReferences++;
                }

                context.SubmitChanges();
            }

            return resultPhrase;
        }

        private void CreateSentenceMapping(Sentence sentence, Phrase phrase, object orderObject)
        {
            // If the current sentence doesn't have the phrase mapped
            // add the relation
            if (!(phrase == null || sentence == null || sentence.SentencePhraseMapping.Count(a => a.PhraseId == phrase.Id) > 0))
            {
                SentencePhraseMapping phraseMapping = new SentencePhraseMapping();
                phraseMapping.Sentence = sentence;
                phraseMapping.Phrase = phrase;

                foreach (PhraseTermMapping phraseTermMapping in phrase.PhraseTermMapping)
                {
                    SentencePhraseTermMapping termMapping = new SentencePhraseTermMapping();
                    termMapping.SentencePhraseMapping = phraseMapping;
                    termMapping.PhraseTermMapping = phraseTermMapping;

                    //If orderObject is a ChunkedGroup then the aspect was created from a POS chunk phrase
                    if (orderObject != null && orderObject.GetType() == typeof(ChunkedGroup))
                    {
                        ChunkedTerm chunkedTerm = (orderObject as ChunkedGroup).Terms.FirstOrDefault(a => a.Id == phraseTermMapping.TermId);
                        termMapping.TermOrder = chunkedTerm != null ? chunkedTerm.Order : (int?)null;
                    }
                    //Else it is generated by aspect terms
                    else if (orderObject != null && orderObject.GetType() == typeof(Dictionary<Term, int?>))
                    {
                        termMapping.TermOrder = (orderObject as Dictionary<Term, int?>)[phraseTermMapping.Term];
                    }
                }

                phraseMapping.IsGenerated = orderObject != null && orderObject.GetType() == typeof(Dictionary<Term, int?>);

                sentence.RefreshChunkedSentence();
                context.SentencePhraseMapping.InsertOnSubmit(phraseMapping);
                context.SubmitChanges();
            }
        }

        //Generate aspect combinations specified by term count
        private void GenerateAspects(Sentence sentence, int termCount, List<List<Phrase>> frequentAspectCombinations)
        {
            //Get all aspects of a sentence and generate all possible combinations excluding single items
            //that we already have.
            List<List<Phrase>> sentenceAspectsCombinations = sentence.SentencePhraseMapping
                                                             .Where(a => a.Phrase.POSId == StaticMappings.GetDBPOSTagId(DistinctChunkTags.NounPhrase))
                                                             .Select(a => a.Phrase)
                                                             .ToList()
                                                             .GetAllCombos()
                                                             .Where(a => a.Count > 1)
                                                             .ToList();
            //For each combination...
            foreach (List<Phrase> aspects in sentenceAspectsCombinations)
            {
                //If the term count of the aspects we combine to generate a new one equals the provided parameter
                //and all the aspects we combine exist in the frequent aspect calculations
                if (aspects.Select(a => a.PhraseTermMapping.Count).Sum() == termCount && frequentAspectCombinations.Any(a => !a.Except(aspects).Any()))
                {
                    //Get the sentence-term mapping of those aspects
                    List<SentencePhraseTermMapping> aspectTermMapping = new List<SentencePhraseTermMapping>();
                    foreach (Phrase aspect in aspects)
                        aspectTermMapping.AddRange(sentence.SentencePhraseMapping.SingleOrDefault(a => a.Phrase == aspect).SentencePhraseTermMapping);

                    bool invalidGeneration = false;

                    //And create a structure containing the term id and the term order in the sentence
                    //(this is because there is no ChunkText for the generated aspect to provide in the CreateSentenceMapping function)
                    Dictionary<Term, int?> termOrderMapping = new Dictionary<Term, int?>();
                    foreach (SentencePhraseTermMapping sentenceAspectTermMapping in aspectTermMapping)
                        if (termOrderMapping.ContainsKey(sentenceAspectTermMapping.PhraseTermMapping.Term))
                            invalidGeneration = true; /* If the same key is added this means we try to,
                                                         generate a phrase by combining the phrase itself
                                                         and a subset of itwhich is invalid and we skip it */
                        else
                            termOrderMapping.Add(sentenceAspectTermMapping.PhraseTermMapping.Term, sentenceAspectTermMapping.TermOrder);

                    if (!invalidGeneration)
                    {
                        //Submit the generated aspect
                        Phrase generatedAspect = ProcessPhrase(termOrderMapping.Keys.ToList());
                        //And add a mapping to the sentence
                        CreateSentenceMapping(sentence, generatedAspect, termOrderMapping);
                    }
                }
            }
        }

        private List<List<Phrase>> Apriori()
        {
            /* To apply the apriori algorithm we consider an aspect (Phrase) as the item
             * and the sentence as the transaction. Becase the aspect consists of terms
             * the matching checks are made on term level.
             */

            //Here we hold the combination of generated rules
            List<List<Phrase>> result = new List<List<Phrase>>();


            List<PhraseProductMapping> allAspects = (from u in context.PhraseProductMapping
                                                     where u.ProductId == productId
                                                           && !u.IsManual
                                                     select u).ToList();

            //Get all frequent aspects to generate rules from those itemsets
            var frequentAspects = (from u in context.PhraseProductMapping
                                   where !u.IsManual && u.ProductId == productId
                                   select u).ToList()
                                            .Where(a => a.GetFrequencyPercent(allSentences.Count) > MinFrequency)
                                            .ToList();

            //We start combining sequences of frequent items of length = 2 and increase in each iteration
            int combinationSize = 2;

            //If the frequent itemsets are less than the combination size we stop
            while (frequentAspects.Count >= combinationSize)
            {
                //This will generate all possible unique combinations of the frequent itemset we provide
                //for a specified itemset length
                var allCombinations = new Combinations<PhraseProductMapping>(frequentAspects, combinationSize);
                //We clear the frequent items in order to update with the frequent ones from the generated combinations
                frequentAspects.Clear();

                //For each generated combination
                foreach (var combination in allCombinations)
                {
                    //We get the term ids of the combined aspects (because the matching check is done in Term level)
                    List<int> termIds = combination.SelectMany(a => a.Phrase.PhraseTermMapping).Select(a => a.TermId).ToList();
                    //We count the sentences (transactions) that contain every term id of the generated combination
                    double frequency = allSentences.Count(a => !termIds.Except(a.SentencePhraseMapping
                                                                       .SelectMany(b => b.SentencePhraseTermMapping)
                                                                       .Select(b => b.PhraseTermMapping.TermId))
                                                                       .Any());

                    //If the frequency satisfies our minimum support we add the rule and the combination
                    if (frequency / allSentences.Count * 100 > MinFrequency)
                    {
                        result.Add(combination.Select(a => a.Phrase).ToList());
                        frequentAspects.AddRange(combination);
                    }
                }

                //We remove the duplicates from the frequent itemset
                frequentAspects = frequentAspects.Distinct().ToList();
                //and increase the generated combination size
                combinationSize++;
            }

            return result;
        }

        //Get different similarity limits based on the string length
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

        public void InitializeSentiWordNet(string sourceFile)
        {
            sentiWordNet = new SentiWordNet(sourceFile);
        }

        private void CreatePolaritySentenceMapping(int phraseId, int sentenceId, bool? polarity, bool isManual, bool submitChanges = false)
        {
            PhrasePolaritySentenceMapping newPolarityMapping = new PhrasePolaritySentenceMapping();
            newPolarityMapping.PhraseId = phraseId;
            newPolarityMapping.SentenceId = sentenceId;
            newPolarityMapping.Polarity = polarity;
            newPolarityMapping.IsManual = isManual;
            context.PhrasePolaritySentenceMapping.InsertOnSubmit(newPolarityMapping);

            if (submitChanges)
                context.SubmitChanges();
        }

        #endregion
    }
}
