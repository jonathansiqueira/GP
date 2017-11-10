using Amazon.Lambda.Core;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using H2HGermPlasmProcessor.Data.EntryMeans;
using System.Collections.Generic;
using H2HGermPlasmProcessor.Data.Model;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class SQSWrapper : IQueue
    {
        private AmazonSQSClient client = null;
        ReceiveMessageRequest request = null;
        DeleteMessageRequest deleteRequest = null;
        private string gpQueueUrl;
        private string pairQueueUrl;
        private string processQueueUrl;

        ~SQSWrapper()
        {
            if (this.client != null)
            {
                this.client.Dispose();
            }
        }

        public void Initialize(GermPlasmSNSRequest request)
        {
            this.gpQueueUrl = request.GPQueueUrl;
            
            this.request = new ReceiveMessageRequest(gpQueueUrl);
            this.request.MaxNumberOfMessages = 1;
            this.request.WaitTimeSeconds = 0;
            this.request.VisibilityTimeout = 10;
            this.deleteRequest = new DeleteMessageRequest();
            this.deleteRequest.QueueUrl = gpQueueUrl;

            if (this.client != null)
            {
                this.client.Dispose();
            }
            this.client = new AmazonSQSClient();
        }

        public async Task DeleteGPQueueAsync()
        {
            DeleteQueueRequest request = new DeleteQueueRequest(this.gpQueueUrl);
            DeleteQueueResponse response = await this.client.DeleteQueueAsync(request);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed to delete queue: {this.gpQueueUrl} with code: {response.HttpStatusCode}");
            }
        }

        public QueueGermPlasmEvent GetNext(ILambdaContext context)
        {
            Message message = null;
            try
            {
                context.Logger.LogLine($"trying to receive messages with timeout: {this.client.Config.Timeout}.");
                CancellationTokenSource token = new CancellationTokenSource();
                Task<ReceiveMessageResponse> receivedMessageTask = client.ReceiveMessageAsync(this.request, token.Token);
                if (!receivedMessageTask.Wait(12000))
                {
                    context.Logger.LogLine($"message was not received");
                    try
                    {
                        token.Cancel();
                    }
                    catch (Exception)
                    {
                        context.Logger.LogLine($"receive message was cancelled.");
                        return null;
                    }
                }
                ReceiveMessageResponse receivedMessage = receivedMessageTask.Result;
                context.Logger.LogLine($"{receivedMessage.Messages.Count} messages received");
                message = receivedMessage.Messages.FirstOrDefault();
                if (message == null)
                    return null;
            }
            catch (Exception exc)
            {
                context.Logger.LogLine($"Exception: {exc}.");
                return null;
            }
            deleteRequest.ReceiptHandle = message.ReceiptHandle;

            DeleteMessageResponse deleteResponse = client.DeleteMessageAsync(deleteRequest).Result;
            if (deleteResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed to delete message {message.MessageId} from queue: {request.QueueUrl}: received message: {deleteResponse.HttpStatusCode}");
            }
            return JsonConvert.DeserializeObject<QueueGermPlasmEvent>(message.Body);
        }

        public async Task CreateMessageAsync(ReducedBandKey key)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            SendMessageRequest message = new SendMessageRequest(this.pairQueueUrl, JsonConvert.SerializeObject(key));
            SendMessageResponse response = await client.SendMessageAsync(message);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed to populate SQS: {this.pairQueueUrl} with code: {response.HttpStatusCode}");
            }
            return;
        }

        public async Task CreateMessageAsync(QueueGermPlasmEvent gpEvent)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            SendMessageRequest message = new SendMessageRequest(this.gpQueueUrl, JsonConvert.SerializeObject(gpEvent));
            SendMessageResponse response = await client.SendMessageAsync(message);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"failed to populate SQS: {this.pairQueueUrl} with code: {response.HttpStatusCode}");
            }
            return;
        }
    }
}
