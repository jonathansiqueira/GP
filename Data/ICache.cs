using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using H2HGermPlasmProcessor.Data.ReportData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface ICache
    {
        void InitializeObservations(string reportName, List<Observation> observations);

        ulong InitializeCounter(string reportName, ulong initialValue);

        ulong DecrementMyCounter(string reportName, ILambdaContext context);

        bool GermPlasmProcessed(string reportName, int reportId, long germPlasmId);

        Task PersistGermPlasmOutputAsync(
            ILambdaContext context, 
            string reportName,
            int reportId,
            Germplasm germPlasm, 
            string headOrOther,
            List<ReducedEntryMeans> reducedEntryMeansList);

        void CleanupCache(ILambdaContext context, string cacheReportName, int reportId);
    }
}
