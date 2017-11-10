using H2HGermPlasmProcessor.Data.EntryMeans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public abstract class BaseBand
    {
        private readonly string bandName;

        public BaseBand(string bandName)
        {
            this.bandName = bandName ?? throw new ArgumentNullException("bandName");
        }

        protected abstract object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row);

        public string BandName
        {
            get
            {
                return this.bandName;
            }
        }

        public virtual void AddBandToSet(CancellationToken cancellationToken, List<GroupBySet> sets, string key, Dictionary<string, dynamic> row)
        {
            object value = GetBandValue(cancellationToken, row);
            foreach (GroupBySet set in sets)
            {
                if (value != null)
                {
                    set.Add(new GroupBy(key, value.ToString(), true));
                }
                else
                {
                    set.Add(new GroupBy(key, string.Empty, true));
                }
            }
        }
    }
}
