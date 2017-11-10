using H2HGermPlasmProcessor.Data.Bands;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.ReportData
{
    public static class ReportGrouperFactory
    {
        public static List<ReducedEntryMeans> GetStandardReducedEntryMeans(Dictionary<string, BaseBand> bands, string analysisType, List<string> observations)
        {
            List<ReducedEntryMeans> list = new List<ReducedEntryMeans>(3);
            list.Add(new BandlessBandsGrouper(bands, analysisType, observations));
            list.Add(new EnumeratedBandsGrouper(bands, analysisType, observations));
            list.Add(new NestedBandsGrouper(bands, analysisType, observations));
            list.Add(new StabilityBandsGrouper(bands, analysisType, observations));

            return list;
        }
    }
}
