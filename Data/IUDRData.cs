using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface IUDRData
    {
        Task<IEnumerable<UDRGeography>> GetUDRsForCrop(HttpClient httpClient, string crop, IEnumerable<string> udrNames);
    }
}
