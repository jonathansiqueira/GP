using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    [Serializable]
    [DataContract]
    [JsonObject]
    public class Germplasm
    {
        private readonly long germplasmId;
        private readonly string alias;
        private readonly DisplayType displayType = DisplayType.Pedigree;
        private readonly long productNameId;

        private string cEvent;

        [JsonConstructor]
        public Germplasm(long germplasmId, string alias = "", DisplayType displayType = DisplayType.Pedigree, long productNameId = 0)
        {
            this.germplasmId = germplasmId;
            this.alias = alias;
            this.displayType = displayType;
            this.productNameId = productNameId;
            this.cEvent = null;

            if (!string.IsNullOrEmpty(alias))
                this.displayType = DisplayType.Alias;
            else if (this.displayType == DisplayType.Unknown)
                this.displayType = DisplayType.Pedigree;
        }

        [JsonProperty("germplasmId")]
        public long GermplasmId
        {
            get
            {
                return germplasmId;
            }
        }

        [JsonProperty("alias")]
        public string Alias
        {
            get
            {
                return alias;
            }
        }

        [JsonProperty("displayType")]
        public DisplayType DisplayType
        {
            get
            {
                return displayType;
            }
        }

        [JsonProperty("productNameId")]
        public long ProductNameId
        {
            get
            {
                return productNameId;
            }
        }

        public string GetProduct()
        {
            if (!string.IsNullOrEmpty(alias))
                return $"{(int)displayType}|{alias}";
            if (displayType == DisplayType.Product)
                return $"{(int)displayType}|{germplasmId}|{productNameId}";
            return $"{(int)displayType}|{germplasmId}";
        }

        [JsonProperty("gpEvent")]
        public string CEvent
        {
            set
            {
                this.cEvent = value;
            }
            get
            {
                if (!string.IsNullOrEmpty(alias))
                    return "";
                return this.cEvent;
            }
        }

    }
}
