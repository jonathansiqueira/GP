using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class UDRGeography
    {
        [JsonProperty]
        public long userDefRegionId { get; set; }
        [JsonProperty]
        public string generalGeoTypeCd { get; set; }
        [JsonProperty]
        public string cropName { get; set; }
        [JsonProperty]
        public string userDefRegionName { get; set; }

        [JsonProperty]
        public long geoAreaId { get; set; }
        [JsonProperty]
        public string worldRegionCd { get; set; }
        [JsonProperty]
        public string regionRefId { get; set; }
        [JsonProperty]
        public string countryName { get; set; }
        [JsonProperty]
        public string subCountryName { get; set; }
        [JsonProperty]
        public string subSubCountryName { get; set; }
        [JsonProperty]
        public string locationId { get; set; }
        [JsonProperty]
        public string fieldId { get; set; }
    }
}
