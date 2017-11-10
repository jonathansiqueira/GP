using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class ObservationBandItem<T>
    {
        private readonly string headerBandName;
        private readonly T lowValue;
        private readonly T highValue;

        public ObservationBandItem(string bandName, T lowValue, T highValue)
        {
            this.headerBandName = $"{bandName} {lowValue}-{highValue}";
            this.lowValue = lowValue;
            this.highValue = highValue;
        }

        public string HeaderBandName
        {
            get
            {
                return this.headerBandName;
            }
        }

        public T LowValue
        {
            get
            {
                return this.lowValue;
            }
        }

        public T HighValue
        {
            get
            {
                return this.highValue;
            }
        }
    }
}
