using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class APILookupFilter : FilterBase
    {
        private readonly UDRList udrList;

        public APILookupFilter(FilterDefinition definition, List<string> values, UDRList udrList) 
            : base(definition, values)
        {
            this.udrList = udrList;
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
            if (udrList.ContainsUDR(base.Values, row))
                return false;
            return true;
        }
    }
}
