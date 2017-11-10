using System;
using System.Collections.Generic;
using System.Text;
using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.EntryMeans;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.ReportData
{
    public class EnumeratedBandsGrouper : ReducedEntryMeans
    {
        public EnumeratedBandsGrouper(Dictionary<string, BaseBand> bands, string analysisType, List<string> observations) 
            : base(ReportGrouping.EnumeratedBands, bands, analysisType, observations)
        {
        }

        protected override List<AnalyisTypeSummary> GetRowSummaries(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            List<AnalyisTypeSummary> summaries = new List<AnalyisTypeSummary>();
            AnalyisTypeSummary summary;
            if (base.Bands.Count == 0)
            {
                GroupBySet bandedKey = new GroupBySet();
                if (!base.Summary.TryGetValue(bandedKey, out summary))
                {
                    summary = new AnalyisTypeSummary(null);
                    base.Summary.Add(bandedKey, summary);
                }
                summaries.Add(summary);
                return summaries;
            }
            foreach (KeyValuePair<string, BaseBand> field in base.Bands)
            {
                List<GroupBySet> bandedKeys = new List<GroupBySet>();
                bandedKeys.Add(new GroupBySet());
                field.Value.AddBandToSet(cancellationToken, bandedKeys, field.Key, row);
                foreach (GroupBySet bandedKey in bandedKeys)
                {
                    if (!base.Summary.TryGetValue(bandedKey, out summary))
                    {
                        summary = new AnalyisTypeSummary(null);
                        base.Summary.Add(bandedKey, summary);
                    }
                    summaries.Add(summary);
                }
            }
            return summaries;
        }
    }
}
