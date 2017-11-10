using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class ObservationBand : BaseBand
    {
        public ObservationBand(BandDefinition definition) : base(definition.BandName)
        {
            if (definition.Interval == null) throw new ArgumentNullException("Interval");
            if (definition.MinValue == null) throw new ArgumentNullException("MinValue");
            if (definition.MaxValue == null) throw new ArgumentNullException("MaxValue");

            InitializeBands(definition);
        }

        private readonly List<ObservationBandItem<double>> items = new List<ObservationBandItem<double>>();
        private void InitializeBands(BandDefinition definition)
        {
            definition = definition.CoerceObjectsToDouble();
            for (double current = (double)definition.MinValue; current < (double)definition.MaxValue; current += definition.Interval.Value)
            {
                items.Add(new ObservationBandItem<double>(definition.BandName, current, current + definition.Interval.Value));
            }
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            const string observationRefCd = "observationRefCd";
            const string testMean = "testMean";

            dynamic value;
            if (row.TryGetValue(observationRefCd, out value) && value.ToString() == base.BandName)
            {
                if (row.TryGetValue(testMean, out value) && value is double)
                {
                    double observation = (double)value;
                    ObservationBandItem<double> item = items.Where(i => observation >= i.LowValue && observation < i.HighValue).FirstOrDefault();
                    if (item != null)
                    {
                        return item.HeaderBandName;
                    }
                }
            }
            return null;
        }
    }
}
