using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.UDR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public static class BandFactory
    {
        public static BaseBand Create(Dictionary<string, BaseBand> existingBands, ILambdaContext context, HttpClient httpClient, BandDefinition definition, IProductAnalyticsAPIClient client, UDRList udrList)
        {
            BandDefinition toUse = TranslateDefinition(definition);
            Func<BandDefinition, BaseBand> creationFunction = AllBands.GetCreator(toUse.BandName);

            BaseBand band;
            if (creationFunction != null)
            {
                band = creationFunction(toUse);
            }
            else if (toUse.BandingGroup == "Field Observations")
            {
                if (existingBands.TryGetValue(toUse.BandingGroup, out band) && band is FieldObservationBand)
                {
                    ((FieldObservationBand)band).AddBandDefinition(toUse);
                }
                else
                {
                    band = new FieldObservationBand(context, httpClient, toUse, client);
                    existingBands.Add(toUse.BandingGroup, band);
                }
                return band;
            }
            else if (toUse.Category == "Field Stress")
            {
                band = new FieldStressBand(context, httpClient, toUse, client);
            }
            else if (toUse.BandingGroup == "UDR")
            {
                if (!existingBands.TryGetValue(toUse.BandingGroup, out band))
                {                  
                    band = new UDRBand(toUse, udrList);
                    existingBands.Add(toUse.BandingGroup, band);
                }
                return band;                
            }
            else if (toUse.BandingGroup == "TestMeans" && toUse.MinValue != null && toUse.MaxValue != null && toUse.Interval.HasValue)
            {
                band = new ObservationBand(toUse);
            }
            else if (toUse.MinValue != null && toUse.MaxValue != null && toUse.Interval.HasValue)
            {
                if (toUse.MinValue is DateTime)
                    band = new IntervalBandDateTime(toUse);
                else if (toUse.MinValue is double)
                    band = new IntervalBandDouble(toUse);
                else if (toUse.MinValue is long)
                    band = new IntervalBandLong(toUse);
                else
                    throw new ArgumentOutOfRangeException($"Band: {toUse.BandName} is not handled or is not defined properly.");

            }
            else if (toUse.MinValue == null || toUse.MaxValue == null || toUse.Interval == null)
            {
                band = new ColumnBand(toUse.BandName);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Band: {toUse.BandName} is not handled or is not defined properly.");
            }
            existingBands.Add(band.BandName, band);
            return band;
        }

        private static BandDefinition TranslateDefinition(BandDefinition submitted)
        {
            string translatedName;
            if (!BandNameTranslation.TryGetValue(submitted, out translatedName))
                return submitted;
            return new BandDefinition(translatedName, submitted.Category, submitted.BandingGroup, submitted.MinValue, submitted.MaxValue, submitted.Interval);
        }

        private static readonly Dictionary<BandDefinition, string> BandNameTranslation = new Dictionary<BandDefinition, string>()
        {
            { new BandDefinition("Harvest Year", "", "Agronomics"), "harvestYear" },
            { new BandDefinition("Harvest Population", "", "Agronomics"), "harvestPopulation" },
            { new BandDefinition("# Rows Planted", "", "Agronomics"), "rowsPlanted" },
            { new BandDefinition("Planting Date", "", "Agronomics"), "plantingDate" },
            { new BandDefinition("Experiment Type", "", "Experiment"), "experTypeRefId" },
            { new BandDefinition("Experiment Stage", "", "Experiment"), "experStageRefId" },
            { new BandDefinition("Harvest Type", "", "Experiment"), "harvestTypeName" },
            { new BandDefinition("Location", "", "Geography"), "locationId" },
            { new BandDefinition("Country", "", "Geography"), "country" },
            { new BandDefinition("SubCountry", "", "Geography"), "subCountry" },
            { new BandDefinition("SubSubCountry", "", "Geography"), "subSubCountry" },
            { new BandDefinition("Region", "", "Geography"), "region" },
            { new BandDefinition("Flowering Rating", "Field Stress", "Drought"), "Flowering" },
            { new BandDefinition("Grainfill Rating", "Field Stress", "Drought"), "Grainfill" },
            { new BandDefinition("Overall Rating", "Field Stress", "Drought"), "Overall" },
            { new BandDefinition("Vegetative Rating", "Field Stress", "Drought"), "Vegetative" },
            { new BandDefinition("Flowering Rating", "Field Stress", "Daytime Heat"), "Flowering" },
            { new BandDefinition("Grainfill Rating", "Field Stress", "Daytime Heat"), "Grainfill" },
            { new BandDefinition("Overall Rating", "Field Stress", "Daytime Heat"), "Overall" },
            { new BandDefinition("Vegetative Rating", "Field Stress", "Daytime Heat"), "Vegetative" },
            { new BandDefinition("Flowering Rating", "Field Stress", "Nighttime Heat"), "Flowering" }
        };
    }
}
