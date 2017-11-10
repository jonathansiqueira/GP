using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public class CoerceDataFilterDefinition<T> : FilterDefinition
        where T : struct
    {
        private readonly Func<object, T> coerceValue;
        private readonly Func<T, Nullable<T>, Nullable<T>, bool> inRange;


        public CoerceDataFilterDefinition(string filterName, FilterCategory filterCategory, int maxInBody, Func<object, T> coerceValue, Func<T, Nullable<T>, Nullable<T>, bool> inRange)
            : this(filterName, filterCategory, maxInBody, LookupAPICategory.Unknown, coerceValue, inRange)
        {
        }

        public CoerceDataFilterDefinition(string filterName, FilterCategory filterCategory, int maxInBody, LookupAPICategory lookupAPICategory, Func<object, T> coerceValue, Func<T, Nullable<T>, Nullable<T>, bool> inRange)
            : base(filterName, filterCategory, maxInBody, lookupAPICategory)
        {
            this.coerceValue = coerceValue;
            this.inRange = inRange;
        }

        public T CoerceValue(dynamic inputValue)
        {
            try
            {
                return coerceValue(inputValue);
            }
            catch(Exception exc)
            {
                throw new Exception($"failed to coerce {inputValue} to {typeof(T).FullName}", exc);
            }
        }

        public bool InRange(T value, Nullable<T> lower, Nullable<T> upper)
        {
            try
            {
                return inRange(value, lower, upper);
            }
            catch (Exception exc)
            {
                throw new Exception($"failed to verify if {value} was in range for type: {typeof(T).FullName}", exc);
            }
        }

        public override FilterBase CreateFilter(List<string> values, List<IFilter> postAPIFilters, UDRList udrList)
        {
            if (values.Count == 0)
                throw new ArgumentOutOfRangeException($"{this.FilterName} must have values supplied");
            switch (this.FilterCategory)
            {
                case FilterCategory.APIReturnValueRangeBegin:
                case FilterCategory.APIReturnValueRangeEnd:
                    {
                        IFilter existingFilter = postAPIFilters.FirstOrDefault(f => f.ParameterName == this.FilterName);
                        T value = coerceValue(values[0]);
                        if (existingFilter == null)
                            return new ReturnValueRangeFilter<T>(this, value);
                        else
                        {
                            ((ReturnValueRangeFilter<T>)existingFilter).InitializeWithDefinition(this, value);
                            return ((ReturnValueRangeFilter<T>)existingFilter);
                        }
                    }
                default:
                    throw new ArgumentOutOfRangeException($"cannot construct a filter of category: {this.FilterCategory.ToString()}.");
            }
        }
    }
}
