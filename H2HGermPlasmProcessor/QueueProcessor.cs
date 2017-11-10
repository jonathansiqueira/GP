using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data;
using H2HGermPlasmProcessor.Data.Dto;
using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.ReportData;
using System.Linq;
using H2HGermPlasmProcessor.Data.UDR;
using Newtonsoft.Json;

namespace H2HGermPlasmProcessor
{
    public class QueueProcessor
    {
        private readonly IQueue queue;
        private readonly INotifier notifier;
        private readonly IProductAnalyticsAPIClient productAnalyticsAPIClient;
        private readonly ICache persister;
        private readonly IUDRData udrData;
        private readonly HttpClient httpClient;
        private readonly string bearerToken;
        private FilterApplicator filter = null;
        private UDRList udrList = null;
        private readonly ISlackAPI slack;
        private readonly IHeadtoHeadAPIClient headToHeadAPIClient;
       

        public QueueProcessor(
            IQueue queue,
            INotifier notifier,
            IProductAnalyticsAPIClient productAnalyticsAPIClient,
            ICache persister,
            IUDRData udrData,
            HttpClient httpClient,
            string bearerToken,
            ISlackAPI slack,
            IHeadtoHeadAPIClient headToHeadAPIClient
            )
        {
            this.queue = queue ?? throw new ArgumentNullException("queue");
            this.notifier = notifier ?? throw new ArgumentNullException("notifier"); ;
            this.productAnalyticsAPIClient = productAnalyticsAPIClient ?? throw new ArgumentNullException("productAnalyticsAPIClient");
            this.persister = persister ?? throw new ArgumentNullException("persister");
            this.udrData = udrData ?? throw new ArgumentNullException("udrData");
            this.bearerToken = bearerToken ?? throw new ArgumentNullException("bearerToken");
            this.httpClient = httpClient ?? throw new ArgumentNullException("httpClient");
            this.bearerToken = bearerToken;
            this.slack = slack ?? throw new ArgumentNullException("slack");
            this.headToHeadAPIClient= headToHeadAPIClient ?? throw new ArgumentNullException("headToHeadAPIClient");
        }

        public void Process(ILambdaContext context, GermPlasmSNSRequest request, string topicArn)
        {
            string keyReportName = request.ReportName.Replace(' ', '`');
            int reportId = request.ReportId;
            ulong count = this.persister.InitializeCounter(keyReportName, request.MyCount);
            if (count == (ulong)request.MyCount)
            {
                //Removed the CacheClean Up we can put it back
                //this.persister.CleanupCache(context, keyReportName, reportId);
                count = this.persister.InitializeCounter(keyReportName, request.MyCount);
            }

            string queueUrl = request.GPQueueUrl;
            context.Logger.LogLine($"starting to process against SQS: {queueUrl} and count: {count}");

            // give lambda 20 secods to create a new notification to kick off a new lambda processor
            TimeSpan limit = new TimeSpan(0, 0, 20);
            bool remaining = true;
            // initialize Analytics API
            bool initialized = false;
            queue.Initialize(request);
            QueueGermPlasmEvent gpEvent;
            while (remaining && context.RemainingTime > limit)
            {
                context.Logger.LogLine($"getting from SQS: {queueUrl} with {context.RemainingTime} remaining");
                try
                {
                    // Load Q data
                    gpEvent = this.queue.GetNext(context);
                    if (gpEvent == null)
                        remaining = false;
                    else
                    {
                        if (!initialized)
                        {
                           
                            this.persister.InitializeObservations(keyReportName, gpEvent.Observations);
                            productAnalyticsAPIClient.Initialize(this.httpClient, bearerToken, gpEvent.UserId);
                            this.headToHeadAPIClient.Initialize(this.httpClient, bearerToken, gpEvent.UserId);
                            List<string> udrNames = gpEvent.Bands.Where(b => b.BandingGroup == "UDR").Select(b => b.BandName).ToList();
                            udrNames.AddRange(gpEvent.DataFilters.Where(df => df.StartsWith("UserDefinedRegions")).SelectMany(df => df.Split('=')[1].Split('&')));
                            this.udrList = new UDRList(this.udrData.GetUDRsForCrop(this.httpClient, gpEvent.Crop, udrNames).Result);
                            initialized = true;
                        }
                        remaining = ProcessGermPlasmEvent(context, request, gpEvent);
                    }
                }
                catch (AggregateException exc)
                {
                    string error;
                    foreach (Exception iExc in exc.InnerExceptions)
                    {
                        error = $"ERROR: queue: {queueUrl}: error: {iExc.ToString()}";
                        string slackResponse = slack.SlackIntegrationPostAsync(this.httpClient, error).Result;
                        headToHeadAPIClient.ReportFailure(context, this.httpClient, reportId, "AggregateException = " + error);
                        context.Logger.Log(error);
                        context.Logger.Log(slackResponse);
                    }
                    
                    remaining = false;
                }
                catch (Exception exc)
                {
                    string error = $"ERROR: queue: {queueUrl}: error: {exc.ToString()}";
                    context.Logger.Log(error);
                    string slackResponse = slack.SlackIntegrationPostAsync(this.httpClient, error).Result;
                    headToHeadAPIClient.ReportFailure(context, this.httpClient, reportId,"Exception= " +error);
                    context.Logger.Log(slackResponse);
                    remaining = false;
                }
            }
            // need to queue a new processor to the SNS
            if (remaining)
            {
                // use notifier to send to first entrypoint topic
                notifier.SendSelfProcessStartAsync(context, request, topicArn).Wait();
            }
        }

