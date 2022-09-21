// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Microsoft.Graph;

namespace MicrosoftGraphSDK
{
    public class GraphClient
    {
        /// <summary>
        /// Creates a Graph Service Client using Delegated permissions
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationId"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>GraphServiceClient</returns>
        public static GraphServiceClient GetDelegateClient(string tenantId, string applicationId, string userName, string password)
        {
            try
            {
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
            catch (Exception ex)
            {
                throw new GraphException($"GetDelegateClient call: {ex.Message}", tenantId, applicationId, userName);
            }
        }

        /// <summary>
        /// Creates a Graph Service Client using Application permissions
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationId"></param>
        /// <param name="secret"></param>
        /// <returns>GraphServiceClient</returns>
        public static GraphServiceClient GetApplicationClient(string tenantId, string applicationId, string secret)
        {
            try
            {
                var clientSecretCredential = new ClientSecretCredential(tenantId, applicationId, secret);
                const string DefaultAuthScope = "https://graph.microsoft.com/.default";
                var authProvider = new TokenCredentialAuthProvider(
                    clientSecretCredential,
                    new List<string> { DefaultAuthScope });

                return new Microsoft.Graph.GraphServiceClient(authProvider);
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetApplicationClient call: {ex.Message}", tenantId, applicationId);
            }
        }
    }
}