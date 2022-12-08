// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for teams
    /// </summary>
    public static class Team
    {
        /// <summary>
        /// Returns teams information joined by the user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <returns>V1.0: IUserJoinedTeamsCollectionPage | BETA: IUserJoinedTeamsCollectionWithReferencesPage</returns>
        public static async Task<IUserJoinedTeamsCollectionPage> GetJoinedTeamsAsync(
             this GraphServiceClient client)
        {
            try
            {
                return await client.Me.JoinedTeams
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetJoinedTeamsAsync call: {ex.Message}", ex);
            }
        }
    }
}
