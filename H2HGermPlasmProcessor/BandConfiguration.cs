using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor
{
    public class BandConfiguration
    {
        private readonly string bandName;
        private readonly List<string> sourceColumns;
        private readonly string lookupServiceName;
        private readonly List<string> returnColumns;

        [JsonConstructor]
        public BandConfiguration(string bandName, List<string> sourceColumns, string lookupServiceName, List<string> returnColumns)
        {
            this.bandName = this.bandName ?? throw new ArgumentNullException("bandName");
            this.sourceColumns = this.sourceColumns ?? throw new ArgumentNullException("sourceColumns");
            this.lookupServiceName = this.lookupServiceName ?? throw new ArgumentNullException("lookupServiceName");
            this.returnColumns = this.returnColumns ?? throw new ArgumentNullException("returnColumns");
        }

        public string BandName
        {
            get
            {
                return this.bandName;
            }
        }

        public List<string> SourceColumns
        {
            get
            {
                return this.sourceColumns;
            }
        }

        public string LookupServiceName
        {
            get
            {
                return this.lookupServiceName;
            }
        }

        public List<string> ReturnColumns
        {
            get
            {
                return this.returnColumns;
            }
        }



    }
}
