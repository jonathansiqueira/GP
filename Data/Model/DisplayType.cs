using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    [Serializable]
    public enum DisplayType
    {
        [JsonProperty]
        Unknown = 0,
        [JsonProperty]
        Pedigree,
        [JsonProperty]
        PreCommercialProduct,
        [JsonProperty]
        Product,
        [JsonProperty]
        Alias
    }
}
