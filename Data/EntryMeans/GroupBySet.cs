using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    [Serializable]
    public class GroupBySet : IEquatable<GroupBySet>
    {
        private HashSet<GroupBy> list = new HashSet<GroupBy>();
        private long hashCode = 0;

        private static HashSet<string> bandedFields = new HashSet<string>();
        private static HashSet<string> analysisTypeFields = new HashSet<string>();
        private static Dictionary<string, long> bandMappingIndex = new Dictionary<string, long>();
        private static long currentBandIndex = 1;

        private static readonly object sema = new object();

        public GroupBySet()
        {
            list = new HashSet<GroupBy>();
        }

        public static void Initialize(List<string> bands, string analysisType)
        {
            bandedFields.Clear();
            analysisTypeFields.Clear();
            AnalysisTypeDefinition atType = AnalysisTypeDefinition.Get(analysisType);
            if (atType != null)
            {
                foreach (string field in atType.Fields)
                {
                    GetBandMappingIndex(field);
                }

            }
            foreach (string band in bands)
            {
                GetBandMappingIndex(band);
            }
        }

        public GroupBySet(List<GroupBy> bandValues)
        {
            list = new HashSet<GroupBy>();
            foreach(GroupBy bandValue in bandValues)
            {
                this.Add(bandValue);
            }
        }

        public void Add(GroupBy bandValue)
        {
            list.Add(bandValue);
            hashCode += bandValue.GetHashCode() * GetBandMappingIndex(bandValue.Name);
        }

        public bool Equals(GroupBySet other)
        {
            return this.hashCode.Equals(other.hashCode);
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupBySet)
            {
                return this.Equals((GroupBySet)obj);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return hashCode.GetHashCode();
        }

        public long GetLongHashCode()
        {
            return hashCode;
        }

        private static long GetBandMappingIndex(string name)
        {
            long index;
            if (!bandMappingIndex.TryGetValue(name, out index))
            {
                lock (sema)
                {
                    if (!bandMappingIndex.TryGetValue(name, out index))
                    {
                        index = currentBandIndex;
                        bandMappingIndex.Add(name, index);
                        currentBandIndex *= 2;
                    }
                }
            }
            return index;
        }

        [JsonProperty]
        public IEnumerable<GroupBy> List
        {
            get
            {
                return list;
            }
        }

        public string ToKeyString()
        {
            return string.Join(";", list.Select(i => i.ToKeyString()));
        }
    }
}
