using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class EncryptedEnvVariable : IEncryptedEnvVariable
    {
        public async Task<string> DecodeEnvVarAsync(string envVarName)
        {
            // retrieve env var text
            var encryptedBase64Text = Environment.GetEnvironmentVariable(envVarName);
            // convert base64-encoded text to bytes
            var encryptedBytes = Convert.FromBase64String(encryptedBase64Text);
            // construct client
            using (var client = new AmazonKeyManagementServiceClient())
            {
                // construct request
                var decryptRequest = new DecryptRequest
                {
                    CiphertextBlob = new MemoryStream(encryptedBytes),
                };
                // call KMS to decrypt data
                var response = await client.DecryptAsync(decryptRequest);
                using (var plaintextStream = response.Plaintext)
                {
                    // get decrypted bytes
                    var plaintextBytes = plaintextStream.ToArray();
                    // convert decrypted bytes to ASCII text
                    var plaintext = Encoding.UTF8.GetString(plaintextBytes);
                    return plaintext;
                }
            }
        }
    }
}
