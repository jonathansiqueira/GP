using System;
using System.Collections.Generic;
using System.Linq;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class APIBodyFilter : FilterBase
    {
        public APIBodyFilter(FilterDefinition definition, List<string> values)
            : base(definition, values)
        {
        }

        public override int NumberOfContents(int maxNumberSoFar)
        {
            if (base.Definition.MaxInBody < this.Values.Count)
            {
                int parts = this.Values.Count / base.Definition.MaxInBody;
                if (parts > maxNumberSoFar)
                    return parts;
            }
            return maxNumberSoFar;
        }

        public override void ApplyToAPICall(List<string> queryParts, List<List<string>> bodyParts, List<string> harvestYears)
        {

            if (bodyParts.Count > 1)
            {
                List<List<string>> newParts = new List<List<string>>(bodyParts.Count);
                for (int i = 0; i < bodyParts.Count; i++)
                {
                    newParts.Add(new List<string>());
                }
                int parts = bodyParts.Count;
                int index = 0;
                foreach (string value in this.Values)
                {
                    newParts[index++ % parts].Add(value);
                }
                for (int i = 0; i < bodyParts.Count;i++)
                {
                    if (this.Definition.FilterName == "TestSetSeasons")
                    {
                        ParseTestSetSeason(newParts[i], harvestYears);
                        bodyParts[i].Add($"\"{this.ParameterName}\": [\"{string.Join("\",\"", newParts[i])}\"]");
                    }
                    else
                    bodyParts[i].Add($"\"{this.ParameterName}\": [\"{string.Join("\",\"", this.Values)}\"]");
                }
            }
            else
            {
                if (this.Definition.FilterName == "TestSetSeasons")
                {
                    ParseTestSetSeason(this.Values, harvestYears);
                }
                bodyParts[0].Add($"\"{this.ParameterName}\": [\"{string.Join("\",\"", this.Values)}\"]");
            }
        }

        private void ParseTestSetSeason(IEnumerable<string> testSetSeasons, List<string> harvestYears)
        {
            int year, month;
            foreach(string testSetSeasion in testSetSeasons)
            {
                string[] parts = testSetSeasion.Split(':');
                if (parts.Length == 2)
                {
                    if (int.TryParse(parts[0], out year) && int.TryParse(parts[1], out month))
                    {
                        if (month > 9 && month <= 12)
                        {
                            harvestYears.Add((year + 1).ToString());
                        }
                        else
                        {
                            harvestYears.Add((year).ToString());
                        }
                    }
                }
            }
        }


        public override bool ShouldFilter(Dictionary<string, dynamic> row)
        {
            throw new NotImplementedException();
        }
    }
}
