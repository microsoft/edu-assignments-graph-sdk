// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using System;

namespace MicrosoftGraphSDK
{
    public class User
    {
        /// <summary>
        /// Returns current user information
        /// </summary>
        /// <param name="client"></param>
        /// <returns>Microsoft.Graph.User</returns>
        public static async Task<Microsoft.Graph.User> getUserInfo(
            GraphServiceClient client)
        {
            try
            {
                return await client.Me
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"getUserInfo call: {ex.Message}");
            }
        }
    }
}
