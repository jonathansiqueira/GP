using System;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using H2HGermPlasmProcessor.Data;
using System.Net.Http;
using H2HGermPlasmProcessor.Data.EntryMeans;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using H2HGermPlasmProcessor.Data.Model;
using H2HGermPlasmProcessor.Data.Impl;
using PingHelper;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace H2HGermPlasmProcessor
{
    public class Engine
    {
        private readonly IQueue queue;
        private readonly INotifier notifier;
        private readonly IProductAnalyticsAPIClient productAnalyticsAPIClient;
        private readonly IHeadtoHeadAPIClient headToHeadAPIClient;
        private readonly ICache persister;
        private readonly IUDRData udrData;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOAuthClient oauthClient;
        private readonly IEncryptedEnvVariable decryptVariable;
        private readonly ISlackAPI slack;

        private readonly static IServiceProvider serviceProvider;

        static Engine()
        {
            Startup startup = new Startup();
            ServiceCollection svcCollection = new ServiceCollection();
            startup.ConfigureServices(svcCollection);
            serviceProvider = svcCollection.BuildServiceProvider();
        }

        public Engine()
            : this(
                  serviceProvider.GetService<IQueue>(),
                  serviceProvider.GetService<INotifier>(),
                  serviceProvider.GetService<IProductAnalyticsAPIClient>(),
                  serviceProvider.GetService<ICache>(),
                  serviceProvider.GetService<IUDRData>(),
                  serviceProvider.GetService<IHttpClientFactory>(),
                  serviceProvider.GetService<IOAuthClient>(),
                  serviceProvider.GetService<IEncryptedEnvVariable>(),
                  serviceProvider.GetService<ISlackAPI>(),
                  serviceProvider.GetService<IHeadtoHeadAPIClient>()
                  )
        {
        }

        public Engine(
            IQueue queue, 
            INotifier notifier, 
            IProductAnalyticsAPIClient productAnalyticsAPIClient,
            ICache persister,
            IUDRData udrData,
            IHttpClientFactory httpClientFactory,
            IOAuthClient oauthClient,
            IEncryptedEnvVariable decryptVariable,
            ISlackAPI slack,
            IHeadtoHeadAPIClient headToHeadAPIClient
            )
        {
            this.queue = queue ?? throw new ArgumentNullException("queue");
            this.notifier = notifier ?? throw new ArgumentNullException("notifier");
            this.productAnalyticsAPIClient = productAnalyticsAPIClient ?? throw new ArgumentNullException("productAnalyticsAPIClient");
            this.persister = persister ?? throw new ArgumentNullException("persister");
            this.udrData = udrData ?? throw new ArgumentNullException("udrData");
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException("httpClientFactory");
            this.oauthClient = oauthClient ?? throw new ArgumentNullException("oauthClient");
            this.decryptVariable = decryptVariable ?? throw new ArgumentNullException("decryptVariable");
            this.slack = slack ?? throw new ArgumentNullException("slack");
            this.headToHeadAPIClient = headToHeadAPIClient?? throw new ArgumentNullException("headToHeadAPIClient");
        }

        ~Engine()
        {
            this.httpClientFactory.CreateClient().Dispose();
        }

        public void ProcessEvent(SNSEvent snsEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {snsEvent.Records.Count} records...");

            string bearerToken = $"Bearer {this.oauthClient.GetBearerTokenAsync().Result}";
            QueueProcessor processor = new QueueProcessor(queue, notifier, productAnalyticsAPIClient, persister, udrData, httpClientFactory.CreateClient(), bearerToken, slack, headToHeadAPIClient);
            foreach (SNSEvent.SNSRecord record in snsEvent.Records)
            {
                GermPlasmSNSRequest body = JsonConvert.DeserializeObject<GermPlasmSNSRequest>(record.Sns.Message);
                processor.Process(context, body, record.Sns.TopicArn);
            }
            context.Logger.LogLine($"SNS event processing is complete.");
        }

        private string GetMessageQueueName(SNSEvent.SNSMessage sns)
        {
            return sns.Message;
        }


    }
}
