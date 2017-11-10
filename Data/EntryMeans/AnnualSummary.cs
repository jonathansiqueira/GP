using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    [Serializable]
    public class AnnualSummary
    {
        private Dictionary<string, ObservationValueCollection> observationsByYear;

        public const string YearEntryMeanColumn = "testStageYear";

        [JsonConstructor]
        public AnnualSummary(Dictionary<string, ObservationValueCollection> observationsByYear)
        {
            this.observationsByYear = observationsByYear ?? new Dictionary<string, ObservationValueCollection>();
        }

        [JsonProperty("observationsByYear")]
        public Dictionary<string, ObservationValueCollection> ObservationsByYear
        {
            get
            {
                return this.observationsByYear;
            }
        }
    }
}
