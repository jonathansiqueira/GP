using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface ISlackAPI
    {
        Task<string> SlackIntegrationPostAsync(HttpClient httpClient, string data);
    }
}
