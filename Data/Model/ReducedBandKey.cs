using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.ReportData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace H2HGermPlasmProcessor
{
    public class ReducedBandKey
    {
        private readonly string reportName;
        private readonly string reportGrouping;
        private readonly string product;
        private readonly string headOrOther;
        private readonly GroupBySet bandSet;
        private readonly string cEvent;
        private readonly string cacheKey;
        private readonly int reportId;

        [JsonConstructor]
        public ReducedBandKey(string reportName,int reportId, string reportGrouping, string product, string headOrOther, GroupBySet bandSet, string cEvent)
        {
            this.reportName = reportName ?? throw new ArgumentNullException("reportName");
            this.reportGrouping = reportGrouping ?? throw new ArgumentNullException("reportGrouping");
            this.product = product ?? throw new ArgumentNullException("product");
            this.headOrOther = headOrOther ?? throw new ArgumentNullException("headOrOther");
            this.bandSet = bandSet;
            this.cEvent = cEvent;
            this.cacheKey = Guid.NewGuid().ToString();
            this.reportId = reportId;
        }

        [JsonProperty("reportName")]
        public string ReportName
        {
            get
            {
                return this.reportName;
            }
        }
        [JsonProperty("reportId")]
        public int ReportId
        {
            get
            {
                return this.reportId;
            }
        }
        [JsonProperty("reportGrouping")]
        public string ReportGrouping
        {
            get
            {
                return this.reportGrouping;
            }
        }

        [JsonProperty("product")]
        public string Product
        {
            get
            {
                return this.product;
            }
        }

        [JsonProperty("headOrOther")]
        public string HeadOrOther
        {
            get
            {
                return this.headOrOther;
            }
        }

        [JsonProperty("bandSet")]
        public GroupBySet BandSet
        {
            get
            {
                return this.bandSet;
            }
        }

        [JsonProperty("cEvent")]
        public string CEvent
        {
            get
            {
                return this.cEvent;
            }
        }


        [JsonProperty("cacheKey")]
        public string CacheKey
        {
            get
            {
                return this.cacheKey;
            }
        }

        public string ToKeyString()
        {
            return this.reportName + this.reportGrouping + this.product + headOrOther + bandSet.ToKeyString() + ":" + this.cEvent;
        }


    }
}
