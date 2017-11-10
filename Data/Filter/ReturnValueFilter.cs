using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class ReturnValueFilter : FilterBase
    {
        private dynamic value;

        public ReturnValueFilter(FilterDefinition definition, List<string> values) 
            : base(definition, values)
        {
        }
        public override int NumberOfContents(int maxNumberSoFar)
        {
            return maxNumberSoFar;
        }

        public override void ApplyToAPICall(List<string> queryParts, List<List<string>> bodyParts, List<string> harvestYears)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldFilter(Dictionary<string, dynamic> row)
        {
            if (row.TryGetValue(base.ParameterName, out value))
            {
                if (!base.Values.Contains(value.ToString()))
                    return true;
            }
            return false;
        }
    }
}
