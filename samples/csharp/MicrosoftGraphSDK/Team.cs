﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
        /// Returns teams information
        /// </summary>
        /// <param name="client"></param>
        /// <returns>IUserJoinedTeamsCollectionPage</returns>
        public static async Task<IUserJoinedTeamsCollectionPage> GetJoinedTeams(
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
                throw new GraphException($"GetJoinedTeams call: {ex.Message}");
            }
        }
    }
}