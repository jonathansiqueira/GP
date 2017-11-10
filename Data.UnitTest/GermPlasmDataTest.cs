using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.Impl;
using H2HGermPlasmProcessor.Data.Model;
using H2HGermPlasmProcessor.Data.UDR;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace H2HGermPlasmProcessor.Data.UnitTest
{
    public class GermPlasmDataTest
    {
        private readonly Mock<ILambdaContext> contextMock = new Mock<ILambdaContext>();
        public GermPlasmDataTest()
        {
            Environment.SetEnvironmentVariable("EntryMeansURL", "https://api-t.monsanto.com/productanalyticsapi/search/by-loc");
        }

        private UDRList GetSimpleUDRList()
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
            return new UDRList(geographies);

        }

        public void Verify_Data_Returned()
        {
            ProductAnalyticsAPIClient data = new ProductAnalyticsAPIClient();
            long germPlasmId = 457931693752320;
            List<IFilter> postAPIFilters = new List<IFilter>();
            UDRList udrList = GetSimpleUDRList();
            IFilter[] filters = new List<IFilter>()
            {
                FilterBase.GetFilterDefinition("testSetSeason=2015:04", postAPIFilters, udrList)
            }.ToArray();
            List<string> observations = new List<string>()
            {
                "MST",
                "YLD_BE",
                "SELIN",
                "TWT_BE"
            };
            HttpClient httpClient = new HttpClient();
            List<Dictionary<string, dynamic>> entryMeans;

            CancellationTokenSource cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            entryMeans = data.GetEntryMeansAsynch(contextMock.Object, httpClient, cancellationToken.Token, "USER_A", germPlasmId, filters, observations, "Apples",null).Result;

            Assert.NotEqual<int>(0, entryMeans.Count);
            httpClient.Dispose();
            httpClient = null;
        }

        public class FiddlerAwareHandler : HttpClientHandler
        {
            public FiddlerAwareHandler()
                : base()
            {
                base.UseProxy = true;
                base.Proxy = new AllowFiddlerProxy();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return base.SendAsync(request, cancellationToken);
            }
        }

        public class AllowFiddlerProxy : IWebProxy
        {
            public ICredentials Credentials { get; set; }

            public Uri GetProxy(Uri destination)
            {
                return new Uri("http://localhost:8888");
            }

            public bool IsBypassed(Uri host)
            {
                return false;
            }
        }
    }
}
