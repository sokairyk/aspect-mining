using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace Tools
{
    public enum POSTags
    {
        [StringValue("CC")]
        CoordinatingConjunction, //and
        [StringValue("CD")]
        CardinalNumber, //1, three
        [StringValue("DT")]
        Determiner, //the
        [StringValue("EX")]
        Existential, //there is
        [StringValue("FW")]
        ForeignWord, //d'hoevre
        [StringValue("IN")]
        PrepositionSubordinatingConjunction, //in, of, like, after, that
        [StringValue("JJ")]
        Adjective, //green
        [StringValue("JJR")]
        AdjectiveComparative, //greener
        [StringValue("JJS")]
        AdjectiveSuperlative, //greenest
        [StringValue("LS")] //1)
        ListMarker,
        [StringValue("MD")]
        Modal, //could, will
        [StringValue("NN")]
        NounSingularOrMass, //table
        [StringValue("NNS")]
        NounPlural, //tables
        [StringValue("NNP")]
        ProperNounSingular, //John
        [StringValue("NNPS")]
        ProperNounPlural, //Vikings
        [StringValue("PDT")]
        Predeterminer, //both the boys
        [StringValue("POS")]
        PossessiveEnding, //friend's
        [StringValue("PRP")]
        PersonalPronoun, //I, he, it
        [StringValue("PRP$")]
        PossessivePronoun, //my, his
        [StringValue("RB")]
        Adverb, //however, usually, naturally, here, good
        [StringValue("RBR")]
        AdverbComparative, //better
        [StringValue("RBS")]
        AdverbSuperlative, //best
        [StringValue("RP")]
        Particle, //give up
        [StringValue("SYM")]
        Symbol, //$, %
        [StringValue("TO")]
        To, //to go, to him
        [StringValue("UH")]
        Interjection, //uhhuhhuhh
        [StringValue("VB")]
        VerbBaseForm, //take
        [StringValue("VBD")]
        VerbPastTense, //took
        [StringValue("VBG")]
        VerbGerundOrPresentParticiple, //taking
        [StringValue("VBN")]
        VerbPastParticiple, //taken
        [StringValue("VBP")]
        VerbSingularPresentNon3d, //take
        [StringValue("VBZ")]
        Verb3rdPersonSingularPresent, //takes
        [StringValue("WDT")]
        WhDeterminer, //which
        [StringValue("WP")]
        WhPronoun, //who, what
        [StringValue("WP$")]
        PossessiveWhPronoun, //whose
        [StringValue("WRB")]
        WhAbverb, //where, when 
    }

    public enum ChunkTags
    {
        [StringValue("B-ADJP")]
        BeginsAdjectivePhrase,
        [StringValue("I-ADJP")]
        InsideAdjectivePhrase,
        [StringValue("B-ADVP")]
        BeginsAdverbialPhrase,
        [StringValue("I-ADVP")]
        InsideAdverbialPhrase,
        [StringValue("B-CONJP")]
        BeginsConjunctivePhrase,
        [StringValue("I-CONJP")]
        InsideConjunctivePhrase,
        [StringValue("B-INTJ")]
        BeginsInterjection,
        [StringValue("I-INTJ")]
        InsideInterjection,
        [StringValue("B-LST")]
        BeginsListMarker,
        [StringValue("I-LST")]
        InsideListMarker,
        [StringValue("B-NP")]
        BeginsNounPhrase,
        [StringValue("I-NP")]
        InsideNounPhrase,
        [StringValue("B-PP")]
        BeginsPrepositionalPhrase,
        [StringValue("I-PP")]
        InsidePrepositionalPhrase,
        [StringValue("B-PRT")]
        BeginsParticle,
        [StringValue("I-PRT")]
        InsideParticle,
        [StringValue("B-SBAR")]
        BeginsSubordinatedClause,
        [StringValue("I-SBAR")]
        InsideSubordinatedClause,
        [StringValue("B-UCP")]
        BeginsUnlikeCoordinatedPhrase,
        [StringValue("I-UCP")]
        InsideUnlikeCoordinatedPhrase,
        [StringValue("B-VP")]
        BeginsVerbPhrase,
        [StringValue("I-VP")]
        InsideVerbPhrase,
        [StringValue("O")]
        OutsideOfAnyChunk
    }

    public enum DistinctChunkTags
    {
        [StringValue("ADJP")]
        AdjectivePhrase,
        [StringValue("ADVP")]
        AdverbialPhrase,
        [StringValue("CONJP")]
        ConjunctivePhrase,
        [StringValue("INTJ")]
        Interjection,
        [StringValue("LST")]
        ListMarker,
        [StringValue("NP")]
        NounPhrase,
        [StringValue("PP")]
        PrepositionalPhrase,
        [StringValue("PRT")]
        Particle,
        [StringValue("SBAR")]
        SubordinatedClause,
        [StringValue("UCP")]
        UnlikeCoordinatedPhrase,
        [StringValue("VP")]
        VerbPhrase,
        [StringValue("O")]
        OutsideOfAnyChunk
    }

    public enum SentiWordNetPOS
    {
        [StringValue("a")]
        Adjective,
        [StringValue("v")]
        Verb,
        [StringValue("n")]
        Noun,
        [StringValue("r")]
        Adverb
    }

    public enum Languages
    {
        [StringValue("en")]
        English
    }

    public enum DatasetParserType
    {
        Undefined = -1,
        HuLiu = 1,
        SemEval = 2
    }

    public static class StaticMappings
    {
        public static Dictionary<ChunkTags, DistinctChunkTags> DistinctChunkTagsMapper = new Dictionary<ChunkTags, DistinctChunkTags>()
        {
            {ChunkTags.BeginsAdjectivePhrase, DistinctChunkTags.AdjectivePhrase},
            {ChunkTags.InsideAdjectivePhrase, DistinctChunkTags.AdjectivePhrase},
            {ChunkTags.BeginsAdverbialPhrase, DistinctChunkTags.AdverbialPhrase},
            {ChunkTags.InsideAdverbialPhrase, DistinctChunkTags.AdverbialPhrase},
            {ChunkTags.BeginsConjunctivePhrase, DistinctChunkTags.ConjunctivePhrase},
            {ChunkTags.InsideConjunctivePhrase, DistinctChunkTags.ConjunctivePhrase},
            {ChunkTags.BeginsInterjection, DistinctChunkTags.Interjection},
            {ChunkTags.InsideInterjection, DistinctChunkTags.Interjection},
            {ChunkTags.BeginsListMarker, DistinctChunkTags.ListMarker},
            {ChunkTags.InsideListMarker, DistinctChunkTags.ListMarker},
            {ChunkTags.BeginsNounPhrase, DistinctChunkTags.NounPhrase},
            {ChunkTags.InsideNounPhrase, DistinctChunkTags.NounPhrase},
            {ChunkTags.BeginsParticle, DistinctChunkTags.Particle},
            {ChunkTags.InsideParticle, DistinctChunkTags.Particle},
            {ChunkTags.BeginsPrepositionalPhrase, DistinctChunkTags.PrepositionalPhrase},
            {ChunkTags.InsidePrepositionalPhrase, DistinctChunkTags.PrepositionalPhrase},
            {ChunkTags.BeginsSubordinatedClause, DistinctChunkTags.SubordinatedClause},
            {ChunkTags.InsideSubordinatedClause, DistinctChunkTags.SubordinatedClause},
            {ChunkTags.BeginsUnlikeCoordinatedPhrase, DistinctChunkTags.UnlikeCoordinatedPhrase},
            {ChunkTags.InsideUnlikeCoordinatedPhrase, DistinctChunkTags.UnlikeCoordinatedPhrase},
            {ChunkTags.BeginsVerbPhrase, DistinctChunkTags.VerbPhrase},
            {ChunkTags.InsideVerbPhrase, DistinctChunkTags.VerbPhrase},
            {ChunkTags.OutsideOfAnyChunk, DistinctChunkTags.OutsideOfAnyChunk}
        };

        public static List<ChunkTags> BeginChunkTags = new List<ChunkTags>()
        {
            ChunkTags.BeginsAdjectivePhrase, ChunkTags.BeginsAdverbialPhrase,
            ChunkTags.BeginsConjunctivePhrase, ChunkTags.BeginsInterjection,
            ChunkTags.BeginsListMarker, ChunkTags.BeginsNounPhrase,
            ChunkTags.BeginsParticle, ChunkTags.BeginsPrepositionalPhrase,
            ChunkTags.BeginsSubordinatedClause, ChunkTags.BeginsUnlikeCoordinatedPhrase,
            ChunkTags.BeginsVerbPhrase
        };

        //Chunk - POS mapping
        public static Dictionary<DistinctChunkTags, List<String>> ChunkPOSMapping = new Dictionary<DistinctChunkTags, List<String>>()
        {
            {
                DistinctChunkTags.NounPhrase, new List<string>()
                {
                    POSTags.NounSingularOrMass.StringValue(),
                    POSTags.NounPlural.StringValue(),
                    POSTags.ProperNounSingular.StringValue(),
                    POSTags.ProperNounPlural.StringValue()
                }
            },
            {
                DistinctChunkTags.AdjectivePhrase, new List<string>()
                {
                    POSTags.Adjective.StringValue(),
                    POSTags.AdjectiveComparative.StringValue(),
                    POSTags.AdjectiveSuperlative.StringValue()
                }
            },
            {
                DistinctChunkTags.VerbPhrase, new List<string>()
                {
                    POSTags.Verb3rdPersonSingularPresent.StringValue(),
                    POSTags.VerbBaseForm.StringValue(),
                    POSTags.VerbGerundOrPresentParticiple.StringValue(),
                    POSTags.VerbPastParticiple.StringValue(),
                    POSTags.VerbPastTense.StringValue(),
                    POSTags.VerbSingularPresentNon3d.StringValue()
                }
            },
            {
                DistinctChunkTags.AdverbialPhrase, new List<string>()
                {
                    POSTags.Adverb.StringValue(),
                    POSTags.AdverbComparative.StringValue(),
                    POSTags.AdverbSuperlative.StringValue()
                }
            }
        };

        //POS - Chunk mapping
        public static Dictionary<string, string> POSChunkMapping = new Dictionary<string, string>()
        {
            { POSTags.NounSingularOrMass.StringValue(), DistinctChunkTags.NounPhrase.StringValue() },
            { POSTags.NounPlural.StringValue(), DistinctChunkTags.NounPhrase.StringValue() },
            { POSTags.ProperNounSingular.StringValue(), DistinctChunkTags.NounPhrase.StringValue() },
            { POSTags.ProperNounPlural.StringValue(), DistinctChunkTags.NounPhrase.StringValue() },
            { POSTags.Adjective.StringValue(), DistinctChunkTags.AdjectivePhrase.StringValue() },
            { POSTags.AdjectiveComparative.StringValue(), DistinctChunkTags.AdjectivePhrase.StringValue() },
            { POSTags.AdjectiveSuperlative.StringValue(), DistinctChunkTags.AdjectivePhrase.StringValue() },
        };

        //SentiWordNet - POS mapping
        public static Dictionary<string, SentiWordNetPOS> SentiWordNetPOSMapping = new Dictionary<string, SentiWordNetPOS>()
        {
            { POSTags.Adjective.StringValue(), SentiWordNetPOS.Adjective },
            { POSTags.AdjectiveComparative.StringValue(), SentiWordNetPOS.Adjective },
            { POSTags.AdjectiveSuperlative.StringValue(), SentiWordNetPOS.Adjective },
            { POSTags.Verb3rdPersonSingularPresent.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.VerbBaseForm.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.VerbGerundOrPresentParticiple.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.VerbPastParticiple.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.VerbPastTense.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.VerbSingularPresentNon3d.StringValue(), SentiWordNetPOS.Verb },
            { POSTags.NounPlural.StringValue(), SentiWordNetPOS.Noun },
            { POSTags.NounSingularOrMass.StringValue(), SentiWordNetPOS.Noun },
            { POSTags.ProperNounPlural.StringValue(), SentiWordNetPOS.Noun },
            { POSTags.ProperNounSingular.StringValue(), SentiWordNetPOS.Noun },
            { POSTags.Adverb.StringValue(), SentiWordNetPOS.Adverb },
            { POSTags.AdverbComparative.StringValue(), SentiWordNetPOS.Adverb },
            { POSTags.AdverbSuperlative.StringValue(), SentiWordNetPOS.Adverb },
            {DistinctChunkTags.AdjectivePhrase.StringValue(), SentiWordNetPOS.Adjective},
            {DistinctChunkTags.VerbPhrase.StringValue(), SentiWordNetPOS.Verb},
            {DistinctChunkTags.NounPhrase.StringValue(), SentiWordNetPOS.Noun},
            {DistinctChunkTags.AdverbialPhrase.StringValue(), SentiWordNetPOS.Adverb},
        };

        //CAUTION: The specified return values MUST correspond to the POSType database table entries
        //         otherwise every core function won't work properly
        public static short GetDBPOSTagId(DistinctChunkTags chunkTag)
        {
            switch (chunkTag)
            {
                case DistinctChunkTags.NounPhrase:
                    return 1;
                case DistinctChunkTags.AdjectivePhrase:
                    return 2;
                default:
                    return -1;
            }
        }

        public static short GetDBPOSTagId(POSTags posTag)
        {
            switch (posTag)
            {
                case POSTags.NounPlural:
                case POSTags.NounSingularOrMass:
                case POSTags.ProperNounPlural:
                case POSTags.ProperNounSingular:
                    return 3;
                case POSTags.Adjective:
                case POSTags.AdjectiveComparative:
                case POSTags.AdjectiveSuperlative:
                    return 4;
                default:
                    return -1;
            }
        }

        //This returns a phrase's POSTypeId given the POSTypeId of a term 
        //and vice versa. Use with caution!
        public static short GetDBPOSTagId(short posTagId)
        {
            switch (posTagId)
            {
                case 1:
                    return 3;
                case 2:
                    return 4;
                case 3:
                    return 1;
                case 4:
                    return 2;
                default:
                    return -1;
            }
        }
    }

    public class StaticPreprocess
    {
        //Lucene stopwords
        public static HashSet<String> StopWords = new HashSet<String>()
        {
            "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into",
            "is", "it", "no", "not", "of", "on", "or", "such", "that", "the", "their", "then",
            "there", "these", "they", "this", "to", "was", "will", "with"
        };
    }
}
