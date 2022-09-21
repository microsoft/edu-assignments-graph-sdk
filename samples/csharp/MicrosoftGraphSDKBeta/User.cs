// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for logged user
    /// </summary>
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
            try
            {
                return await client.Me
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"getUserInfo call: {ex.Message}");
            }
        }
    }
}
