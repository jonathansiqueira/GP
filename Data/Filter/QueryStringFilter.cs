using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class QueryStringFilter : FilterBase
    {
        public QueryStringFilter(FilterDefinition definition, List<string> values) 
            : base(definition, values)
        {
        }

        public override int NumberOfContents(int maxNumberSoFar)
        {
            return maxNumberSoFar;
        }

        public override void ApplyToAPICall(List<string> queryParts, List<List<string>> bodyParts, List<string> harvestYears)
        {
            queryParts.Add($"{this.ParameterName}={string.Join("&", this.Values)}");
        }

        public override bool ShouldFilter(Dictionary<string, dynamic> row)
        {
            throw new NotImplementedException();
        }
    }
}
