// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

namespace MicrosoftGraphSDK
{
        public static class Team
    {
        public static async Task<TeamCollectionResponse> GetJoinedTeams(
            this GraphServiceClient client)
        {
            try
            {
                return await client.Me.JoinedTeams
                 .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetJoinedTeams call: {ex.Message}");
            }
        }
    }
}
