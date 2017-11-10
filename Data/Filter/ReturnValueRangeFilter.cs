using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class ReturnValueRangeFilter<T> : FilterBase
        where T: struct
    {
        private T? beginValue;
        private T? endValue;
        CoerceDataFilterDefinition<T> definition;
        private dynamic value;

        public ReturnValueRangeFilter(CoerceDataFilterDefinition<T> definition, T value) 
            : base(definition)
        {
            this.definition = definition;
            InitializeWithDefinition(definition, value);
        }

        public void InitializeWithDefinition(FilterDefinition definition, T value)
        {
            if (definition.FilterCategory == FilterCategory.APIReturnValueRangeBegin)
            {
                beginValue = value;
            }
            else if (definition.FilterCategory == FilterCategory.APIReturnValueRangeEnd)
            {
                endValue = value;
            }
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
                T coercedValue = this.definition.CoerceValue(value);
                if (!definition.InRange(coercedValue, beginValue, endValue))
                    return true;
            }
            return false;
        }
    }
}
