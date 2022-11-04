// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using MicrosoftGraphSDK;

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
        public static async Task<Microsoft.Graph.Beta.Models.User> GetUserInfo(
            GraphServiceClient client)
        {
            try
            {
                return await client.Me
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
        public static async Task<EducationAssignmentCollectionResponse> GetMeAssignments(
            GraphServiceClient client)
        {
            try
            {
                return await client.Education.Me.Assignments
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignments call: {ex.Message}");
            }
        }
    }
}