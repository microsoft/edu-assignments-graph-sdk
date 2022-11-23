// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

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
        /// <param name="client">Microsoft Graph service client</param>
        /// <returns>Microsoft.Graph.Beta.Models.User</returns>
        public static async Task<Microsoft.Graph.Beta.Models.User> GetUserInfoAsync(
            GraphServiceClient client)
        {
            try
            {
                return await client.Me
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetUserInfoAsync call: {ex.Message}", ex);
            }
        }

        // <summary>
        /// Lists assignments for the user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <returns>EducationAssignmentCollectionResponse</returns>
        public static async Task<EducationAssignmentCollectionResponse> GetMeAssignmentsAsync(
            GraphServiceClient client)
        {
            try
            {
                return await client.Education.Me.Assignments
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignmentsAsync call: {ex.Message}", ex);
            }
        }
    }
}