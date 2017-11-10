using H2HGermPlasmProcessor.Data.Model;
using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace H2HGermPlasmProcessor.Data.UnitTest
{
    public class UDRTest
    {

        private IEnumerable<UDRGeography> GetGeographies()
        {
            UDRGeography[] geographies = new UDRGeography[2]
            {
                new UDRGeography() {
                    countryName = "Canada",
                    cropName ="Apples",
                    generalGeoTypeCd ="CTRY",
                    geoAreaId = 1,
                    regionRefId = "NA",
                    userDefRegionId = 1,
                    worldRegionCd = "NA",
                    userDefRegionName = "North America"
                },
                new UDRGeography() {
                    countryName = "United States of America",
                    cropName ="Apples",
                    generalGeoTypeCd ="CTRY",
                    geoAreaId = 2,
                    regionRefId = "NA",
                    userDefRegionId = 1,
                    worldRegionCd = "NA",
                    userDefRegionName = "North America"
                }
            };
            return geographies;
        }

        private UDRList GetSimpleUDRList()
        {
            return new UDRList(GetGeographies());

        }


        [Fact]
        public void Verify_UDR_List_Matches_Geography()
        {
            UDRList list = GetSimpleUDRList();
            Dictionary<string, dynamic> row = new Dictionary<string, dynamic>()
            {
                { "country", "Canada" }
            };
            Assert.Equal(true, list.ContainsUDR(new string[] { "North America" }, row));
        }

        [Fact]
        public void Verify_UDR_List_Does_Not_Match_Geography()
        {
            UDRList list = GetSimpleUDRList();
            Dictionary<string, dynamic> row = new Dictionary<string, dynamic>()
            {
                { "country", "Mexico" }
            };
            Assert.Equal(false, list.ContainsUDR(new string[] { "North America" }, row));
        }

    }
}
