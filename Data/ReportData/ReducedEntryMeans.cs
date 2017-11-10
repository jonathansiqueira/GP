using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.EntryMeans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.ReportData
{
    public abstract class ReducedEntryMeans
    {
        private readonly Dictionary<string, BaseBand> bands;
        private readonly Dictionary<string, BaseBand> properties = new Dictionary<string, BaseBand>();
        private HashSet<string> analysisTypeFields = new HashSet<string>();
        private HashSet<string> observations;

        private readonly ReportGrouping reportGrouping;
        private readonly Dictionary<GroupBySet, AnalyisTypeSummary> summarizedValues =
            new Dictionary<GroupBySet, AnalyisTypeSummary>();

        public ReducedEntryMeans(ReportGrouping reportGrouping, Dictionary<string, BaseBand> bands, string analysisType, List<string> observations)
        {
            this.reportGrouping = reportGrouping;
            AnalysisTypeDefinition atType = AnalysisTypeDefinition.Get(analysisType);
            if (atType != null)
            {
                foreach (string field in atType.Fields)
                {
                    analysisTypeFields.Add(field);
                }
            }
            this.bands = new Dictionary<string, BaseBand>(bands);
            if (observations.Count == 0) throw new ArgumentOutOfRangeException("at least one observation needs to be supplied.");
            this.observations = new HashSet<string>(observations);
        }

        protected Dictionary<string, BaseBand> Bands { get { return bands; } }

        protected Dictionary<string, BaseBand> Properties { get { return properties; } }

        protected HashSet<string> AnalysisTypeFields { get { return analysisTypeFields; } }

        protected HashSet<string> Observations { get { return observations; } }

        public ReportGrouping ReportGrouping
        {
            get
            {
                return reportGrouping;
            }
        }

        public Dictionary<GroupBySet, AnalyisTypeSummary> Summary
        {
            get
            {
                return this.summarizedValues;
            }
        }

        protected abstract List<AnalyisTypeSummary> GetRowSummaries(CancellationToken cancellationToken, Dictionary<string, dynamic> row);

        public void ProcessRecord(CancellationToken cancellationToken, bool isMetric, Dictionary<string, dynamic> row)
        {
            List<AnalyisTypeSummary> rowSummaries = GetRowSummaries(cancellationToken, row);

            object value;
            try
            {
                GroupBySet analysisTypeKey = new GroupBySet();

                foreach (string field in analysisTypeFields)
                {
                    if (row.TryGetValue(field, out value))
                    {
                        if (value != null)
                        {
                            analysisTypeKey.Add(new GroupBy(field, value.ToString(), false));
                        }
                        else
                        {
                            analysisTypeKey.Add(new GroupBy(field, string.Empty, false));
                        }
                    }
                }
                string year;
                if (!row.TryGetValue(AnnualSummary.YearEntryMeanColumn, out value))
                {
                    return;
                }
                year = value.ToString();
                // support lists of summaries
                List<ObservationValueCollection> observationsList = new List<ObservationValueCollection>();
                ObservationValueCollection observations;
                AnnualSummary annualSummary;
                foreach (AnalyisTypeSummary summary in rowSummaries)
                {
                    if (!summary.TryGetValue(analysisTypeKey, out annualSummary))
                    {
                        annualSummary = new AnnualSummary(new Dictionary<string, ObservationValueCollection>());
                        summary.Add(analysisTypeKey, annualSummary);
                    }
                    if (!annualSummary.ObservationsByYear.TryGetValue(year, out observations))
                    {
                        observations = new ObservationValueCollection();
                        annualSummary.ObservationsByYear.Add(year, observations);
                    }
                    observationsList.Add(observations);
                }

                const string observationRefCd = "observationRefCd";
                const string entryMean = "entryMean";
                const string testMean = "testMean";
                const string metricToEnglishFactor = "metricToEnglishFactor";

                string observation;
                dynamic conversionFactor;
                dynamic testMeanValue;
                if (row.TryGetValue(observationRefCd, out value))
                {
                    observation = value.ToString();
                    if (row.TryGetValue(entryMean, out value) &&
                        row.TryGetValue(metricToEnglishFactor, out conversionFactor) &&
                        row.TryGetValue(testMean, out testMeanValue))
                    {
                        foreach (ObservationValueCollection observationsListItem in observationsList)
                        {
                            if (isMetric)
                            {
                                observationsListItem.AddObservation(observation, (double)value, (double)testMeanValue);
                            }
                            else
                            {
                                observationsListItem.AddObservation(observation, (double)value * (double)conversionFactor, (double)testMeanValue * (double)conversionFactor);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
