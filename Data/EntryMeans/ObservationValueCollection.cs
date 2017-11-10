using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{

    [Serializable]
    public class ObservationValueCollection
    {
        private readonly Dictionary<string, ObservationValue> observations = new Dictionary<string, ObservationValue>();

        private static object sema = new object();

        public void AddObservation(string traitName, double? observedValue, double testMean)
        {
            if (!observedValue.HasValue)
                return;
            ObservationValue observation;
            if (!observations.TryGetValue(traitName, out observation))
            {
                lock (sema)
                {
                    if (!observations.TryGetValue(traitName, out observation))
                    {
                        observation = new ObservationValue(observedValue.Value, testMean, observedValue.Value, testMean, 1);
                        observations.Add(traitName, observation);
                        return;
                    }
                }
            }
            lock (sema)
            {
                observations[traitName] += new ObservationValue(observedValue.Value, testMean, observedValue.Value, testMean, 1);
            }
        }

        [JsonProperty("observations")]
        public Dictionary<string, ObservationValue> Observations
        {
            get
            {
                return this.observations;
            }
        }
    }
}
