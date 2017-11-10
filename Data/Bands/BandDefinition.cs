using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class BandDefinition
    {
        private readonly string category;
        private readonly string bandingGroup;
        private readonly string bandName;
        private readonly object minValue;
        private readonly object maxValue;
        private readonly double? interval;
        private readonly long hashCode;

        [JsonConstructor]
        public BandDefinition(string bandName, string category = "", string bandingGroup = "", object minValue = null, object maxValue = null, double? interval = null)
        {
            this.hashCode = 0;
            this.category = category;
            this.bandingGroup = bandingGroup;
            this.bandName = bandName ?? throw new ArgumentNullException("bandName");
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.interval = interval;

            if (string.IsNullOrEmpty(this.category))
            {
                this.hashCode += Int32.MaxValue;
            }
            else
            {
                this.hashCode += this.category.GetHashCode();
            }
            if (string.IsNullOrEmpty(this.bandingGroup))
            {
                this.hashCode += Int32.MaxValue * 2L;
            }
            else
            {
                this.hashCode += this.bandingGroup.GetHashCode() * 2L;
            }
            this.hashCode += this.bandName.GetHashCode() * 4L;
        }

        [JsonProperty("category")]
        public string Category
        {
            get
            {
                return this.category;
            }
        }

        [JsonProperty("bandingGroup")]
        public string BandingGroup
        {
            get
            {
                return this.bandingGroup;
            }
        }

        [JsonProperty("bandName")]
        [JsonRequired]
        public string BandName
        {
            get
            {
                return this.bandName;
            }
        }

        [JsonProperty("minValue")]
        public object MinValue
        {
            get
            {
                return this.minValue;
            }
        }

        [JsonProperty("maxValue")]
        public object MaxValue
        {
            get
            {
                return this.maxValue;
            }
        }

        [JsonProperty("interval")]
        public double? Interval
        {
            get
            {
                return this.interval;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is BandDefinition)
            {
                return (this.hashCode == ((BandDefinition)obj).hashCode);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return hashCode.GetHashCode();
        }

        public BandDefinition CoerceObjectsToDouble()
        {
            double minValue = Convert.ToDouble(this.MinValue);
            double maxValue = Convert.ToDouble(this.MaxValue);
            return new BandDefinition(this.bandName, this.category, this.bandingGroup, minValue, maxValue, this.interval);
        }
    }
}
