using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public static class AllBands
    {
        private static readonly Dictionary<string, Func<BandDefinition, BaseBand>> bandCreationDictionary = new Dictionary<string, Func<BandDefinition, BaseBand>>()
        {
            {"subSubCountry",  (BandDefinition definition) => { return new SubSubCountryBand(definition); }  }
        };

        public static Func<BandDefinition, BaseBand> GetCreator(string specialBandName)
        {
            Func<BandDefinition, BaseBand> func;
            if (bandCreationDictionary.TryGetValue(specialBandName, out func))
                return func;

            return null;
        }
    }
}
