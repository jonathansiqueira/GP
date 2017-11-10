using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class FieldObservationBand : BaseBand
    {
        private const string fieldIdKey = "fieldId";

        private readonly ILambdaContext context;
        private readonly HttpClient httpClient;
        private readonly IProductAnalyticsAPIClient client;
        private readonly HashSet<string> fieldObservations;

        private readonly Dictionary<long, List<FieldObservation>> cachedObservations = new Dictionary<long, List<FieldObservation>>();

        public FieldObservationBand(ILambdaContext context, HttpClient httpClient, BandDefinition definition, IProductAnalyticsAPIClient client) : base($"{definition.BandingGroup}{definition.BandName}")
        {
            this.context = context ?? throw new ArgumentNullException("context");
            this.httpClient = httpClient ?? throw new ArgumentNullException("httpClient");
            this.client = client ?? throw new ArgumentNullException("client");
            this.fieldObservations = new HashSet<string>();
            this.fieldObservations.Add(definition.BandName);
        }

        public void AddBandDefinition(BandDefinition definition)
        {
            this.fieldObservations.Add(definition.BandName);
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            throw new NotImplementedException();
        }

        public override void AddBandToSet(CancellationToken cancellationToken, List<GroupBySet> sets, string key, Dictionary<string, dynamic> row)
        {
            object value;
            long fieldId;
            if (row.TryGetValue(fieldIdKey, out value) && value is long)
            {
                fieldId = (long)value;
                List<FieldObservation> fieldObservations;
                if (!this.cachedObservations.TryGetValue(fieldId, out fieldObservations))
                {
                    fieldObservations = this.client.GetFieldObservationsAsync(
                        this.context,
                        this.httpClient,
                        cancellationToken,
                        fieldId,
                        this.fieldObservations).Result;
                    this.cachedObservations.Add(fieldId, fieldObservations);
                }

                Dictionary<string, int> keyCounts = new Dictionary<string, int>();
                foreach (FieldObservation fieldObservation in fieldObservations)
                {
                    if (keyCounts.ContainsKey(fieldObservation.ObsRefCd))
                        keyCounts[fieldObservation.ObsRefCd] = keyCounts[fieldObservation.ObsRefCd] + 1;
                    else
                    {
                        keyCounts.Add(fieldObservation.ObsRefCd, 1);
                    }
                }

                foreach (FieldObservation fieldObservation in fieldObservations)
                {
                    if (this.fieldObservations.Contains(fieldObservation.ObsRefCd))
                    {
                        key = $"{fieldObservation.ObsRefCd}{fieldObservation.Repetition}]";
                    }
                    else
                    {
                        key = $"{fieldObservation.Name}{fieldObservation.Repetition}]";
                    }
                    foreach (GroupBySet set in sets)
                    {
                        set.Add(new GroupBy(key, fieldObservation.ToString(), true));
                    }
                }
            }
        }
    }
}