using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.EntryMeans
{
    public class ObservationValue
    {
        private readonly double entryMean;
        private readonly double testMean;
        private readonly double summarizedEntryMean;
        private readonly double summarizedTestMean;
        private readonly int sampleSize;

        [JsonConstructor]
        public ObservationValue(double entryMean, double testMean, double summarizedEntryMean, double summarizedTestMean, int sampleSize)
        {
            this.entryMean = entryMean;
            this.testMean = testMean;
            this.summarizedEntryMean = summarizedEntryMean;
            this.summarizedTestMean = summarizedTestMean;
            this.sampleSize = sampleSize;
        }
        [JsonProperty("entryMean")]
        public double EntryMean
        {
            get
            {
                return entryMean;
            }
        }

        [JsonProperty("testMean")]
        public double TestMean
        {
            get
            {
                return testMean;
            }
        }

        [JsonProperty("summarizedEntryMean")]
        public double SummarizedEntryMean
        {
            get
            {
                return summarizedEntryMean;
            }
        }


        [JsonProperty("summarizedTestMean")]
        public double SummarizedTestMean
        {
            get
            {
                return summarizedTestMean;
            }
        }


        [JsonProperty("sampleSize")]
        public int SampleSize
        {
            get
            {
                return sampleSize;
            }
        }

        public static ObservationValue operator +(ObservationValue a, ObservationValue b)
        {
            int sampleSize = a.sampleSize + b.sampleSize;
            double summarizedEntryMean = a.summarizedEntryMean + b.summarizedEntryMean;
            double summarizedTestMean = a.summarizedTestMean + b.summarizedTestMean;
            double entryMean = summarizedEntryMean / sampleSize;
            double testMean = summarizedTestMean / sampleSize;

            return new ObservationValue(entryMean, testMean, summarizedEntryMean, summarizedTestMean, sampleSize);
        }
    }
}
