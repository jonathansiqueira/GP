using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class Observation
    {
        private readonly string obsRefCd;
        private readonly string rankOrder;
        
        [JsonConstructor]
        public Observation(string refCode, string rankOrder)
        {
            this.obsRefCd = refCode ?? throw new ArgumentNullException("refCode");
            this.rankOrder = rankOrder ?? throw new ArgumentNullException("rankOrder");
        }

        [JsonRequired]
        [JsonProperty("refCode")]
        public string ObsRefCd
        {
            get
            {
                return this.obsRefCd;
            }
        }

        [JsonRequired]
        [JsonProperty("rankOrder")]
        public string RankOrder
        {
            get
            {
                return this.rankOrder;
            }
        }
    }
}