        private bool ProcessGermPlasmEvent(ILambdaContext context, GermPlasmSNSRequest request, QueueGermPlasmEvent gpEvent)
        {
            long germPlasmId = gpEvent.Germplasm.GermplasmId;           
            context.Logger.LogLine($"processing data keys to cache for germ Plasm Id: {germPlasmId}; key:{gpEvent.Germplasm.GetProduct()}; isMetric: {request.IsMetric}");
            if (filter == null)
            {
                filter = new FilterApplicator(gpEvent.DataFilters, this.udrList);
            }
            string cacheReportName = gpEvent.ReportName.Replace(' ', '`');
            int reportId = gpEvent.ReportId;
            TimeSpan ts = context.RemainingTime.Subtract(TimeSpan.FromSeconds(20));
            if (ts.Ticks <= 0)
            {
                context.Logger.LogLine($"CreateMessageAsync for germ Plasm Id: {germPlasmId}");
                queue.CreateMessageAsync(gpEvent);
                return true;
            }

            Dictionary<string, BaseBand> bands = new Dictionary<string, BaseBand>();
            foreach(BandDefinition bandDefintion in gpEvent.Bands)
            {
                BandFactory.Create(bands, context, this.httpClient, bandDefintion, this.productAnalyticsAPIClient, this.udrList);
            }

            CancellationTokenSource cancellationToken = new CancellationTokenSource(ts);

            if (!this.persister.GermPlasmProcessed(cacheReportName, reportId, germPlasmId))
            {
                try
                {
                    context.Logger.Log($"processing the germplasm {germPlasmId}");

                    List<string> observations = gpEvent.Observations.Select(o => o.ObsRefCd).ToList();
                    // load data, supplying API supported filters
                    List<Dictionary<string, dynamic>> data =
                        this.productAnalyticsAPIClient.GetEntryMeansAsynch(
                            context,
                            this.httpClient,
                            cancellationToken.Token,
                            gpEvent.UserId,
                            germPlasmId,
                            filter.ApiFilters,
                            observations,
                            gpEvent.Crop,
                            gpEvent.RegionName
                            ).Result;
                    // S<IP for now: then load any missing banding/observational data
                    // output to summarize valid records
                    List<ReducedEntryMeans> reportOutputs = ReportGrouperFactory.GetStandardReducedEntryMeans(
                        bands, 
                        gpEvent.AnalysisType,
                        observations);
                   
                    object cEvent;
                    foreach (Dictionary<string, dynamic> row in data)
                    {
                        if (gpEvent.Germplasm.CEvent == null && row.TryGetValue("cEvent", out cEvent) && cEvent != null && !string.IsNullOrEmpty(cEvent.ToString()))
                        {
                            gpEvent.Germplasm.CEvent = cEvent.ToString().Replace(',', '|');
                        }
                        if (filter.IsApplicable(row)) // first, see if the row should be filtered
                        {
                            context.Logger.Log($"first, see if the row should be filtered {JsonConvert.SerializeObject(row)}");
                            foreach (ReducedEntryMeans output in reportOutputs)
                            {
                                output.ProcessRecord(cancellationToken.Token, request.IsMetric, row); // apply bands and gather observations
                            }
                        }
                    }
                    context.Logger.LogLine($"persisting data to cache for germ Plasm Id: {germPlasmId}, cEvent: {gpEvent.Germplasm.CEvent}");
                    persister.PersistGermPlasmOutputAsync(context, cacheReportName,reportId, gpEvent.Germplasm, gpEvent.Category, reportOutputs).Wait();
                }
                catch(AggregateException exc)
                {

                    context.Logger.LogLine($"ProcessGermPlasmEvent : {exc.StackTrace}-ExceptionMessage:{exc.Message}-reportName:{cacheReportName}- germplasmId: {germPlasmId}");
                   
                    //headToHeadAPIClient.ReportFailure(context, this.httpClient, request.ReportId, "AggregateException= " +exc.Message);
                    queue.CreateMessageAsync(gpEvent);
                    bool cancelled = cancellationToken.IsCancellationRequested || exc.InnerExceptions.Select(e => e is TaskCanceledException).Count() > 0;
                    if (!cancelled)
                        throw;
                    context.Logger.LogLine($"cancelled: {cancellationToken.IsCancellationRequested}");
                    return true;
                }
        }
            else
            {
                context.Logger.Log($"skipping processing for report: {cacheReportName} and germ plasm id: {germPlasmId}");
            }

            if (AreWeFinished(context, cacheReportName, gpEvent.UserId, germPlasmId)) //when we are finished
            {
                context.Logger.Log($"starting up band generation");
                //put single SNS message to initiiate next process.
                this.notifier.SendBandProcessStartAsync(new BandNotificationMessage(                   
                    userId: request.UserId,
                    reportName: request.ReportName,
                    reportId:request.ReportId,
                    crop: gpEvent.Crop,
                    region: gpEvent.Region,
                    year: gpEvent.Year,                  
                    compareHeads: request.CompareHeads,
                    reportIdentifier:request.ReportIdentifier))
                    .Wait();

                this.queue.DeleteGPQueueAsync().Wait();
                context.Logger.Log($"Delete Queue DeleteGPQueueAsync End");
                return false;
            }
            return true;
        }

        private bool AreWeFinished(ILambdaContext context, string cacheReportName, string userId, long germPlasmId)
        {
            ulong remaining = this.persister.DecrementMyCounter(cacheReportName, context);
            context.Logger.Log($"remaining items to process: {remaining}");
            if (remaining == 0)
            {
                return true;
            }
            return false;
        }
    }
}
