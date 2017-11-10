using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class ColumnBand : BaseBand
    {
        public ColumnBand(string bandName) : base(bandName)
        {
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            object value;
            if (row.TryGetValue(this.BandName, out value))
            {
                return value;
            }
            return null;
        }
    }
}
