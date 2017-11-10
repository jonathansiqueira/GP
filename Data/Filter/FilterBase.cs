using H2HGermPlasmProcessor.Data.Filter;
using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Linq;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public abstract class FilterBase : IFilter
    {
        private static DateTime CoerceDateTimeValueFromAPI(object value)
        {
            if (value is DateTime)
                return (DateTime)value;
            return DateTime.Parse(value.ToString());
        }

        private static Int32 CoerceInt32ValueFromAPI(object value)
        {
            if (value is Int32)
                return (Int32)value;
            return Int32.Parse(value.ToString());
        }

        private static bool DateTimeInRange(DateTime value, DateTime? lower, DateTime? upper)
        {
            if (lower.HasValue && value < lower.Value)
                return false;
            if (upper.HasValue && value > upper.Value)
                return false;
            return true;
        }

        private static bool Int32InRange(Int32 value, Int32? lower, Int32? upper)
        {
            if (lower.HasValue && value < lower.Value)
                return false;
            if (upper.HasValue && value > upper.Value)
                return false;
            return true;
        }

        private static readonly Dictionary<string, FilterDefinition> filterList = new Dictionary<string, FilterDefinition>()
        {
            { "crop", new FilterDefinition("crop", FilterCategory.APIReturnValue, int.MaxValue) },
            { "region", new FilterDefinition("region", FilterCategory.Unknown, int.MaxValue) },
            { "year", new FilterDefinition("year", FilterCategory.Unknown, int.MaxValue) },

            { "Irrigated", new FilterDefinition("isirrigated", FilterCategory.APIQueryString, int.MaxValue) },

            { "HarvestType", new FilterDefinition("HarvestTypes", FilterCategory.APIBody, int.MaxValue) },
            { "testSetSeason", new FilterDefinition("TestSetSeasons", FilterCategory.APIBody, 1) },
            { "PreviousYear", new FilterDefinition("TestStageYears", FilterCategory.APIBody, int.MaxValue) },
            { "PreviousCrop", new FilterDefinition("PreviousCrops", FilterCategory.APIBody, int.MaxValue) },
            { "SoilTypes", new FilterDefinition("SoilTypes", FilterCategory.APIBody, int.MaxValue) },
            { "Tillage", new FilterDefinition("Tillages", FilterCategory.APIBody, int.MaxValue) },
            { "Drought", new FilterDefinition("Droughts", FilterCategory.APIBody, int.MaxValue) },
            { "TestSetIds", new FilterDefinition("TestSetIds", FilterCategory.APIBody, int.MaxValue) },
            { "TestSetNames", new FilterDefinition("TestSetNames", FilterCategory.APIBody, int.MaxValue) },
            { "BrRepId", new FilterDefinition("BrRepId", FilterCategory.APIBody, int.MaxValue) },

            //Change from Return to Body
            { "experStageRefId", new FilterDefinition("ExperStageRefIds", FilterCategory.APIBody, int.MaxValue) },
            { "experTypeRefId", new FilterDefinition("ExperTypeRefIds", FilterCategory.APIBody, int.MaxValue) },
            
            { "UserDefinedRegions",  new FilterDefinition("UserDefinedRegion", FilterCategory.LookupFromAPIReturnValue, int.MaxValue) },
            { "countries", new FilterDefinition("country", FilterCategory.APIReturnValue, int.MaxValue) },
            { "sub-country", new FilterDefinition("subCountry", FilterCategory.APIReturnValue, int.MaxValue) },
            { "sub-sub-country", new FilterDefinition("subSubCountry", FilterCategory.APIReturnValue, int.MaxValue) },
            { "city", new FilterDefinition("city", FilterCategory.APIReturnValue, int.MaxValue) },
            { "loc", new FilterDefinition("locationId", FilterCategory.APIReturnValue, int.MaxValue) },
            { "field", new FilterDefinition("fieldId", FilterCategory.APIReturnValue, int.MaxValue) },

            { "plantingDateBegin", new CoerceDataFilterDefinition<DateTime>("plantingDate", FilterCategory.APIReturnValueRangeBegin, int.MaxValue, CoerceDateTimeValueFromAPI, DateTimeInRange) },
            { "plantingDateEnd", new CoerceDataFilterDefinition<DateTime>("plantingDate", FilterCategory.APIReturnValueRangeEnd, int.MaxValue, CoerceDateTimeValueFromAPI, DateTimeInRange) },
            { "harvestPopulationMin", new CoerceDataFilterDefinition<Int32>("harvestPopulation", FilterCategory.APIReturnValueRangeBegin, int.MaxValue, CoerceInt32ValueFromAPI, Int32InRange) },
            { "harvestPopulationMax", new CoerceDataFilterDefinition<Int32>("harvestPopulation", FilterCategory.APIReturnValueRangeEnd, int.MaxValue, CoerceInt32ValueFromAPI, Int32InRange) },
            { "rowsPlanted", new FilterDefinition("rowsPlanted", FilterCategory.APIReturnValue, int.MaxValue) },
            { "trackIntentId", new FilterDefinition("TrackIntentIds", FilterCategory.APIBody, int.MaxValue) }
        };

        public static FilterBase GetFilterDefinition(string filter, List<IFilter> postAPIFilters, UDRList udrList)
        {
            string[] filterParts = filter.Split('=');
            if (filterParts.GetLength(0) != 2)
            {
                throw new ArgumentException("filter must be in the form 'key=value1[&value2]'");
            }
            List<string> values = filterParts[1].Split('&').ToList();
            FilterDefinition filterDefinition;
            if (!filterList.TryGetValue(filterParts[0], out filterDefinition))
                return null;

            return filterDefinition.CreateFilter(values, postAPIFilters, udrList);
        }

        private readonly FilterDefinition definition;
        private readonly HashSet<string> values;

        public FilterCategory FilterCategory
        {
            get
            {
                return definition.FilterCategory;
            }
        }

        public string ParameterName
        {
            get
            {
                return definition.FilterName;
            }
        }

        public HashSet<string> Values
        {
            get
            {
                return values;
            }
        }

        protected FilterDefinition Definition
        {
            get
            {
                return this.definition;
            }
        }

        protected FilterBase(FilterDefinition definition)
        {
            this.definition = definition ?? throw new ArgumentNullException("definition");
            this.values = new HashSet<string>();
        }

        protected FilterBase(FilterDefinition definition, List<string> values)
        {
            this.definition = definition ?? throw new ArgumentNullException("definition");
            this.values = values != null ? new HashSet<string>(values) : throw new ArgumentNullException("values");
        }

        public abstract int NumberOfContents(int maxNumberSoFar);

        public abstract void ApplyToAPICall(List<string> queryParts, List<List<string>> bodyParts, List<string> harvestYears);

        public abstract bool ShouldFilter(Dictionary<string, dynamic> row);
    }
}
