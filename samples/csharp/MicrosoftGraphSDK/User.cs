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
        /// <returns>Microsoft.Graph.Models.User</returns>
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
        /// Lists assignments for the given user
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

        // <summary>
        /// Lists top N assignments for the given user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="top">Top</param>
        /// <returns>EducationAssignmentCollectionResponse</returns>
        public static async Task<EducationAssignmentCollectionResponse> GetMeAssignmentsWithTopAsync(
            GraphServiceClient client,
            int top)
        {
            try
            {
                return await client.Education.Me.Assignments
                    .GetAsync(requestConfiguration => { 
                        requestConfiguration.QueryParameters.Top = top;
                    });
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignmentsWithTopAsync call: {ex.Message}", ex, top);
            }
        }

        // <summary>
        /// Lists assignments assigned to a user for all classes
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="userId">User Id</param>
        /// <returns>EducationAssignmentCollectionResponse</returns>
        public static async Task<EducationAssignmentCollectionResponse> GetUserAssignmentsAsync(
            GraphServiceClient client,
            string userId)
        {
            try {
                return await client.Education
                    .Users[userId]
                    .Assignments
                    .GetAsync();
            }
            catch (Exception ex) {
                throw new GraphException($"GetUserAssignmentsAsync call: {ex.Message}", userId, ex);
            }
        }
    }
}
