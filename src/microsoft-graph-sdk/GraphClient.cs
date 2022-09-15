using Azure.Identity;
using Microsoft.Graph;

namespace microsoft_graph_sdk
{
    public class GraphClient
    {
        public static GraphServiceClient GetDelegateClient(string tenantId, string applicationId, string userName, string password)
        {
            // Delegated permission
            // Multi-tenant apps can use "common" at tenant ID property
            // Single-tenant apps must use the tenant ID from the Azure portal
            var scopes = new[] { "User.Read" };
            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var userNamePasswordCredential = new UsernamePasswordCredential(
                userName, password, tenantId, applicationId, options);

            return new GraphServiceClient(userNamePasswordCredential, scopes);
        }

        public static GraphServiceClient GetApplicationClient(string tenantId, string applicationId, string secret)
        {
            // Application permission
            var clientSecretCredential = new ClientSecretCredential(tenantId, applicationId, secret);
            const string DefaultAuthScope = "https://graph.microsoft.com/.default";
            var authProvider = new TokenCredentialAuthProvider(
                clientSecretCredential,
                new List<string> { DefaultAuthScope });

            return new Microsoft.Graph.GraphServiceClient(authProvider);
        }
    }
}