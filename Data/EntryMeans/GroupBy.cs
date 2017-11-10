using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    public class GroupBy : IEquatable<GroupBy>
    {
        private readonly string name;
        private readonly string value;
        private readonly bool band;
        private readonly int hashCode;

        [JsonConstructor]
        public GroupBy(string name, string value, bool band)
        {
            this.name = name ?? throw new ArgumentNullException("name");
            this.value = value ?? throw new ArgumentNullException("value");
            this.band = band;
            this.hashCode = (name + ":" + value).GetHashCode();
        }

        [JsonProperty("name")]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        [JsonProperty("value")]
        public string Value
        {
            get
            {
                return this.value;
            }
        }

        [JsonProperty("band")]
        public bool Band
        {
            get
            {
                return this.band;
            }
        }

        public bool Equals(GroupBy other)
        {
            return this.hashCode.Equals(other.hashCode);
        }

        public override bool Equals(object obj)
        {
            if (obj is GroupBy)
            {
                return this.Equals((GroupBy)obj);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public string ToKeyString()
        {
            return this.name + this.value;
        }

    }
}
