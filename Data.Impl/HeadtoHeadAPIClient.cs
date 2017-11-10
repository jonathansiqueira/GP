using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class HeadtoHeadAPIClient : IHeadtoHeadAPIClient
    {
        private readonly string headToHeadClientAPI;

        public HeadtoHeadAPIClient()
        {

            this.headToHeadClientAPI = Environment.GetEnvironmentVariable("HeadToHeadAPIReportStatus");          
        }

        public void Initialize(HttpClient httpClient, string bearerToken, string userId)
        {
            //please remove bearer token
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken);
            httpClient.DefaultRequestHeaders.Add("user-id", userId);
        }

        public async Task<string> ReportFailure(ILambdaContext context, HttpClient httpClient, int reportId, string statusMessage)
        {
          
            StringContent requestContent = new StringContent(JsonConvert.SerializeObject(statusMessage), Encoding.UTF8, "application/json");          
            HttpResponseMessage message = await httpClient.PutAsync($"{headToHeadClientAPI}/"+ reportId + "/failed", requestContent, CancellationToken.None);
            return message.ToString();

        }

        public async Task<string> ReportSuccess(ILambdaContext context, HttpClient httpClient,  int reportId)
        {
            HttpResponseMessage message;
            try
            {
                var requestMessage = new HttpRequestMessage();
                requestMessage.Method = HttpMethod.Put;
                requestMessage.RequestUri = new Uri($"{headToHeadClientAPI}/" + reportId + "/succeeded");
                requestMessage.Content = new StringContent(string.Empty);
                requestMessage.Content.Headers.ContentLength = 0;
                requestMessage.Content.Headers.ContentType = null;


                message = await httpClient.SendAsync(requestMessage);
                return message.ToString();
            }
            catch (Exception ex)
            {
                throw;
            }

        }
    }
}
