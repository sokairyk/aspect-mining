using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Tools;


namespace JavaLibraries
{
    [Serializable]
    [XmlRoot("Sentence")]
    public class ChunkedText
    {
        [XmlIgnore]
        public String Text;
        [XmlIgnore]
        public String[] TokenArray;
        [XmlIgnore]
        public String[] POSTagArray;
        [XmlIgnore]
        public String[] ChunkTagArray;
        [XmlElement]
        public List<ChunkedGroup> Groups = new List<ChunkedGroup>();

        public void MakeSerializable()
        {
            //Validate parameters
            if (TokenArray == null || POSTagArray == null || ChunkTagArray == null ||
                TokenArray.Length != POSTagArray.Length || POSTagArray.Length != ChunkTagArray.Length) return;
            ChunkedGroup group = new ChunkedGroup();

            for (int i = 0; i < TokenArray.Length; i++)
            {
                ChunkedTerm term = new ChunkedTerm();
                term.Text = TokenArray[i];
                term.POS = POSTagArray[i];
                term.Order = i;
                if (StaticMappings.BeginChunkTags.Contains(ChunkTagArray[i].ParseAsStringEnum<ChunkTags>()) ||
                    ChunkTagArray[i] == ChunkTags.OutsideOfAnyChunk.StringValue())
                {
                    if (group.Terms.Count > 0)
                    {
                        Groups.Add(group);
                    }
                    group = new ChunkedGroup();
                    group.Tag =
                        StaticMappings.DistinctChunkTagsMapper[ChunkTagArray[i].ParseAsStringEnum<ChunkTags>()]
                            .StringValue();
                }
                else
                {
                    if (group.Terms.Count == 0)
                    {
                        group = new ChunkedGroup();
                        group.Tag =
                            StaticMappings.DistinctChunkTagsMapper[ChunkTagArray[i].ParseAsStringEnum<ChunkTags>()]
                                .StringValue();
                    }
                }
                group.Terms.Add(term);
            }

            if (group.Terms.Count > 0)
            {
                Groups.Add(group);
            }
        }

        public List<ChunkedGroup> GetNearestGroupById(int groupId, int[] termIds, DistinctChunkTags chunkTag, int maxDistance)
        {
            List<ChunkedGroup> foundGroups = new List<ChunkedGroup>();
            List<ChunkedGroup> targetGroups = Groups.Where(a => a.Id == groupId).ToList();

            //If a group is inserted in the database as multiple phrases because of the maxPhraseSize limit
            //in the phrase extraction phase, then the groupId in the XML structure only holds one of those
            //phrase values. To still be able to find the target group by the ineer term ids we do the following:
            if (targetGroups.Count == 0)
            {
                targetGroups = Groups.Where(a => { 
                    var ids = a.Terms.Select(b => b.Id);
                    return !termIds.Except(ids).Any();
                }).ToList();
            }

            foreach (ChunkedGroup targetGroup in targetGroups)
            {
                //Get the positions in the sentence by the first and last term of the group id
                int firstPosition = targetGroup.Terms.Count > 0 ? targetGroup.Terms.First().Order : -1;
                int lastPosition = targetGroup.Terms.Count > 0 ? targetGroup.Terms.Last().Order : -1;

                //If we find a group of the specified chunk tag OR any group with a related POS tag of the specified chunk, 
                //and the distance between the first term of our target group and the last term of a candidate group
                //or the last term of the target group and the first term of a candidate group, is smaller
                //than the specified target distance add the candidate group
                if (firstPosition >= 0 && lastPosition >= 0)
                    foundGroups.AddRange(Groups.Where(a => (a.Tag == chunkTag.StringValue()
                                                            || a.Terms.Any(b => StaticMappings.ChunkPOSMapping[chunkTag].Contains(b.POS)))
                                                           && a.Terms.Count > 0
                                                           && (Math.Abs(lastPosition - a.Terms.First().Order) < maxDistance
                                                               || Math.Abs(firstPosition - a.Terms.Last().Order) < maxDistance)));
            }

            foreach (ChunkedGroup targetGroup in targetGroups)
                foundGroups.Remove(targetGroup);

            return foundGroups.Distinct().ToList();
        }
    }

    [Serializable]
    [XmlRoot("Group")]
    public class ChunkedGroup
    {
        [XmlAttribute("Id")]
        public int Id;
        [XmlAttribute("Tag")]
        public String Tag;
        [XmlElement]
        public List<ChunkedTerm> Terms = new List<ChunkedTerm>();
    }

    [Serializable]
    [XmlRoot("Term")]
    public class ChunkedTerm
    {
        [XmlAttribute("Id")]
        public int Id;
        [XmlAttribute("POS")]
        public String POS;
        [XmlAttribute("Word")]
        public String Text;
        [XmlAttribute("Order")]
        public int Order;
    }
}
