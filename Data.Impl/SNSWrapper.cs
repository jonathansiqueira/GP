using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using H2HGermPlasmProcessor.Data.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using Amazon.Lambda.Core;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class SNSWrapper : INotifier
    {
        private readonly string bandProcessorTopicARN;

        public SNSWrapper()
        {
            bandProcessorTopicARN = Environment.GetEnvironmentVariable("BandProcessorTopicARN");
        }

        public async Task SendBandProcessStartAsync(BandNotificationMessage message)
        {
            var client = new AmazonSimpleNotificationServiceClient();
            var request = new PublishRequest(bandProcessorTopicARN, JsonConvert.SerializeObject(message));
            PublishResponse response = await client.PublishAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed sending a message to SNS: {response.HttpStatusCode}");
            }
        }

        public async Task SendSelfProcessStartAsync(ILambdaContext context, GermPlasmSNSRequest message, string topicArn)
        {
            context.Logger.Log($"starting another instance of this lambda by sending message to: {topicArn}.");

            var client = new AmazonSimpleNotificationServiceClient();
            var request = new PublishRequest(topicArn, JsonConvert.SerializeObject(message));
            PublishResponse response = await client.PublishAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed sending a message to source SNS: {response.HttpStatusCode}");
            }
            else
            {
                context.Logger.LogLine($"SNS response: {response.HttpStatusCode}; ID: {response.MessageId}");
            }
        }
    }
}
