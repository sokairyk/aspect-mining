using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AspectMining.Core
{
    [Serializable]
    public class ManualResults
    {
        [XmlElement]
        public List<HLResultAspect> HLResultAspects = new List<HLResultAspect>();
        [XmlElement]
        public List<SemEvalResultAspect> SemEvalResultAspects = new List<SemEvalResultAspect>();
    }

    [Serializable]
    public class HLResultAspect
    {
        [XmlAttribute("Aspect")]
        public String AspectTermText;
        [XmlAttribute("OpinionStrength")]
        public int OpinionStrength;
        [XmlAttribute("NotAppeared")]
        public bool NotAppeared;
        [XmlAttribute("PronounResolutionNeeded")]
        public bool PronounResolutionNeeded;
    }

    [Serializable]
    public class SemEvalResultAspect
    {
        [XmlAttribute("Aspect")]
        public String AspectTermText;
        [XmlAttribute("Polarity")]
        public String Polarity;
        [XmlAttribute("OpinionStrengthFrom")]
        public int OpinionStrengthFrom;
        [XmlAttribute("OpinionStrengthTo")]
        public int OpinionStrengthTo;
    }
}
