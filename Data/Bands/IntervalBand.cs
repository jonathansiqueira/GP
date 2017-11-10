using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class IntervalBandDateTime : BaseBand
    {
        public IntervalBandDateTime(BandDefinition definition) : base(definition.BandName)
        {
            if (definition.Interval == null) throw new ArgumentNullException("Interval");
            if (definition.MinValue == null) throw new ArgumentNullException("MinValue");
            if (definition.MaxValue == null) throw new ArgumentNullException("MaxValue");

            InitializeBands(definition);
        }

        private readonly List<ObservationBandItem<DateTime>> items = new List<ObservationBandItem<DateTime>>();
        private void InitializeBands(BandDefinition definition)
        {
            for (DateTime current = (DateTime)definition.MinValue; current < (DateTime)definition.MaxValue; current = current.AddDays(definition.Interval.Value))
            {
                items.Add(new ObservationBandItem<DateTime>(definition.BandName, current, current.AddDays(definition.Interval.Value)));
            }
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            dynamic value;
            if (row.TryGetValue(base.BandName, out value) && value is DateTime)
            {
                DateTime observation = (DateTime)value;
                ObservationBandItem<DateTime> item = items.Where(i => observation >= i.LowValue && observation < i.HighValue).FirstOrDefault();
                if (item != null)
                {
                    return item.HeaderBandName;
                }
            }
            return null;
        }
    }

    public class IntervalBandLong : BaseBand
    {
        public IntervalBandLong(BandDefinition definition) : base(definition.BandName)
        {
            if (definition.Interval == null) throw new ArgumentNullException("Interval");
            if (definition.MinValue == null) throw new ArgumentNullException("MinValue");
            if (definition.MaxValue == null) throw new ArgumentNullException("MaxValue");

            InitializeBands(definition);
        }

        private readonly List<ObservationBandItem<long>> items = new List<ObservationBandItem<long>>();
        private void InitializeBands(BandDefinition definition)
        {
            for (long current = (long)definition.MinValue; current < (long)definition.MaxValue; current += (long)definition.Interval.Value)
            {
                items.Add(new ObservationBandItem<long>(definition.BandName, current, current + (long)definition.Interval.Value));
            }
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            dynamic value;
            if (row.TryGetValue(base.BandName, out value) && value is long)
            {
                long observation = (long)value;
                ObservationBandItem<long> item = items.Where(i => observation >= i.LowValue && observation < i.HighValue).FirstOrDefault();
                if (item != null)
                {
                    return item.HeaderBandName;
                }
            }
            return null;
        }
    }

    public class IntervalBandDouble : BaseBand
    {
        public IntervalBandDouble(BandDefinition definition) : base(definition.BandName)
        {
            if (definition.Interval == null) throw new ArgumentNullException("Interval");
            if (definition.MinValue == null) throw new ArgumentNullException("MinValue");
            if (definition.MaxValue == null) throw new ArgumentNullException("MaxValue");

            InitializeBands(definition);
        }

        private readonly List<ObservationBandItem<double>> items = new List<ObservationBandItem<double>>();
        private void InitializeBands(BandDefinition definition)
        {
            for (double current = (double)definition.MinValue; current < (double)definition.MaxValue; current += (double)definition.Interval.Value)
            {
                items.Add(new ObservationBandItem<double>(definition.BandName, current, current + definition.Interval.Value));
            }
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            dynamic value;
            if (row.TryGetValue(base.BandName, out value) && value is double)
            {
                double observation = (double)value;
                ObservationBandItem<double> item = items.Where(i => observation >= i.LowValue && observation < i.HighValue).FirstOrDefault();
                if (item != null)
                {
                    return item.HeaderBandName;
                }
            }
            return null;
        }
    }
}
