using Amazon.Lambda.Core;
using Enyim.Caching;
using Enyim.Caching.Memcached;
using Enyim.Caching.Memcached.Results;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using H2HGermPlasmProcessor.Data.ReportData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class CachePersister : ICache
    {
        private readonly IMemcachedClient memcachedClient;
        private readonly TimeSpan expiration;

        public CachePersister(IMemcachedClient memcachedClient)
        {
            this.memcachedClient = memcachedClient ?? throw new ArgumentNullException("memcachedClient");
            memcachedClient.NodeFailed += MemcachedClient_NodeFailed;
            this.expiration = new TimeSpan(18, 0, 0);
        }

        ~CachePersister()
        {
            memcachedClient.NodeFailed -= MemcachedClient_NodeFailed;
            memcachedClient.Dispose();
        }

        private void MemcachedClient_NodeFailed(IMemcachedNode obj)
        {
            throw new Exception($"ElastiCache endpoint: {obj.EndPoint} failed: {obj}");
        }

        public bool GermPlasmProcessed(string reportName,int reportId, long germPlasmId)
        {
            return memcachedClient.Get($"{reportName}_{germPlasmId}") != null;
        }

        public void InitializeH2HCounters(string cacheReportName, ulong h2hMatchupCount)
        {
            memcachedClient.Increment($"{cacheReportName}_H2H_Count", h2hMatchupCount, 0, this.expiration);
        }

        private ulong IncrementCachedH2HCounter(string cacheReportName)
        {
            //H2H_Band_Count increase from 0 to 1
            return memcachedClient.Increment($"{cacheReportName}_H2H_Band_Count", 1, 1, this.expiration);
        }

        private async Task<ulong> SendAsync(ILambdaContext context, string cacheReportName, ReducedBandKey bandToMatch)
        {           
            string key = $"{cacheReportName}_H2H_Band{IncrementCachedH2HCounter(cacheReportName)}";
            context.Logger.Log($"{key}");
            bool stored = await memcachedClient.StoreAsync(StoreMode.Add, key, bandToMatch, this.expiration);
            TrackCacheKey(cacheReportName, key);
            return 1;
        }

        public async Task PersistGermPlasmOutputAsync(
            ILambdaContext context,
            string cacheReportName,
            int reportId,
            Germplasm germPlasm, 
            string headOrOther, 
            List<ReducedEntryMeans> reducedEntryMeansList)
        {
            bool stored = memcachedClient.Store(StoreMode.Set, $"{cacheReportName}_{germPlasm.GermplasmId}", JsonConvert.SerializeObject(germPlasm), this.expiration);
            context.Logger.Log($"PersistGermPlasmOutputAsync {cacheReportName}_{germPlasm.GermplasmId} and {JsonConvert.SerializeObject(germPlasm)} in ElastiCache");
            if (!stored)
            {
                context.Logger.Log($"failed to set value for {cacheReportName}_{germPlasm.GermplasmId} in ElastiCache");
                throw new Exception($"failed to set value for {cacheReportName}_{germPlasm.GermplasmId} in ElastiCache");
            }
            this.TrackCacheKey(cacheReportName, $"{cacheReportName}_{germPlasm.GermplasmId}");

            if (reducedEntryMeansList.Count == 0)
            {
                context.Logger.Log($"no data to save for: {germPlasm.GermplasmId}");
            }
            ReducedBandKey key;
            foreach (ReducedEntryMeans reducedEntryMeans in reducedEntryMeansList)
            {
                if (reducedEntryMeans.Summary.Count == 0)
                {
                    context.Logger.Log($"{reducedEntryMeans.ReportGrouping}: no data to save for: {germPlasm.GermplasmId}");
                }
                foreach (KeyValuePair<GroupBySet, AnalyisTypeSummary> pair in reducedEntryMeans.Summary)
                {
                    key = new ReducedBandKey(cacheReportName,reportId, reducedEntryMeans.ReportGrouping.ToString(), germPlasm.GetProduct(), headOrOther, pair.Key, germPlasm.CEvent);
                    await SendAsync(context, cacheReportName, key);
                    string cacheKey = key.CacheKey;
                    if (string.IsNullOrEmpty(germPlasm.Alias))
                    {
                        context.Logger.Log($"trying to store: {cacheKey} for key: {key.ToKeyString()}");
                        stored = memcachedClient.Store(StoreMode.Add, cacheKey, pair.Value, this.expiration);
                        if (!stored)
                        {
                            throw new Exception($"failed to set value in ElastiCache");
                        }
                        this.TrackCacheKey(cacheReportName, cacheKey);
                    }
                    else
                    {
                        StoreBandKeyAsArrayItem(context, cacheReportName,reportId, cacheKey, pair.Value);
                    }
                    //context.Logger.Log($"stored");
                }
            }
            return;
        }

        private void StoreBandKeyAsArrayItem(ILambdaContext context, string cacheReportName,int reportId, string cacheKey, AnalyisTypeSummary summary)
        {
            context.Logger.Log($"trying to increment: {cacheKey}_Count");
            ulong currentIndex = this.memcachedClient.Increment($"{cacheKey}_Count", 0, 1, this.expiration);
            if (currentIndex == 0)
            {
                TrackCacheKey(cacheReportName, $"{cacheKey}_Count");
            }
            context.Logger.Log($"trying to store: {cacheKey}{currentIndex}");
            bool stored = memcachedClient.Store(StoreMode.Set, $"{cacheKey}{currentIndex}", summary, this.expiration);
            if (!stored)
            {
                context.Logger.Log($"trying to store: StoreBandKeyAsArrayItem");
                throw new Exception("failed to set value in ElastiCache");
            }
            this.TrackCacheKey(cacheReportName, $"{cacheKey}{currentIndex}");
        }

        public void InitializeObservations(string reportName, List<Observation> observations)
        {
            // only one process will actuall add it
            string key = $"{reportName}_Observations";
            if (memcachedClient.Store(StoreMode.Add, key, observations, this.expiration))
            {
                TrackCacheKey(reportName, key);
            }
        }

        private void TrackCacheKey(string cacheReportName, string key)
        {
            ulong index = memcachedClient.Increment($"{cacheReportName}_Cache_Count", 0, 1, this.expiration);
            memcachedClient.Store(StoreMode.Add, $"{cacheReportName}_Cache_{index}", key, this.expiration);
        }

        public ulong InitializeCounter(string reportName, ulong initialValue)
        {
            ulong value = memcachedClient.Increment($"{reportName}_GP_Count", initialValue, 0, this.expiration);
            return value;
        }

        public ulong DecrementMyCounter(string reportName, ILambdaContext context)
        {
            ulong index = memcachedClient.Decrement($"{reportName}_GP_Count", 0, 1, this.expiration);
            context.Logger.Log($"DecrementMyCounter {index}");
            if (index == 0)
            {
                context.Logger.Log($"DecrementMyCounter Removed index == 0");
                memcachedClient.Remove($"{reportName}_GP_Count");
            }
            return index;
        }

        public void CleanupCache(ILambdaContext context, string cacheReportName,int reportId)
        {
            TimeSpan limit = new TimeSpan(0, 0, 20);
            string key = $"{cacheReportName}_Cleanup";
            if (memcachedClient.Store(StoreMode.Add, key, true, new TimeSpan(0, 0, 20)))
            {
                if (context.RemainingTime <= limit)
                {
                    memcachedClient.Remove(key);
                    return;
                }
                context.Logger.LogLine($"resetting cache for {cacheReportName}-{reportId}");
                ulong index = 0;
                IGetOperationResult<string> keyToRemove;
                while ((index = memcachedClient.Decrement($"{cacheReportName}_Cache_Count", 0, 1)) != 0)
                {
                    keyToRemove = memcachedClient.GetAsync<string>($"{cacheReportName}_Cache_{index}").Result;
                    if (keyToRemove.HasValue)
                    {
                        if (memcachedClient.Remove(keyToRemove.Value))
                        {
                            context.Logger.LogLine($"{keyToRemove.Value} removed from cache");
                        }
                        memcachedClient.Remove($"{cacheReportName}_Cache_{index}");
                    }
                    else
                    {
                        context.Logger.LogLine($"{cacheReportName}_Cache_{index} not removed from cache");
                    }
                }
                memcachedClient.Remove($"{cacheReportName}_GP_Count");
                memcachedClient.Remove($"{cacheReportName}_Cache_Count");
                memcachedClient.Remove($"{cacheReportName}_H2H_Matchup_Count");
                //memcachedClient.Remove($"{cacheReportName}_H2H_Band_Count");
                //memcachedClient.Remove($"{cacheReportName}_H2H_Count");
                memcachedClient.Remove(key);
            }
            else
            {
                context.Logger.LogLine($"skipping resetting cache for {cacheReportName}-{reportId}");
            }
        }
    }
}
