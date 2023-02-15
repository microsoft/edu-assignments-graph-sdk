using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MicrosoftEduImportFromGoogle
{
    internal class MicrosoftAuthenticator
    {
        public static async Task<GraphServiceClient> InitializeMicrosoftGraphClient(string clientID)
        {
            var credential = new InteractiveBrowserCredential(
                new InteractiveBrowserCredentialOptions
                {
                    ClientId = clientID,
                });
            // Use the credential to get an access token           
           // return await credential.GetTokenAsync(new TokenRequestContext(new[] { "EduAssignments.ReadWrite" }));

            return new GraphServiceClient(credential, new[] { "EduAssignments.ReadWrite" });
        }
    }
}
