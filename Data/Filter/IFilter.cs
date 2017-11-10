using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public interface IFilter
    {
        FilterCategory FilterCategory { get; }

        string ParameterName { get; }

        HashSet<string> Values { get; }

        int NumberOfContents(int maxNumberSoFar);

        void ApplyToAPICall(List<string> queryParts, List<List<string>> bodyParts, List<string> harvestYears);

        bool ShouldFilter(Dictionary<string, dynamic> row);
    }
}
