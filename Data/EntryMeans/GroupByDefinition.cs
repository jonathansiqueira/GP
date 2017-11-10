using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    public class GroupByDefinition
    {
        private readonly string name;
        private readonly GroupByCategory groupByCategory;

        public GroupByDefinition(string name, GroupByCategory groupByCategory)
        {
            this.name = name ?? throw new ArgumentNullException("name");
            this.groupByCategory = groupByCategory;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        public GroupByCategory GroupByCategory
        {
            get
            {
                return this.groupByCategory;
            }
        }
    }
}
