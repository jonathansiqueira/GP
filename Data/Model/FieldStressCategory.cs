using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class FieldStressCategory
    {
        private readonly string name;
        private readonly Dictionary<string, FieldStressGrowthStage> stages;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">the field stress category name</param>
        [JsonConstructor]
        public FieldStressCategory(string name, IEnumerable<FieldStressGrowthStage> stages)
        {
            this.name = name ?? throw new ArgumentNullException("name");
            if (stages == null) throw new ArgumentNullException("stages");
            this.stages = new Dictionary<string, FieldStressGrowthStage>();
            foreach(FieldStressGrowthStage stage in stages)
            {
                this.stages.Add(stage.GrowthStageName, stage);
            }
        }

        /// <summary>
        /// Name of the field stress category
        /// </summary>
        [JsonProperty("name")]
        public string Name { get { return this.name; } }

        /// <summary>
        /// the gwowth stages affiliated with this stress category
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, FieldStressGrowthStage> StagesDictionary { get { return this.stages; } }

        [JsonProperty("stages")]
        public IEnumerable<FieldStressGrowthStage> Stages { get { return this.stages.Values; } }
    }
}
