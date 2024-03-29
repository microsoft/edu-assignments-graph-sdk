﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Identity;
using Microsoft.Graph.Beta;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for creating graph client object
    /// </summary>
    public class GraphClient
    {
        /// <summary>
        /// Creates a Graph Service Client using Delegated permissions
        /// </summary>
        /// <param name="tenantId">The Azure Directory tenant identifier</param>
        /// <param name="applicationId">Identifier for the application</param>
        /// <param name="userName">Name for the user</param>
        /// <param name="password">Password for the user</param>
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
                throw new GraphException($"GetDelegateClient call: {ex.Message}", ex, tenantId, applicationId, userName);
            }
        }

        /// <summary>
        /// Creates a Graph Service Client using Delegated permissions
        /// </summary>
        /// <param name="clientId">Identifier for the application</param>
        /// <returns>GraphServiceClient</returns>
        public static GraphServiceClient GetDelegateClient(string clientId)
        {
            try
            {
                var credential = new InteractiveBrowserCredential(
                new InteractiveBrowserCredentialOptions
                {
                    ClientId = clientId,
                });

                return new GraphServiceClient(credential, new[] { "EduAssignments.ReadWrite", "EduCurricula.ReadWrite" });
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetDelegateClient call: {ex.Message}", ex, clientId);
            }
        }

        /// <summary>
        /// Creates a Graph Service Client using Application permissions
        /// </summary>
        /// <param name="tenantId">The Azure Directory tenant identifier</param>
        /// <param name="applicationId">Identifier for the application</param>
        /// <param name="secret">Application secret</param>
        /// <returns>GraphServiceClient</returns>
        public static GraphServiceClient GetApplicationClient(string tenantId, string applicationId, string secret)
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