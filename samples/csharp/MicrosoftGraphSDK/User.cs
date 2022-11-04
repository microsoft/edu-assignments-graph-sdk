// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using System;

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
        /// <returns>Microsoft.Graph.User</returns>
        public static async Task<Microsoft.Graph.User> GetUserInfo(
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
                throw new GraphException($"GetUserInfo call: {ex.Message}");
            }
        }

        // <summary>
        /// Lists assignments for the user
        /// </summary>
        public static async Task<IEducationUserAssignmentsCollectionPage> GetMeAssignments(
            GraphServiceClient client)
        {
            try
            {
                return await client.Education.Me.Assignments
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignments call: {ex.Message}");
            }
        }
    }
}
