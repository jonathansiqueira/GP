using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Model
{
    public class FieldStress
    {
        private readonly long geographyDimKey;
        private readonly Dictionary<string, FieldStressCategory> categories;

        /// <summary>
        /// only constructur
        /// </summary>
        /// <param name="geographyDimKey">geography dim key is the key for the overall field stress</param>
        [JsonConstructor]
        public FieldStress(long geographyDimKey, Dictionary<string, FieldStressCategory> categories)
        {
            this.geographyDimKey = geographyDimKey;
            this.categories = categories ?? throw new ArgumentNullException("categories");
        }

        /// <summary>
        /// geography dim key which provides the key for this field stress
        /// </summary>
        public long GeographyDimKey { get { return this.geographyDimKey; } }

        /// <summary>
        /// field Stress Categories
        /// </summary>
        public Dictionary<string, FieldStressCategory> Categories { get { return this.categories; } }
    }
}
