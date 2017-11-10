using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class SubSubCountryBand : BaseBand
    {
        private const string subCountryField = "subCountry";

        public SubSubCountryBand(BandDefinition definition) : base(definition.BandName)
        {

        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            object subSubCountry, subCountry;
            if (row.TryGetValue(this.BandName, out subSubCountry) && row.TryGetValue(subCountryField, out subCountry))
            {
                return $"{subCountry.ToString()} - {subSubCountry.ToString()}";
            }
            return null;

        }
    }
}
