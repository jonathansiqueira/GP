using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class InputFilterParameter
    {
        private readonly string inputParameterName;
        private readonly List<string> inputParameterValues;

        public InputFilterParameter(string inputParameterName, List<string> inputParameterValues)
        {
            this.inputParameterName = inputParameterName ?? throw new ArgumentNullException("inputParameterName");
            this.inputParameterValues = inputParameterValues ?? throw new ArgumentNullException("inputParameterValues");
            if (inputParameterValues.Count == 0) throw new ArgumentOutOfRangeException("inputParameterValues", "must have one or more filter values");
        }

        public string InputParameterName
        {
            get
            {
                return inputParameterName;
            }
        }

        public List<string> InputParameterValues
        {
            get
            {
                return inputParameterValues;
            }
        }
    }
}
