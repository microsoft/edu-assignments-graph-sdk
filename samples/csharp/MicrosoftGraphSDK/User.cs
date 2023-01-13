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
        /// <param name="client">Microsoft Graph service client</param>
        /// <returns>Microsoft.Graph.User</returns>
        public static async Task<Microsoft.Graph.User> GetUserInfoAsync(
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
                throw new GraphException($"GetUserInfoAsync call: {ex.Message}", ex);
            }
        }

        // <summary>
        /// Lists assignments for the given user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <returns>IEducationUserAssignmentsCollectionPage</returns>
        public static async Task<IEducationUserAssignmentsCollectionPage> GetMeAssignmentsAsync(
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
                throw new GraphException($"GetMeAssignmentsAsync call: {ex.Message}", ex);
            }
        }

        // <summary>
        /// Lists top N assignments for the given user
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="top">Top</param>
        /// <returns>IEducationUserAssignmentsCollectionPage</returns>
        public static async Task<IEducationUserAssignmentsCollectionPage> GetMeAssignmentsWithTopAsync(
            GraphServiceClient client,
            int top)
        {
            try
            {
                return await client.Education.Me.Assignments
                    .Request()
                    .Top(top)
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignmentsWithTopAsync call: {ex.Message}", ex);
            }
        }
    }
}
