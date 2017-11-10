using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class FieldStressGrowthStage
    {
        private readonly string growthStageName;

        private readonly string stressLevelText;

        private readonly double? stressLevelScore;

        /// <summary>
        /// constructor for the growth stage
        /// </summary>
        /// <param name="growthStageName"></param>
        /// <param name="stressLevelText"></param>
        /// <param name="stressLevelScore"></param>
        [JsonConstructor]
        public FieldStressGrowthStage(string growthStageName, string stressLevelText, double? stressLevelScore)
        {
            if (string.IsNullOrEmpty(growthStageName)) throw new ArgumentNullException("growthStageName");

            this.growthStageName = growthStageName;
            this.stressLevelText = stressLevelText;
            this.stressLevelScore = stressLevelScore;
        }

        /// <summary>
        /// the growth stage affiliated with the field stress
        /// </summary>
        public string GrowthStageName { get { return this.growthStageName; } }

        /// <summary>
        /// if a stress level text is applicable, contains the text
        /// </summary>
        public string StressLevelText { get { return this.stressLevelText; } }

        /// <summary>
        /// if the stress level has a score, contains the numeric score
        /// </summary>
        public double? StressLevelScore { get { return this.stressLevelScore; } }
    }
}
