using Azure.Identity;
using Microsoft.Graph.Beta;
using MicrosoftGraphSDK;

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

        public static async Task<GraphServiceClient> GetApplicationClient(string tenantId, string applicationId, string secret)
        {
            try
            {
                var scopes = new[] { "https://graph.microsoft.com/.default" };

                var options = new ClientSecretCredentialOptions
                {
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                };

                // Learn more: https://learn.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
                var clientSecretCredential = new ClientSecretCredential(
                    tenantId, applicationId, secret, options);

                return new GraphServiceClient(clientSecretCredential, scopes);
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetApplicationClient call: {ex.Message}", ex, tenantId, applicationId);
            }
        }
    }
}
