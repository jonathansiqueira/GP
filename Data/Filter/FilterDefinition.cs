using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class FilterDefinition
    {
        private readonly string filterName;
        private readonly FilterCategory filterCategory;
        private readonly LookupAPICategory lookupAPICategory;
        private readonly int maxInBody;

        public FilterDefinition(string filterName, FilterCategory filterCategory, int maxInBody)
            : this(filterName, filterCategory, maxInBody, LookupAPICategory.Unknown)
        {
        }

        public FilterDefinition(string filterName, FilterCategory filterCategory, int maxInBody, LookupAPICategory lookupAPICategory)
        {
            this.filterName = filterName ?? throw new ArgumentNullException("filterName");
            this.filterCategory = filterCategory;
            this.lookupAPICategory = lookupAPICategory;
            this.maxInBody = maxInBody;
        }

        public string FilterName
        {
            get
            {
                return this.filterName;
            }
        }

        public FilterCategory FilterCategory
        {
            get
            {
                return this.filterCategory;
            }
        }

        public LookupAPICategory LookupAPICategory
        {
            get
            {
                return this.lookupAPICategory;
            }
        }

        public int MaxInBody

        {
            get
            {
                return this.maxInBody;
            }
        }

        public virtual FilterBase CreateFilter(List<string> values, List<IFilter> postAPIFilters, UDRList udrList)
        {
            switch (this.FilterCategory)
            {
                case FilterCategory.APIBody:
                    return new APIBodyFilter(this, values);
                case FilterCategory.APIQueryString:
                    return new QueryStringFilter(this, values);
                case FilterCategory.APIReturnValue:
                    return new ReturnValueFilter(this, values);
                case FilterCategory.LookupFromAPIReturnValue:
                    return new APILookupFilter(this, values, udrList);
                default:
                    throw new ArgumentOutOfRangeException($"cannot construct a filter of category: {this.FilterCategory.ToString()}.");
            }
        }

    }
}
