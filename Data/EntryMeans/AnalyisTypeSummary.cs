using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    [Serializable]
    [JsonArray]
    public class AnalyisTypeSummary : Dictionary<GroupBySet, AnnualSummary>
    {
        [JsonConstructor]
        public AnalyisTypeSummary(Dictionary<GroupBySet, AnnualSummary> analysisTypeAnnualSummary)
            : base(analysisTypeAnnualSummary ?? new Dictionary<GroupBySet, AnnualSummary>())
        {
        }
    }
}
