using H2HGermPlasmProcessor.Data.Bands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    [DataContract]
    public class QueueGermPlasmEvent
    {
        [JsonConstructor]
        public QueueGermPlasmEvent(
            List<Guid> correlationIDs,
            string userId,
            string reportName,
            int reportId,
            List<string> dataFilters, 
            List<Observation> observations, 
            string analysisType, 
            List<BandDefinition> bands,
            string cat,
            Germplasm germplasm,
            string crop,
            string region,
            int year,
            string regionName
            )
        {
            this.CorrelationIDs = correlationIDs ?? throw new ArgumentNullException("correlationIDs");
            this.UserId = userId ?? throw new ArgumentNullException("userId");
            this.ReportName = reportName ?? throw new ArgumentNullException("reportName");
            this.DataFilters = dataFilters ?? throw new ArgumentNullException("dataFilters");
            this.Observations = observations ?? throw new ArgumentNullException("observations");
            this.AnalysisType = analysisType ?? throw new ArgumentNullException("analysisType");
            this.Bands = bands ?? throw new ArgumentNullException("bands");
            this.Category = cat ?? throw new ArgumentNullException("cat");
            this.Germplasm = germplasm ?? throw new ArgumentNullException("germplasm");
            this.Crop = crop ?? throw new ArgumentNullException("crop");
            this.Region = region ?? throw new ArgumentNullException("region");
            this.ReportId = reportId;
            this.Year = year;
            this.RegionName = regionName;
        }

        [JsonRequired]
        [JsonProperty("correlationIDs")]
        public List<Guid> CorrelationIDs { get; private set; }


        [JsonRequired]
        [JsonProperty("userId")]
        public string UserId { get; private set; }

        [JsonRequired]
        [JsonProperty("reportName")]
        public string ReportName { get; private set; }

        [JsonRequired]
        [JsonProperty("reportId")]
        public int ReportId { get; private set; }

        [JsonRequired]
        [JsonProperty("dataFilters")]
        public List<string> DataFilters { get; private set; }

        [JsonRequired]
        [JsonProperty("observations")]
        public List<Observation> Observations { get; private set; }

        [JsonRequired]
        [JsonProperty("analysisType")]
        public string AnalysisType { get; private set; }

        [JsonRequired]
        [JsonProperty("bands")]
        public List<BandDefinition> Bands { get; private set; }

        [JsonRequired]
        [JsonProperty("cat")]
        public string Category { get; private set; }

        [JsonRequired]
        [JsonProperty("germplasm")]
        public Germplasm Germplasm { get; private set; }

        [JsonRequired]
        [JsonProperty("crop")]
        public string Crop { get; private set; }

        [JsonRequired]
        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonRequired]
        [JsonProperty("year")]
        public int Year { get; private set; }
       
        [JsonProperty("regionName",Required = Required.Default)]
        public string RegionName { get; private set; }
    }
}
