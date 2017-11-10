using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using H2HGermPlasmProcessor.Data.Model;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class UDRAPIClient : IUDRData
    {
        private readonly string udrByCropURL;

        public UDRAPIClient()
        {
            this.udrByCropURL = Environment.GetEnvironmentVariable("UDRByCropURL");
        }

        public async Task<IEnumerable<UDRGeography>> GetUDRsForCrop(HttpClient httpClient, string crop, IEnumerable<string> udrNames)
        {
            StringContent requestContent = new StringContent($"[\"{string.Join("\",\"", udrNames)}\"]", Encoding.UTF8, "application/json");
            string url = this.udrByCropURL.Replace("{crop}", crop);
            HttpResponseMessage message = await httpClient.PostAsync(url, requestContent);

            if (message.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"request for UDR data on crop: {crop} failed with response: {message.StatusCode}:{message.ReasonPhrase}");
            }
            string content = await message.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<UDRGeography>>(
                content
            );
        }
    }
}
