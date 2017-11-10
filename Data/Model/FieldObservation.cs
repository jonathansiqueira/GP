using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class FieldObservation
    {
        private readonly string obsRefCd;
        private readonly string name;
        private readonly string strValue;
        private readonly DateTime? dtValue;
        private readonly double? numValue;
        private readonly int repetition;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="obsRefCd">observation ref cd of observation</param>
        /// <param name="name">name of observation</param>
        /// <param name="strValue">possible string value of observation</param>
        /// <param name="dtValue">possible date time value of observation</param>
        /// <param name="numValue">possible number value of observation</param>
        /// <param name="repetition"></param>
        [JsonConstructor]
        public FieldObservation(
            string obsRefCd,
            string name,
            string strValue,
            DateTime? dtValue,
            double? numValue,
            int repetition)
        {
            if (string.IsNullOrEmpty(obsRefCd)) throw new ArgumentNullException("obsRefCd");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            this.obsRefCd = obsRefCd;
            this.name = name;
            this.strValue = strValue;
            this.dtValue = dtValue;
            this.numValue = numValue;
            this.repetition = repetition;
        }

        /// <summary>
        /// Observatoin Ref Code
        /// </summary>
        [JsonProperty()]
        public string ObsRefCd
        {
            get
            {
                return this.obsRefCd;
            }
        }

        /// <summary>
        /// Name of observation
        /// </summary>
        [JsonProperty()]
        public string Name
        {
            get
            {
                return this.name;
            }
        }

        /// <summary>
        /// string value of observation, if string type
        /// </summary>
        [JsonProperty()]
        public string StrValue
        {
            get
            {
                return this.strValue;
            }
        }

        /// <summary>
        /// DateTime value of observation, if DateTime type
        /// </summary>
        [JsonProperty()]
        public DateTime? DtValue
        {
            get
            {
                return this.dtValue;
            }
        }

        /// <summary>
        /// numeric value of observation, if numeric type
        /// </summary>
        [JsonProperty()]
        public double? NumValue
        {
            get
            {
                return this.numValue;
            }
        }

        /// <summary>
        /// repitition number of observation, to indicate if it was taken more than one time
        /// </summary>
        [JsonProperty()]
        public int Repetition
        {
            get
            {
                return this.repetition;
            }
        }

        public override string ToString()
        {
            if (this.strValue != null)
                return this.strValue;
            if (this.numValue.HasValue)
                return this.numValue.Value.ToString();
            if (this.dtValue.HasValue)
                return this.dtValue.Value.ToString("YYYY-MM-DD");
            return string.Empty;
        }
    }
}
