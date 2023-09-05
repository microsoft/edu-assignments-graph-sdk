// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

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
        /// <returns>TeamCollectionResponse</returns>
        public static async Task<TeamCollectionResponse> GetJoinedTeamsAsync(
             this GraphServiceClient client)
        {
            try
            {
                return await client.Me.JoinedTeams
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetJoinedTeamsAsync call: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns teams information joined by an specific user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="userId">User id</param>
        /// <returns>TeamCollectionResponse</returns>
        public static async Task<TeamCollectionResponse> GetUserJoinedTeamsAsync(
             this GraphServiceClient client,
             string userId)
        {
            try
            {
                return await client.Users[userId].JoinedTeams
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetUserJoinedTeamsAsync call: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Returns channels information for a team
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User id</param>
        /// <returns>ChannelCollectionResponse</returns>
        public static async Task<ChannelCollectionResponse> GetChannelsAsync(
             this GraphServiceClient client,
             string classId)
        {
            try
            {
                return await client.Teams[classId].Channels.GetAsync(requestConfig => {requestConfig.QueryParameters.Filter = "displayName eq 'General'"; });
            }
            catch (Exception ex)
            {
                throw new GraphException($"ChannelCollectionResponse call: {ex.Message}", ex, classId);
            }
        }
    }
}
