using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.UDR
{
    public static class GeographyKeyFactory
    {
        private const string countryFieldName = "country";
        private const string subCountryFieldName = "subCountry";
        private const string subSubCountryFieldName = "subSubCountry";
        private const string locationIdFieldName = "locationId";
        private const string fieldIdFieldName = "fieldId";

        public static GeographyKey GetGeographyKey(UDRScope udrScope, Dictionary<string, dynamic> row)
        {
            dynamic value;
            string country = null, subCountry = null, subSubCountry = null, locationId = null, fieldId = null; 
            if (udrScope >= UDRScope.FLD)
            {
                if (row.TryGetValue(fieldIdFieldName, out value) && value != null)
                    fieldId = value.ToString();
            }
            if (udrScope >= UDRScope.LCTN)
            {
                if (row.TryGetValue(locationIdFieldName, out value) && value != null)
                    locationId = value.ToString();
            }
            if (udrScope >= UDRScope.SSCTRY)
            {
                if (row.TryGetValue(subSubCountryFieldName, out value) && value != null)
                    subSubCountry = value.ToString();
            }
            if (udrScope >= UDRScope.SCTRY)
            {
                if (row.TryGetValue(subCountryFieldName, out value) && value != null)
                    subCountry = value.ToString();
            }
            if (udrScope >= UDRScope.CTRY)
            {
                if (row.TryGetValue(countryFieldName, out value) && value != null)
                    country = value.ToString();
            }
            return new GeographyKey(country, subCountry, subSubCountry, locationId, fieldId);
        }
    }
}
