using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Filter
{
    public enum FilterCategory
    {
        Unknown,
        APIQueryString,
        APIBody,
        APIReturnValue,
        LookupFromAPIReturnValue,
        APIReturnValueRangeBegin,
        APIReturnValueRangeEnd
    }
}
