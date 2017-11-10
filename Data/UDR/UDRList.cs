using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.UDR
{
    public class UDRList
    {
        private readonly Dictionary<GeographyKey, HashSet<UDR>> UDRs = new Dictionary<GeographyKey, HashSet<UDR>>();
        private readonly HashSet<UDRScope> scopes = new HashSet<UDRScope>();

        public UDRList(IEnumerable<UDRGeography> geographies)
        {
            GeographyKey key;
            HashSet<UDR> udrs;
            UDR udr;
            foreach (UDRGeography geography in geographies)
            {
                key = new GeographyKey(geography.countryName, geography.subCountryName, geography.subSubCountryName, geography.locationId, geography.fieldId);
                if (!UDRs.TryGetValue(key, out udrs))
                {
                    udrs = new HashSet<UDR>();
                    UDRs.Add(key, udrs);
                }
                udr = new UDR(geography.userDefRegionId, geography.generalGeoTypeCd, geography.cropName, geography.userDefRegionName);
                if (!udrs.Contains(udr))
                {
                    udrs.Add(udr);
                }
                if (!scopes.Contains(udr.UdrScope))
                {
                    scopes.Add(udr.UdrScope);
                }
            }
        }

        public Dictionary<string, UDR> GetMatches(Dictionary<string, dynamic> row)
        {
            Dictionary<string, UDR> matchedUDRs = new Dictionary<string, UDR>();
            GeographyKey key;
            HashSet<UDR> udrs;
            foreach (UDRScope scope in scopes)
            {
                key = GeographyKeyFactory.GetGeographyKey(scope, row);
                if (this.UDRs.TryGetValue(key, out udrs))
                {
                    foreach(UDR udr in udrs)
                    {
                        if(!matchedUDRs.ContainsKey(udr.UserDefRegionName))
                        matchedUDRs.Add(udr.UserDefRegionName, udr);
                    }
                }
            }
            return matchedUDRs;
        }

        public bool ContainsUDR(IEnumerable<string> udrNames, Dictionary<string, dynamic> row)
        {
            Dictionary<string, UDR> matchedUDRs = GetMatches(row);
            if (matchedUDRs.Count == 0)
                return false;
            foreach(string udrName in udrNames)
            {
                if (matchedUDRs.ContainsKey(udrName))
                    return true;
            }
            return false;
        }
    }
}
