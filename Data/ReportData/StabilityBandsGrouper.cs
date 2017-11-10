using System;
using System.Collections.Generic;
using System.Text;
using H2HGermPlasmProcessor.Data.Bands;
using H2HGermPlasmProcessor.Data.EntryMeans;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.ReportData
{
    public class StabilityBandsGrouper : ReducedEntryMeans
    {
        public StabilityBandsGrouper(Dictionary<string, BaseBand> bands, string analysisType, List<string> observations) 
            : base(ReportGrouping.Stability, bands, analysisType, observations)
        {
            if (!base.Bands.ContainsKey("harvestYear"))
                base.Bands.Add("YR", new ColumnBand("harvestYear"));
            if (!base.Bands.ContainsKey("testSetId"))
                base.Bands.Add("TEST_SET_ID", new ColumnBand("testSetId"));
            if (!base.Bands.ContainsKey("FIELD_ID") && !base.Bands.ContainsKey("fieldId"))
                base.Bands.Add("FIELD_ID", new ColumnBand("fieldId"));
            if (!base.Bands.ContainsKey("LOCATTION_ID") && !base.Bands.ContainsKey("locationId"))
                base.Bands.Add("LOCATION_ID", new ColumnBand("locationId"));
            if (!base.Bands.ContainsKey("LATITUDE") && !base.Bands.ContainsKey("fieldLatitude"))
                base.Bands.Add("LATITUDE", new ColumnBand("fieldLatitude"));
            if (!base.Bands.ContainsKey("LONGITUDE") && !base.Bands.ContainsKey("fieldLongitude"))
                base.Bands.Add("LONGITUDE", new ColumnBand("fieldLongitude"));
        }

        protected override List<AnalyisTypeSummary> GetRowSummaries(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            List<AnalyisTypeSummary> summaries = new List<AnalyisTypeSummary>();
            AnalyisTypeSummary summary;
            List<GroupBySet> bandedKeys = new List<GroupBySet>();
            bandedKeys.Add(new GroupBySet());
            foreach (KeyValuePair<string, BaseBand> field in base.Bands)
            {
                field.Value.AddBandToSet(cancellationToken, bandedKeys, field.Key, row);
            }
            foreach (GroupBySet bandedKey in bandedKeys)
            {
                if (!base.Summary.TryGetValue(bandedKey, out summary))
                {
                    summary = new AnalyisTypeSummary(null);
                    base.Summary.Add(bandedKey, summary);
                }
                summaries.Add(summary);
            }
            return summaries;
        }
    }
}
