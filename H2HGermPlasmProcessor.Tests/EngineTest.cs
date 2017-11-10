using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;

using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.TestUtilities;

using H2HGermPlasmProcessor;
using Moq;
using H2HGermPlasmProcessor.Data;
using System.Net.Http;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using Newtonsoft.Json;
using Enyim.Caching;
using System.Threading;
using Enyim.Caching.Memcached;
using System.Collections;
using H2HGermPlasmProcessor.Data.Impl;
using PingHelper;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.ReportData;
using Xunit.Abstractions;
using H2HGermPlasmProcessor.Data.UDR;

namespace H2HGermPlasmProcessor.Tests
{
    public class EngineTest
    {
        private readonly ITestOutputHelper output;

        private readonly Mock<IQueue> queueMock = new Mock<IQueue>();
        private readonly Mock<INotifier> notifierMock = new Mock<INotifier>();
        private readonly Mock<IUDRData> udrDataMock = new Mock<IUDRData>();
        private readonly IProductAnalyticsAPIClient productAnalyticsAPIClient;
        private readonly Mock<ILambdaContext> lambdaContextMock = new Mock<ILambdaContext>();
        private readonly Mock<IMemcachedClient> memcachedClientMock = new Mock<IMemcachedClient>();
        private readonly Mock<IHttpClientFactory> httpClientFactoryMock = new Mock<IHttpClientFactory>();
        private readonly Mock<IEncryptedEnvVariable> encryptedEnvVariableMock = new Mock<IEncryptedEnvVariable>();
        private readonly Mock<ISlackAPI> slackAPIMock = new Mock<ISlackAPI>();

        private readonly IHeadtoHeadAPIClient headToHeadAPIClient;

        private readonly MemoryCache memoryCache;
        private readonly LoggerFactory loggerFactory = new LoggerFactory();

        private HttpClient httpClient = new HttpClient(new HttpHandler());

        private Dictionary<string, object> store = new Dictionary<string, object>();

        public EngineTest(ITestOutputHelper output)
        {
            this.output = output;
            Environment.SetEnvironmentVariable("EntryMeansURL", "https://api-t.monsanto.com/productanalyticsapi/search/by-loc-no-ped");
            Environment.SetEnvironmentVariable("ServiceClientID", "BREEDING-IT-HEAD-TO-HEAD-ENGINE-SVC");
            Environment.SetEnvironmentVariable("ServiceClientSecret", "AAA");
            Environment.SetEnvironmentVariable("H2hAPI", "https://api01-np.agro.services/headtoheadapi/engine-progress");
            Environment.SetEnvironmentVariable("enyimMemcached", @"{""Servers"": [{""Address"": ""http:\\\\localhost\\memcached"",""Port"": 11211}]}");
            Environment.SetEnvironmentVariable("FieldObservationsURL", "https://api-t.monsanto.com/productanalyticsapi//fields/{fieldId}/observations");
            Environment.SetEnvironmentVariable("FieldStressURL", "https://api-t.monsanto.com/productanalyticsapi/fields/stresses");
            Environment.SetEnvironmentVariable("UDRByCropURL", "https://api-t.monsanto.com/breeding/v1/field-data/ref/region/geography/{crop}");
            Environment.SetEnvironmentVariable("SlackWebhookUrl", "https://hooks.slack.com/services/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");
            Environment.SetEnvironmentVariable("SlackAppName", "H2H Engine");

            Environment.SetEnvironmentVariable("HeadToHeadAPIReportStatus", "https://api01-np.agro.services/headtoheadapi/report");


            //Environment.SetEnvironmentVariable("HeadToHeadAPIReporStatus", "http://localhost:1234/report");

            Environment.SetEnvironmentVariable("OauthUrl", "https://pingAPIUrl");
            Environment.SetEnvironmentVariable("ServiceClientID", "MyClientID");
            Environment.SetEnvironmentVariable("ServiceClientSecret", Convert.ToBase64String(Encoding.ASCII.GetBytes("SuperSecretClient")));
            Environment.SetEnvironmentVariable("TokenTimeoutHoursDecimal", "0.01");

            List<Guid> correlationdIds = new List<Guid>() { Guid.NewGuid() };
            int counter = 0;
            queueMock.Setup(q => q.GetNext(It.IsAny<ILambdaContext>())).Returns(() =>
            {
                try
                {
                    switch (counter)
                    {
                        case 0:
                            return GetHeadRequest(correlationdIds);
                        case 1:
                            return GetTailRequest(correlationdIds);
                        default:
                            return null;
                    }
                }
                finally
                {
                    counter++;
                }
            });

            productAnalyticsAPIClient = new ProductAnalyticsAPIClient();
            headToHeadAPIClient = new HeadtoHeadAPIClient();

            setupHttpClientFactoryMock();
            SetupEncryptedEnvVariableMock();
            MemoryCacheOptions options = new MemoryCacheOptions()
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1.0)
            };
            memoryCache = new MemoryCache(options);

        }

        private IEnumerable<UDRGeography> GetGeographies()
        {
            UDRGeography[] geographies = new UDRGeography[]
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
                },
                new UDRGeography() {
                    countryName = "United States of America",
                    subCountryName = "Missouri",
                    cropName ="Apples",
                    generalGeoTypeCd ="SCTRY",
                    geoAreaId = 3,
                    regionRefId = "NA",
                    userDefRegionId = 2,
                    worldRegionCd = "NA",
                    userDefRegionName = "Missouri"
                }
            };
            return geographies;
        }

        private UDRList GetSimpleUDRList()
        {
            return new UDRList(GetGeographies());

        }

        private void setupHttpClientFactoryMock()
        {
            httpClientFactoryMock.Setup(m => m.CreateClient(It.IsAny<HttpMessageHandler>()))
                .Returns(() =>
                {
                    return new HttpClient(new HttpHandler(), true);
                });
            httpClientFactoryMock.Setup(m => m.CreateClient())
                .Returns(() =>
                {
                    return new HttpClient(new HttpHandler(), true);
                });
        }

        private void SetupEncryptedEnvVariableMock()
        {
            encryptedEnvVariableMock.Setup(m => m.DecodeEnvVarAsync(It.IsAny<string>())).ReturnsAsync((string key) =>
            {
                return Environment.GetEnvironmentVariable(key);
            });
        }

        ~EngineTest()
        {
            httpClient.Dispose();
            httpClient = null;
        }

        private const string bandJson =
