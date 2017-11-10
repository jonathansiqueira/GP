using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class GermPlasmSNSRequest
    {
        private readonly string gp;       
        private readonly ulong myCount;
        private readonly string userId;
        private readonly string reportName;       
        private readonly bool isMetric;
        private readonly bool compareHeads;
        private readonly int reportId;
        private readonly string reportIdentifier;

        [JsonConstructor]
        public GermPlasmSNSRequest(string gp,ulong myCount, string userId, string reportName,int reportId, bool isMetric, bool compareHeads,string reportIdentifier)
        {
            this.gp = gp ?? throw new ArgumentNullException("gp");
            this.myCount = myCount;
            this.userId = userId ?? throw new ArgumentNullException("userId");
            this.reportName = reportName ?? throw new ArgumentNullException("reportName");
            this.reportId = reportId;
            this.isMetric = isMetric;
            this.compareHeads = compareHeads;
            this.reportIdentifier = reportIdentifier ?? throw new ArgumentNullException("reportIdentifier");
        }

        [JsonProperty("gp")]
        public string GPQueueUrl
        {
            get
            {
                return this.gp;
            }
        }
      

        [JsonProperty("myCount")]
        public ulong MyCount
        {
            get
            {
                return this.myCount;
            }
        }

        [JsonProperty("userId")]
        public string UserId
        {
            get
            {
                return this.userId;
            }
        }

        [JsonProperty("reportName")]
        public string ReportName
        {
            get
            {
                return this.reportName;
            }
        }

        [JsonProperty("reportIdentifier")]
        public string ReportIdentifier
        {
            get
            {
                return this.reportIdentifier;
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

        [JsonProperty("isMetric")]
        public bool IsMetric
        {
            get
            {
                return this.isMetric;
            }
        }

        [JsonProperty("compareHeads")]
        public bool CompareHeads
        {
            get
            {
                return this.compareHeads;
            }
        }

    }
}
