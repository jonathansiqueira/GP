using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.UDR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace H2HGermPlasmProcessor.Data.Bands
{
    public class UDRBand : BaseBand
    {
        private readonly UDRList udrList;
        public UDRBand(BandDefinition definition, UDRList udrList) : base(definition.BandingGroup)
        {
            this.udrList = udrList ?? throw new ArgumentNullException("udrList");           
        }

        protected override object GetBandValue(CancellationToken cancellationToken, Dictionary<string, dynamic> row)
        {
            throw new NotImplementedException();
        }

        public override void AddBandToSet(CancellationToken cancellationToken, List<GroupBySet> sets, string key, Dictionary<string, dynamic> row)
        {
            Dictionary<string, UDR.UDR> matchingUDrs = this.udrList.GetMatches(row);

            if (sets.Count == 1)
            {
                for (int i = 1; i < matchingUDrs.Keys.Count;i++)
                {
                    sets.Add(JsonConvert.DeserializeObject<GroupBySet>(JsonConvert.SerializeObject(sets[0])));
                }
            }
            int index = 0;
            foreach (string udrName in matchingUDrs.Keys)
            {
                sets[index++].Add(new GroupBy(base.BandName, udrName, true));
            }
        }

        internal void AddBandDefinition(BandDefinition toUse)
        {
            throw new NotImplementedException();
        }
    }
}
