using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.UDR
{
    public class Geography
    {
        private readonly long geoAreaId;
        private readonly string worldRegionCd;
        private readonly string regionRefId;

        public Geography(
            long geoAreaId,
            string worldRegionCd,
            string regionRefId)
        {
            this.geoAreaId = geoAreaId;
            this.worldRegionCd = worldRegionCd ?? throw new ArgumentNullException("worldRegionCd");
            this.regionRefId = regionRefId ?? throw new ArgumentNullException("regionRefId");
        }


        public long GeoAreaId { get { return this.geoAreaId; } }
        public string WorldRegionCd { get { return this.worldRegionCd; } }
        public string RegionRefId { get { return this.regionRefId; } }
    }
}