@"[
{
      ""bandName"": ""Planting Date"",
      ""bandingGroup"": ""Agronomics""
    }, 
    {
      ""bandName"": ""Experiment Type"",
      ""bandingGroup"": ""Experiment""
    },  
    {
          ""bandName"": ""RM-100"",
          ""category"": ""Geography"",
          ""bandingGroup"": ""UDR""
    },
    {
          ""bandName"": ""RM-105"",
          ""category"": ""Geography"",
          ""bandingGroup"": ""UDR""
    },
    {
          ""bandName"": ""RM-110"",
          ""category"": ""Geography"",
          ""bandingGroup"": ""UDR""
    }    
]";

        private List<BandDefinition> GetBandDefintions()
        {
            return JsonConvert.DeserializeObject<List<BandDefinition>>(bandJson);
        }

//        private QueueGermPlasmEvent GetHeadRequest(List<Guid> correlationIds)
//        {
//            return new QueueGermPlasmEvent(
//                correlationIds,
//                "KFULT",
//                "My Report",
//                new List<string>()
//                {
//                    "testSetSeason=2015:04",
////                    "UserDefinedRegions=North America&Missouri"
//                },
//                new List<Observation>()
//                {
//                    new Observation("MST", "D"),
//                    new Observation("YLD_BE", "A"),
//                    new Observation("SELIN", "D"),
//                    new Observation("TWT_BE", "D"),
//                },
//                "By Test",
//                GetBandDefintions(),
//                "Head",
//                new Germplasm(879542209164695),
//                //new Germplasm(879548822673502),
//                //new Germplasm(591982906376192),
//                "Corn",
//                "NA",
//                2016
//                );
//        }


        private QueueGermPlasmEvent GetHeadRequest(List<Guid> correlationIds)
        {
            return new QueueGermPlasmEvent(
                correlationIds,
                "JSIQU",
                "My Report",
                71,
                new List<string>()
                {
                    "testSetSeason=2015:04",
                    "HarvestType=G&S",
                    "experTypeRefId=R&TD",
                    "experStageRefId=CM&AC",
//                    "UserDefinedRegions=North America&Missouri"
                },
                new List<Observation>()
                {
                    new Observation("MST", "D"),
                    new Observation("YLD_BE", "A"),
                    new Observation("SELIN", "D"),
                    new Observation("TWT_BE", "D"),
                },
                "By Test",
                GetBandDefintions(),
                "Head",
                new Germplasm(879542209164695),
                //new Germplasm(879548822673502),
                //new Germplasm(591982906376192),
                "Corn",
                "NA",
                2016,
                "North America"
                );
        }

        private QueueGermPlasmEvent GetTailRequest(List<Guid> correlationIds)
        {
            return new QueueGermPlasmEvent(
                correlationIds,
                "JSIQU",
                "My Report",
                71,
                new List<string>()
                {
                    "testSetSeason=2015:04",
                },
                new List<Observation>()
                {
                    new Observation("MST", "D"),
                    new Observation("YLD_BE", "A"),
                    new Observation("SELIN", "D"),
                    new Observation("TWT_BE", "D"),
                },
                "By Test",
                GetBandDefintions(),
                "Other",
                new Germplasm(879545959415910),
                //new Germplasm(591982906376192),
                "Corn",
                "NA",
                2017,
                null
                );
        }

        [Fact]
        public void TestFunction()
        {
            const string gpQueueUrl = "http://bogus/gpQueueUrl";            
            const int myCount = 12;
            const string userId = "KFULT";
            const string reportName = "reportName";
            const int reportId = 71;
            const string reportIdentifier = "reportName_71";
            GermPlasmSNSRequest request = new GermPlasmSNSRequest(
                gpQueueUrl,             
                myCount,
                userId: userId,
                reportName: reportName,
                reportId:reportId,
                isMetric: false,
                compareHeads: true, 
                reportIdentifier: reportIdentifier);

            SNSEvent evnt = new SNSEvent
            {
                Records = new List<SNSEvent.SNSRecord>
                {
                    new SNSEvent.SNSRecord
                    {
                        EventSource = "Test",
                        Sns = new SNSEvent.SNSMessage
                        {
                            Message = JsonConvert.SerializeObject(request),
                            TopicArn = "topic"
                        }
                    }
                }
            };

            Dictionary<long, ReducedEntryMeans> output = new Dictionary<long, ReducedEntryMeans>();

            List<ReducedBandKey> keys = new List<ReducedBandKey>();

            Dictionary<string, ulong> counters = new Dictionary<string, ulong>()
            {
                { "My`Report_GP_Count", 2 }
            };
            memcachedClientMock.Setup(m => m.Decrement(It.IsAny<string>(), It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<TimeSpan>())).Returns((string key, ulong defaultValue, ulong delta, TimeSpan expires) =>
            {
                counters[key] -= delta;
                return counters[key];
            });
            memcachedClientMock.Setup(m => m.Store(It.IsAny<StoreMode>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>())).Returns((StoreMode mode, string key, object value, TimeSpan expires) =>
            {
                if (mode == StoreMode.Set)
                {
                    store[key] = value;
                    if (value is ReducedBandKey)
                    {
                        keys.Add((ReducedBandKey)value);
                    }
                }
                else if (mode == StoreMode.Add)
                {
                    if (value is ReducedBandKey)
                    {
                        keys.Add((ReducedBandKey)value);
                    }
                    if (store.ContainsKey(key))
                    {
                        ((IList)(store[key])).Add(value);
                    }
                    else
                    {
                        IList list = new ArrayList();
                        list.Add(value);
                        store[key] = list;
                    }
                }
                return true;
            });
            memcachedClientMock.Setup(m => m.StoreAsync(It.IsAny<StoreMode>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>())).ReturnsAsync((StoreMode mode, string key, object value, TimeSpan expires) =>
            {
                if (mode == StoreMode.Set)
                {
                    store[key] = value;
                    if (value is ReducedBandKey)
                    {
                        keys.Add((ReducedBandKey)value);
                    }
                }
                else if (mode == StoreMode.Add)
                {
                    if (value is ReducedBandKey)
                    {
                        keys.Add((ReducedBandKey)value);
                    }
                    if (store.ContainsKey(key))
                    {
                        ((IList)(store[key])).Add(value);
                    }
                    else
                    {
                        IList list = new ArrayList();
                        list.Add(value);
                        store[key] = list;
                    }
                }
                return true;
            });

            TestLambdaContext context = new TestLambdaContext();
            context.RemainingTime = new TimeSpan(0, 0, 5, 0, 0);

            CachePersister persister = new CachePersister(memcachedClientMock.Object);

            PingSettingsOptions pingSettingsOptions = new PingSettingsOptions()
            {
                ClientID = "MyClientID",
                ClientSecret = Convert.ToBase64String(Encoding.ASCII.GetBytes("SuperSecretClient")),
                PingAPIUrl = "https://pingAPIUrl",
                PingTokenTimeout = TimeSpan.FromSeconds(10)
            };

            var credentials = new PingSettings(Options.Create(pingSettingsOptions));

            PingIDClient client = new PingIDClient(credentials, memoryCache, httpClientFactoryMock.Object, loggerFactory);

            udrDataMock.Setup(m => m.GetUDRsForCrop(It.IsAny<HttpClient>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(() =>
                {
                    return GetGeographies();
                });

            ISlackAPI slack = new SlackAPI(Options.Create<SlackAPIOptions>(new SlackAPIOptions()
            {
                SlackWebhookUrl = Environment.GetEnvironmentVariable("SlackWebhookUrl"),
                SlackAppName = Environment.GetEnvironmentVariable("SlackAppName")
            }));
            Engine engine = new Engine(
                queueMock.Object,
                notifierMock.Object,
                productAnalyticsAPIClient,
                persister,
                udrDataMock.Object,
                httpClientFactoryMock.Object,
                client,
                encryptedEnvVariableMock.Object,
                slack, headToHeadAPIClient);

            engine.ProcessEvent(evnt, context);

            var testLogger = context.Logger as TestLambdaLogger;
            Assert.True(testLogger.Buffer.ToString().Contains("SNS event processing is complete."));
          // Assert.NotEqual(0, keys.Count);

            string pairContents;
            string pairKey;
            foreach (KeyValuePair<string, object> pair in this.store)
            {
                pairKey = pair.Key;
                pairContents = JsonConvert.SerializeObject(pair.Value);
                this.output.WriteLine($"{pairKey}:{pairContents}");
            }
            // Add asserts to lock in the test results and processing
        }
    }

    public class HttpHandler : HttpClientHandler
    {
        private int gpcounter = 1;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string url = request.RequestUri.ToString();
            string content = await request.Content.ReadAsStringAsync();
            string responseString;
            HttpResponseMessage response;

            if (url.Equals("https://api-t.monsanto.com/productanalyticsapi/search/by-loc-no-ped?germplasmId=111111111111&cropName=Corn&harvestYear=2015"))
                throw new Exception("There was an error in calling the productAnalytics API");


            if (url == "https://api01-np.agro.services/headtoheadapi/engine-progress/germplasm-current-count?intializeId=12")
            {
                responseString = $"{{\"countTotal\":2,\"countCurrent\":{gpcounter++}}}";
            }
            else
            {
                responses.TryGetValue($"{url};{content}", out responseString);
            }
            if (!string.IsNullOrEmpty(responseString))
            {
                response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                response.RequestMessage = request;
                response.Content = new StringContent(responseString);
                return response;
            }

            response = await base.SendAsync(request, cancellationToken);

            responseString = await response.Content.ReadAsStringAsync();

            return response;
        }

        private static Dictionary<string, string> responses = new Dictionary<string, string>()
        {
            {"https://pingapiurl/;client_id=MyClientID&client_secret=U3VwZXJTZWNyZXRDbGllbnQ%3D&grant_type=client_credentials", "{\"access_token\":\"PxsadxSROXhHOGBAUINdMPIhsQWL\",\"token_type\":\"Bearer\",\"expires_in\":7199}"},
            {"https://api-t.monsanto.com/productanalyticsapi/search/by-loc-no-ped?germplasmId=879542209164695&cropName=Corn&regionName=North America&harvestYear=2015;{ \"ObsRefCode\":[\"MST\",\"YLD_BE\",\"SELIN\",\"TWT_BE\"],\"TestSetSeasons\": [\"2015:04\"],\"HarvestTypes\": [\"G\",\"S\"],\"ExperTypeRefIds\": [\"R\",\"TD\"],\"ExperStageRefIds\": [\"CM\",\"AC\"] }", "[{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328218,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IAHA\",\"locationId\":1341661904896,\"fieldName\":\"BHA1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":18.44,\"checkMean\":19.7250,\"testMean\":19.80708029197080,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4696621750},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328218,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IAHA\",\"locationId\":1341661904896,\"fieldName\":\"BHA1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":74.87255016564780,\"checkMean\":77.15456953375890,\"testMean\":76.07070024515220,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4696621750},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328222,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEST\",\"locationId\":1341669834752,\"fieldName\":\"HST2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":70.71398152666620,\"checkMean\":72.25742935056270,\"testMean\":71.38059745448620,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4566411119},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328227,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEKE\",\"locationId\":1341663150080,\"fieldName\":\"PKE2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"SELIN\",\"traitAbbrevName\":\"SELIN\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":125239296,\"englishUomRefId\":null,\"englishUomName\":\"Number\",\"metricUomId\":125239296,\"metricUomRefId\":null,\"metricUomName\":\"Number\",\"metricToEnglishFactor\":1.0,\"entryMean\":93.143259,\"checkMean\":99.99999978571430,\"testMean\":119.312086764706,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4919712774},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328212,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEDA\",\"locationId\":879545973225253,\"fieldName\":\"IDA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"SELIN\",\"traitAbbrevName\":\"SELIN\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":125239296,\"englishUomRefId\":null,\"englishUomName\":\"Number\",\"metricUomId\":125239296,\"metricUomRefId\":null,\"metricUomName\":\"Number\",\"metricToEnglishFactor\":1.0,\"entryMean\":119.305641,\"checkMean\":99.99999992857140,\"testMean\":107.003625424460,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437823},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328219,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IAIA\",\"locationId\":377028731666432,\"fieldName\":\"BIA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":67.50616552251720,\"checkMean\":69.57172663728870,\"testMean\":68.67103109253420,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4936057675},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328222,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEST\",\"locationId\":1341669834752,\"fieldName\":\"HST2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":151.503325213131,\"checkMean\":172.088300120823,\"testMean\":176.924969845593,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4566411119},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328222,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEST\",\"locationId\":1341669834752,\"fieldName\":\"HST2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"SELIN\",\"traitAbbrevName\":\"SELIN\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":125239296,\"englishUomRefId\":null,\"englishUomName\":\"Number\",\"metricUomId\":125239296,\"metricUomRefId\":null,\"metricUomName\":\"Number\",\"metricToEnglishFactor\":1.0,\"entryMean\":60.029589,\"checkMean\":100.0,\"testMean\":107.499922350746,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4566411119},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328227,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEKE\",\"locationId\":1341663150080,\"fieldName\":\"PKE2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":146.802722,\"checkMean\":151.590676785714,\"testMean\":163.744949757353,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4919712774},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328228,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IACN\",\"locationId\":1341658955776,\"fieldName\":\"ZCN1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":139.848213125959,\"checkMean\":133.290999284054,\"testMean\":150.319151755115,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437843},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328229,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IADV\",\"locationId\":588508789080064,\"fieldName\":\"ZDV3\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":18.74,\"checkMean\":18.73714285714290,\"testMean\":18.7440,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4926481774},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328230,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"IAGR\",\"locationId\":588508788293632,\"fieldName\":\"ZGR2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":167.161668360705,\"checkMean\":149.782248020032,\"testMean\":159.895303373154,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4926481830},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"brRepId\":879546627328212,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":25,\"locationRefId\":\"NEDA\",\"locationId\":879545973225253,\"fieldName\":\"IDA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937386918,\"germplasmId\":879545941954396,\"pedigreeName\":\"DIQO514+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":21.15,\"checkMean\":20.89071428571430,\"testMean\":21.4220,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437823}]" },
            {"https://api-t.monsanto.com/productanalyticsapi/search/by-loc-no-ped?germplasmId=879545959415910&cropName=Corn&harvestYear=2015;{ \"ObsRefCode\":[\"MST\",\"YLD_BE\",\"SELIN\",\"TWT_BE\"],\"TestSetSeasons\": [\"2015:04\"],\"HarvestTypes\": [\"G\",\"S\"],\"ExperTypeRefIds\": [\"R\",\"TD\"],\"ExperStageRefIds\": [\"CM\",\"AC\"] }", "[{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328228,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IACN\",\"locationId\":1341658955776,\"fieldName\":\"ZCN1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":21.01,\"checkMean\":20.39928571428570,\"testMean\":20.42413533834590,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437843,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328212,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEDA\",\"locationId\":879545973225253,\"fieldName\":\"IDA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":155.712374373095,\"checkMean\":152.303103455238,\"testMean\":158.420444854039,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437823,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.02\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328227,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEKE\",\"locationId\":1341663150080,\"fieldName\":\"PKE2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":162.714724,\"checkMean\":151.590676785714,\"testMean\":163.744949757353,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4919712774,\"soilTypeName\":null,\"tillageName\":\"Conservation: No-Till\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"Y\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":null,\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328212,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEDA\",\"locationId\":879545973225253,\"fieldName\":\"IDA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"SELIN\",\"traitAbbrevName\":\"SELIN\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":125239296,\"englishUomRefId\":null,\"englishUomName\":\"Number\",\"metricUomId\":125239296,\"metricUomRefId\":null,\"metricUomName\":\"Number\",\"metricToEnglishFactor\":1.0,\"entryMean\":92.391754,\"checkMean\":99.99999992857140,\"testMean\":107.003625424460,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437823,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.02\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328218,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IAHA\",\"locationId\":1341661904896,\"fieldName\":\"BHA1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":75.66612209790240,\"checkMean\":77.15456953375890,\"testMean\":76.07070024515220,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4696621750,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328220,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEHA\",\"locationId\":1341662035968,\"fieldName\":\"HHA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":75.133582819274,\"checkMean\":75.58035725934750,\"testMean\":74.78258467819150,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4567129373,\"soilTypeName\":null,\"tillageName\":\"Conservation: Ridge-Till\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"Y\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":null,\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328223,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NETK\",\"locationId\":879545817975999,\"fieldName\":\"HTK2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":19.12,\"checkMean\":19.30428571428570,\"testMean\":19.97942857142860,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4567129370,\"soilTypeName\":null,\"tillageName\":\"Conservation: Strip-Till\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"Y\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":null,\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328214,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"ILMN\",\"locationId\":3504040247296,\"fieldName\":\"MMN2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"SELIN\",\"traitAbbrevName\":\"SELIN\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":125239296,\"englishUomRefId\":null,\"englishUomName\":\"Number\",\"metricUomId\":125239296,\"metricUomRefId\":null,\"metricUomName\":\"Number\",\"metricToEnglishFactor\":1.0,\"entryMean\":142.006926,\"checkMean\":99.99999992857140,\"testMean\":123.438013809859,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4807063771,\"soilTypeName\":null,\"tillageName\":\"Minimum\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328212,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEDA\",\"locationId\":879545973225253,\"fieldName\":\"IDA2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":74.738326687575,\"checkMean\":76.61279535840520,\"testMean\":75.23537338958190,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4916437823,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.02\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328229,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IADV\",\"locationId\":588508789080064,\"fieldName\":\"ZDV3\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":19.25,\"checkMean\":18.73714285714290,\"testMean\":18.7440,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4926481774,\"soilTypeName\":null,\"tillageName\":\"Minimum\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328218,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IAHA\",\"locationId\":1341661904896,\"fieldName\":\"BHA1\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"MST\",\"traitAbbrevName\":\"MST\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":105644032,\"englishUomRefId\":\"%\",\"englishUomName\":\"Percent\",\"metricUomId\":105644032,\"metricUomRefId\":\"%\",\"metricUomName\":\"Percent\",\"metricToEnglishFactor\":1.0,\"entryMean\":20.41,\"checkMean\":19.7250,\"testMean\":19.80708029197080,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4696621750,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328229,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IADV\",\"locationId\":588508789080064,\"fieldName\":\"ZDV3\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"YLD_BE\",\"traitAbbrevName\":\"YLD_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":169869312,\"englishUomRefId\":null,\"englishUomName\":\"Bushels(56#)/Acre\",\"metricUomId\":169934848,\"metricUomRefId\":null,\"metricUomName\":\"Quintals/Hectare\",\"metricToEnglishFactor\":1.59322002440041,\"entryMean\":149.502614,\"checkMean\":151.940942428571,\"testMean\":158.977931540741,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4926481774,\"soilTypeName\":null,\"tillageName\":\"Minimum\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328222,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NEST\",\"locationId\":1341669834752,\"fieldName\":\"HST2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":71.88874786713780,\"checkMean\":72.25742935056270,\"testMean\":71.38059745448620,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4566411119,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"Y\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":null,\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328223,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"NETK\",\"locationId\":879545817975999,\"fieldName\":\"HTK2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":75.10751884209610,\"checkMean\":75.52936469902550,\"testMean\":74.52652906298140,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4567129370,\"soilTypeName\":null,\"tillageName\":\"Conservation: Strip-Till\",\"previousCrop\":\"Soybeans\",\"isIrrigated\":\"Y\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":null,\"growthStressText\":\"Overall - None\"},{\"programRefId\":\"8A\",\"testStageYear\":2015,\"avgRelativeMaturity\":110.0,\"testSetId\":879546627328066,\"testSetName\":\"N7P11\",\"testSetSeason\":\"2015:04\",\"harvestTypeRefId\":\"G\",\"harvestTypeName\":\"Grain\",\"brRepId\":879546627328230,\"trackIntentId\":879546478418385,\"experTypeRefId\":\"R\",\"experStageRefId\":\"P1\",\"entryNumber\":96,\"locationRefId\":\"IAGR\",\"locationId\":588508788293632,\"fieldName\":\"ZGR2\",\"fieldId\":879551385929640,\"country\":\"United States of America\",\"subCountry\":\"Missouri\",\"geneticMaterialId\":879545937385848,\"germplasmId\":879545941796113,\"pedigreeName\":\"GEQU829+DILU285\",\"observationRefCd\":\"TWT_BE\",\"traitAbbrevName\":\"TWT_BE\",\"observationTypeName\":\"Plant Observation\",\"englishUomId\":135266304,\"englishUomRefId\":null,\"englishUomName\":\"Pounds/Bushel\",\"metricUomId\":141492224,\"metricUomRefId\":null,\"metricUomName\":\"Kilograms/Hectoliter\",\"metricToEnglishFactor\":0.7768885089491250,\"entryMean\":73.51564623986810,\"checkMean\":74.30582461076270,\"testMean\":72.90683381713250,\"crop\":\"Corn\",\"harvestYear\":2015,\"geographyDimKey\":4926481830,\"soilTypeName\":null,\"tillageName\":\"Conventional\",\"previousCrop\":\"Corn-Grain\",\"isIrrigated\":\"N\",\"droughtCategoryName\":\"Drought\",\"fieldStressScore\":\"0.01\",\"growthStressText\":\"Overall - None\"}]" },
            {"https://api-t.monsanto.com/productanalyticsapi//fields/879551385929640/observations;{\"ObsRefCodes\": [\"Fungicide Applied\",\"Is Irrigated\",\"Previous Crop\",\"Relative Planting Date\",\"Root Zone Soil Texture\",\"Tillage Type\",\"ASR_FSR\",\"CERCZM_FSP\",\"CORBNE_FSP\",\"SETOTU_FSP\"] }", "[{\"ObsRefCd\":\"PRECRP\",\"Name\":\"Previous Crop\",\"StrValue\":\"Soybeans\",\"DtValue\":null,\"NumValue\":null,\"Repetition\":1},{\"ObsRefCd\":\"ISIRR\",\"Name\":\"Is Irrigated\",\"StrValue\":\"Yes\",\"DtValue\":null,\"NumValue\":null,\"Repetition\":1},{\"ObsRefCd\":\"TLTYP\",\"Name\":\"Tillage Type\",\"StrValue\":\"Minimum\",\"DtValue\":null,\"NumValue\":null,\"Repetition\":1},{\"ObsRefCd\":\"RLPLT\",\"Name\":\"Relative Planting Date\",\"StrValue\":\"Normal\",\"DtValue\":null,\"NumValue\":null,\"Repetition\":1}]"},
        };

    }

}