using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class BandNotificationMessage
    {      
        private readonly string userId;
        private readonly string reportName;
        private readonly int reportId;
        private readonly string crop;
        private readonly string region;
        private readonly int year;
        private readonly bool compareHeads;
        private readonly string reportIdentifier;

        [JsonConstructor]
        public BandNotificationMessage(string userId, string reportName,int reportId, string crop, string region, int year, bool compareHeads,string reportIdentifier)
        {           
            this.userId = userId ?? throw new ArgumentNullException("userId");
            this.reportName = reportName ?? throw new ArgumentNullException("reportName");
            this.crop= crop ?? throw new ArgumentNullException("crop");
            this.region = region ?? throw new ArgumentNullException("region");
            this.year = year;
            this.compareHeads = compareHeads;
            this.reportId = reportId;
            this.reportIdentifier = reportIdentifier ?? throw new ArgumentNullException("reportIdentifier");
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

        [JsonProperty("crop")]
        public string Crop
        {
            get
            {
                return this.crop;
            }
        }
        [JsonProperty("region")]
        public string Region
        {
            get
            {
                return this.region;
            }
        }
        [JsonProperty("year")]
        public int Year
        {
            get
            {
                return this.year;
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
