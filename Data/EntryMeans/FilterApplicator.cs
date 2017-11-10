using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    public class FilterApplicator
    {
        private readonly IFilter[] postAPIFilters;
        private readonly IFilter[] apiFilters;

        public FilterApplicator(List<string> filters, UDRList udrList)
        {
            FilterBase filterBase;
            List<IFilter> postAPIFilters = new List<IFilter>();
            List<IFilter> preAPIFilters = new List<IFilter>();
            foreach (string filterString in filters)
            {
                filterBase = FilterBase.GetFilterDefinition(filterString, postAPIFilters, udrList);
                if (filterBase is APIBodyFilter || filterBase is QueryStringFilter)
                {
                    preAPIFilters.Add(filterBase);
                }
                else
                {
                    postAPIFilters.Add(filterBase);
                }
            }
            this.apiFilters = preAPIFilters.ToArray();
            this.postAPIFilters = postAPIFilters.ToArray();
        }

        public IFilter[] ApiFilters
        {
            get
            {
                return apiFilters;
            }
        }

        public bool IsApplicable(Dictionary<string, dynamic> row)
        {
            foreach(IFilter filter in postAPIFilters)
            {
                if (filter.ShouldFilter(row))
                    return false;
            }
            return true;
        }
    }
}
