// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;

namespace microsoft_graph_sdk
{
    public class User
    {
        /// <summary>
        /// Returns current user information
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Microsoft.Graph.Beta.Models.User</returns>
        public static async Task<Microsoft.Graph.Beta.Models.User> getUserInfo(
            GraphServiceClient client)
        {
            return await client.Me
                .GetAsync();
        }
    }
}
