using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface IEncryptedEnvVariable
    {
        Task<string> DecodeEnvVarAsync(string envVarName);
    }
}
