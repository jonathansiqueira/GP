using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.UDR
{
    public class GeographyKey
    {
        private readonly string countryName;
        private readonly string subCountryName;
        private readonly string subSubCountryName;
        private readonly string fieldId;
        private readonly string locationId;
        private readonly long hashcode;

        public GeographyKey(
            string countryName,
            string subCountryName,
            string subSubCountryName,
            string locationId,
            string fieldId)
        {
            this.countryName = countryName;
            this.subCountryName = subCountryName;
            this.subSubCountryName = subSubCountryName;
            this.locationId = locationId;
            this.fieldId = fieldId;

            if (!string.IsNullOrEmpty(this.countryName))
                hashcode += this.countryName.GetHashCode();
            if (!string.IsNullOrEmpty(this.subCountryName))
                hashcode += this.subCountryName.GetHashCode() * 2L;
            if (!string.IsNullOrEmpty(this.subSubCountryName))
                hashcode += this.subSubCountryName.GetHashCode() * 4L;
            if (!string.IsNullOrEmpty(this.locationId))
                hashcode += this.locationId.GetHashCode() * 8L;
            if (!string.IsNullOrEmpty(this.fieldId))
                hashcode += this.fieldId.GetHashCode() * 16L;
        }

        public bool Equals(GeographyKey obj)
        {
            return this.hashcode == obj.hashcode;
        }

        public override bool Equals(object obj)
        {
            if (obj is GeographyKey)
                return Equals((GeographyKey)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return hashcode.GetHashCode();
        }

        public string CountryName { get { return this.countryName; } }
        public string SubCountryName { get { return this.subCountryName; } }
        public string SubSubCountryName { get { return this.subSubCountryName; } }
        public string LocationId { get { return this.locationId; } }
        public string FieldId { get { return this.fieldId; } }
    }
}
