using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class FieldStressBand : BaseBand
    {
        private const string geographyDimKeyField = "geographyDimKey";
        private const string testSetSeasonField = "testSetSeason";

        private readonly ILambdaContext context;
        private readonly HttpClient httpClient;
        private readonly IProductAnalyticsAPIClient client;
        private readonly string category;
        private readonly string group;
        private readonly string fieldName;
        private readonly bool hasIntervals;

        private readonly static Dictionary<string, Dictionary<long, FieldStress>> sesonalCachedStresses = new Dictionary<string, Dictionary<long, FieldStress>>();

        public FieldStressBand(ILambdaContext context, HttpClient httpClient, BandDefinition definition, IProductAnalyticsAPIClient client) : base($"{definition.BandingGroup} {definition.BandName}")
        {
            this.context = context ?? throw new ArgumentNullException("context");
            this.httpClient = httpClient ?? throw new ArgumentNullException("httpClient");
            this.client = client ?? throw new ArgumentNullException("client");
            category = definition.Category;
            group = definition.BandingGroup;
            fieldName = definition.BandName;
            hasIntervals = (definition.Interval != null && definition.MinValue != null && definition.MaxValue != null);
            if (hasIntervals)
            {
                InitializeIntervals(definition);
            }
        }

        private readonly List<ObservationBandItem<double>> items = new List<ObservationBandItem<double>>();
        private void InitializeIntervals(BandDefinition definition)
        {
            double localMin = ToDouble(definition.MinValue);
            double localMax = ToDouble(definition.MaxValue);
            double localInterval = ToDouble(definition.Interval.Value);
            for (double current = localMin; current < localMax; current += localInterval)
            {
                items.Add(new ObservationBandItem<double>(definition.BandName, current, current + localInterval));
            }
        }

        private double ToDouble(Object value)
        {
            if (value is double)
            {
                return (double)value;
            }
            else
            {
                return Convert.ToDouble(value);
            }
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            object value, value1;
            long geographyDimKey;
            string testSetSeason;
            if (row.TryGetValue(geographyDimKeyField, out value) && value is long && row.TryGetValue(testSetSeasonField, out value1) && value1 is string)
            {
                geographyDimKey = (long)value;
                testSetSeason = (string)value1;
                Dictionary<long, FieldStress> cachedStresses;
                if (!sesonalCachedStresses.TryGetValue(testSetSeason, out cachedStresses))
                {
                    cachedStresses = this.client.GetFieldStressesAsync(
                        context,
                        httpClient,
                        cancellationToken,
                        new string[] { testSetSeason }).Result;
                    sesonalCachedStresses.Add(testSetSeason, cachedStresses);
                }
                FieldStress stress;
                FieldStressCategory stressCategory;
                FieldStressGrowthStage stage;
                if (cachedStresses.TryGetValue(geographyDimKey, out stress))
                {
                    if (stress.Categories.TryGetValue(this.group, out stressCategory))
                    {
                        if (stressCategory.StagesDictionary.TryGetValue(fieldName, out stage))
                        {
                            if (!hasIntervals)
                                return stage.StressLevelText;
                            if (stage.StressLevelScore.HasValue)
                                return CalculateInterval(stage.StressLevelScore.Value);
                        }
                    }
                }
            }
            return "";
        }

        private string CalculateInterval(double value)
        {
            ObservationBandItem<double> item = items.Where(i => value >= i.LowValue && value < i.HighValue).FirstOrDefault();
            if (item != null)
            {
                return item.HeaderBandName;
            }
            return "";
        }
    }
}