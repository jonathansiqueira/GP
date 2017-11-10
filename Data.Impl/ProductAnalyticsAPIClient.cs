using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class ProductAnalyticsAPIClient : IProductAnalyticsAPIClient
    {
        private readonly string entryMeansSearch;
        private readonly string fieldObservationsURL;
        private readonly string fieldStressURL;
        
        public ProductAnalyticsAPIClient()
        {
            this.entryMeansSearch = Environment.GetEnvironmentVariable("EntryMeansURL");
            this.fieldObservationsURL = Environment.GetEnvironmentVariable("FieldObservationsURL");
            this.fieldStressURL = Environment.GetEnvironmentVariable("FieldStressURL");
        }

        public void Initialize(HttpClient httpClient, string bearerToken, string userId)
        {
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", bearerToken);
            httpClient.DefaultRequestHeaders.Add("user-id", userId);
        }

        private static object sema = new object();

        public async Task<List<Dictionary<string, dynamic>>> GetEntryMeansAsynch(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            string userId,
            long germPlasmId,
            IFilter[] filters,
            List<string> observations,
            string crop,
            string regionName
            )
        {
            string url;
            List<string> harvestYears;
            List<HttpContent> httpContentList = BuildEntryMeansQuery(context, germPlasmId, filters, observations, crop,regionName, out url, out harvestYears);

            List<Dictionary<string, dynamic>> output = new List<Dictionary<string, dynamic>>();
            int i = 0;
            foreach (HttpContent httpContent in httpContentList)
            {
                HttpResponseMessage message = httpClient.PostAsync($"{url}&harvestYear={harvestYears[i]}", httpContent, cancellationToken).Result;

                if (message.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    context.Logger.Log($"request for data on germ plasm: {germPlasmId} failed with response: {message.StatusCode}:{message.ReasonPhrase}");
                    throw new Exception($"request for data on germ plasm: {germPlasmId} failed with response: {message.StatusCode}:{message.ReasonPhrase}");
                }
                string content = await message.Content.ReadAsStringAsync();
                context.Logger.Log($"received for {germPlasmId} using url: {url} and content length: {content.Length}");

                output.AddRange(JsonConvert.DeserializeObject<List<Dictionary<string, dynamic>>>(
                    content
                ));
                i++;
            }
            return output;
        }

        private List<HttpContent> BuildEntryMeansQuery(
            ILambdaContext context, 
            long germPlasmId, 
            IFilter[] filters, 
            List<string> observations, 
            string crop,
            string regionName,
            out string url,
            out List<string> harvestYears)
        {
            List<string> queryParts = new List<string>();
            List<List<string>> bodyParts = new List<List<string>>();
            harvestYears = new List<string>();

            int maxNumberSoFar = 1;
            foreach (IFilter filter in filters)
            {
                maxNumberSoFar = filter.NumberOfContents(maxNumberSoFar);
            }
            for (int i = 0; i < maxNumberSoFar; i++)
            {
                bodyParts.Add(new List<string>());
                bodyParts[i].Add($"\"ObsRefCode\":[\"{string.Join("\",\"", observations)}\"]");
            }
            queryParts.Add($"germplasmId={germPlasmId}&cropName={crop}{(!string.IsNullOrEmpty(regionName)?"&regionName=" + regionName:string.Empty)}");
            foreach (IFilter filter in filters)
            {
                filter.ApplyToAPICall(queryParts, bodyParts, harvestYears);
            }

            url = $"{this.entryMeansSearch}?{string.Join("&", queryParts)}";
            List<HttpContent> contentList = new List<HttpContent>(bodyParts.Count);
            context.Logger.Log($"getting data for {germPlasmId} using url: {url} and count: {bodyParts.Count}");
            for (int i = 0; i < bodyParts.Count; i++)
            {
                string jsonContent = $"{{ {string.Join(",", bodyParts[i])} }}";
                context.Logger.Log($"getting data for {germPlasmId} using url: {url}&harvestYear={harvestYears[i]} and content: {jsonContent}");
                contentList.Add(new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            }

            return contentList;
        }

        public async Task<List<FieldObservation>> GetFieldObservationsAsync(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            long fieldId,
            IEnumerable<string> fieldObservations)
        {
            StringContent requestContent = new StringContent($"{{\"ObsRefCodes\": [\"{GetFieldObservations(fieldObservations)}\"] }}", Encoding.UTF8, "application/json");
            string url = this.fieldObservationsURL.Replace("{fieldId}", fieldId.ToString());
            HttpResponseMessage message = await httpClient.PostAsync(url, requestContent, cancellationToken);

            if (message.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"request for field observation data on fieldId: {fieldId} failed with response: {message.StatusCode}:{message.ReasonPhrase}");
            }
            string content = await message.Content.ReadAsStringAsync();
            context.Logger.Log($"received for {fieldId} using url: {url} and content length: {content.Length}");

            return JsonConvert.DeserializeObject<List<FieldObservation>>(
                content
            );
        }

        public async Task<Dictionary<long, FieldStress>> GetFieldStressesAsync(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            IEnumerable<string> growingSeasons)
        {
            StringContent requestContent = new StringContent($"[\"{string.Join("\",\"", growingSeasons)}\"]", Encoding.UTF8, "application/json");
            HttpResponseMessage message = await httpClient.PostAsync(this.fieldStressURL, requestContent, cancellationToken);

            if (message.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"request for field stresses data on fieldId: {string.Join(",", growingSeasons)} failed on url: {this.fieldStressURL} with response: {message.StatusCode}:{message.ReasonPhrase}");
            }
            string content = await message.Content.ReadAsStringAsync();
            context.Logger.Log($"received for {string.Join(",", growingSeasons)} using url: {this.fieldStressURL} and content length: {content.Length}");

            return JsonConvert.DeserializeObject<Dictionary<long, FieldStress>>(
                content
            );
        }

        private string GetFieldObservations(IEnumerable<string> fieldObservations)
        {
            return string.Join("\",\"", fieldObservations);
        }
    }
}
