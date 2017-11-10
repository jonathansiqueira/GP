using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    public class AnalysisTypeDefinition
    {
        private readonly string name;
        private readonly List<string> fields;

        public static readonly AnalysisTypeDefinition ByTest = new AnalysisTypeDefinition("By Test", new List<string>()
        {
            "fieldId",
            "testSetId"
        });

        public static readonly AnalysisTypeDefinition ByField = new AnalysisTypeDefinition("By Field", new List<string>()
        {
            "fieldId"
        });

        public static AnalysisTypeDefinition Get(string analysisType)
        {
            if (analysisType == "Test" || analysisType == "By Test")
                return ByTest;
            if (analysisType == "Field" || analysisType == "By Field")
                return ByField;

            throw new ArgumentOutOfRangeException("Unkown analysis type: currently support 'Test', 'By Test', 'By Field', 'Field'.");
        }

        public AnalysisTypeDefinition(string name, List<string> fields)
        {
            this.name = name ?? throw new ArgumentNullException("name");
            this.fields = fields ?? throw new ArgumentNullException("fields");
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public List<string> Fields
        {
            get
            {
                return this.fields;
            }
        }

    }
}
