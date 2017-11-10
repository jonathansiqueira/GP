using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Dto
{
    public class ProgressCountPair
    {
        private readonly int countTotal;
        private readonly int countCurrent;

        [JsonConstructor]
        public ProgressCountPair(int countTotal, int countCurrent)
        {
            if (countTotal <= 0) throw new ArgumentOutOfRangeException("We expect the total count to be 1 or more.");
            if (countCurrent <= 0) throw new ArgumentOutOfRangeException("We expect the current count to be 1 or greater by the time we get here.");

            this.countTotal = countTotal;
            this.countCurrent = countCurrent;
        }

        [JsonRequired, JsonProperty("countTotal")]
        public int CountTotal
        {
            get
            {
                return countTotal;
            }
        }

        [JsonRequired, JsonProperty("countCurrent")]
        public int CountCurrent
        {
            get
            {
                return countCurrent;
            }
        }
    }
}
