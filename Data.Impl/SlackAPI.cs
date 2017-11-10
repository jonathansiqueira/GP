using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using PingHelper;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class SlackAPI : ISlackAPI
    {
        private readonly string slackWebhookUrl;
        private readonly string slackAppName;

        public SlackAPI(IOptions<SlackAPIOptions> options)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (options.Value == null) throw new ArgumentNullException("options.Value");

            this.slackWebhookUrl = options.Value.SlackWebhookUrl;
            this.slackAppName = options.Value.SlackAppName;
        }

        public async Task<string> SlackIntegrationPostAsync(HttpClient httpClient, string data)
        {
            try
            {
                var httpClientSafe = new HttpClientFactory().CreateClient();
                HttpResponseMessage message = await httpClientSafe.PostAsync(this.slackWebhookUrl, GenerateContent(data));
                return await message.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Slack Integration Issue " + ex.StackTrace);
            }
            return "Failed";
        }

        private StringContent GenerateContent(string data)
        {
            dynamic content = new { text = data, mrkdwn = true, username = this.slackAppName };
            return new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
        }
    }
}
