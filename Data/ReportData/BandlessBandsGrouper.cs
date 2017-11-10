using System;
using System.Collections.Generic;
using System.Text;
using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.EntryMeans;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.ReportData
{
    public class BandlessBandsGrouper : ReducedEntryMeans
    {
        public BandlessBandsGrouper(Dictionary<string, BaseBand> bands, string analysisType, List<string> observations) 
            : base(ReportGrouping.Bandless, bands, analysisType, observations)
        {
        }

        protected override List<AnalyisTypeSummary> GetRowSummaries(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            AnalyisTypeSummary summary;
            GroupBySet bandedKey = new GroupBySet();
            if (!base.Summary.TryGetValue(bandedKey, out summary))
            {
                summary = new AnalyisTypeSummary(null);
                base.Summary.Add(bandedKey, summary);
            }
            return new List<AnalyisTypeSummary>() { summary };
        }
    }
}
