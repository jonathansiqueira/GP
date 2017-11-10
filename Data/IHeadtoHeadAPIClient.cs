using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace H2HGermPlasmProcessor.Data
{
    public interface IHeadtoHeadAPIClient
    {
        void Initialize(HttpClient httpClient, string bearerToken, string userId);

        Task<string> ReportFailure(
           ILambdaContext context,
           HttpClient httpClient,          
           int reportId,
           string statusMessage
          );

        Task<string> ReportSuccess(
         ILambdaContext context,
         HttpClient httpClient,        
         int reportId       
        );
    }
}
