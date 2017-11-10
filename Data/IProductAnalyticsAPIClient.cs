using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface IProductAnalyticsAPIClient
    {
        void Initialize(HttpClient httpClient, string bearerToken, string userId);

        Task<List<Dictionary<string, dynamic>>> GetEntryMeansAsynch(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            string userId,
            long germPlasmId,
            IFilter[] filters,
            List<string> observations,
            string crop,
            string regionName
            );

        Task<List<FieldObservation>> GetFieldObservationsAsync(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            long fieldId,
            IEnumerable<string> fieldObservations);

        Task<Dictionary<long, FieldStress>> GetFieldStressesAsync(
            ILambdaContext context,
            HttpClient httpClient,
            CancellationToken cancellationToken,
            IEnumerable<string> growingSeasons);
    }
}
