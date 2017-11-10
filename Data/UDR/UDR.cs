using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.UDR
{
    public class UDR : IEquatable<UDR>
    {
        private readonly long userDefRegionId;
        private readonly UDRScope udrScope;
        private readonly string cropName;
        private readonly string userDefRegionName;

        public UDR(long userDefRegionId, string generalGeoTypeCd, string cropName, string userDefRegionName)
        {
            this.userDefRegionId = userDefRegionId;
            if (!Enum.TryParse<UDRScope>(generalGeoTypeCd, out udrScope))
                throw new ArgumentOutOfRangeException("generalGeoTypeCd", $"{generalGeoTypeCd} is not handled");
            this.cropName = cropName ?? throw new ArgumentNullException("cropName");
            this.userDefRegionName = userDefRegionName ?? throw new ArgumentNullException("userDefRegionName");
        }

        public bool Equals(UDR other)
        {
            if (other == null)
                return false;
            return this.userDefRegionId.Equals(other.userDefRegionId);
        }

        public override int GetHashCode()
        {
            return userDefRegionId.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is UDR)
                return Equals((UDR)obj);
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return userDefRegionName;
        }

        public long UserDefRegionId {  get { return this.userDefRegionId; } }
        public UDRScope UdrScope { get { return this.udrScope; } }
        public string CropName { get { return this.cropName; } }
        public string UserDefRegionName { get { return this.userDefRegionName; } }

    }
}
