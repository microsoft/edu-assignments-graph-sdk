// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for modules
    /// </summary>
    public class Module
    {
        /// <summary>
        /// Creates a Module with given Display Name and Description
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="displayName">Display Name</param>
        /// <param name="description">Description</param>
        /// <returns>EducationModule</returns>
        public static async Task<EducationModule> CreateSampleAsync(
            GraphServiceClient client,
            string classId,
            string displayName,
            string description)
        {
            try
            {
                var requestBody = new EducationModule
                {
                    DisplayName = displayName,
                    Description = description,
                };

                return await client.Education
                    .Classes[classId]
                    .Modules.PostAsync(requestBody);
            }
            catch (Exception ex) {
                throw new GraphException($"CreateSampleAsync call: {ex.Message}", ex, classId, displayName, description);
            }
        }

        /// <summary>
        /// Post the resource under a given module
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <returns>EducationModuleResource</returns>
        public static async Task<EducationModuleResource> PostResourceAsync(
            GraphServiceClient client,
            string classId,
            string moduleId,
            EducationModuleResource resource)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules[moduleId]
                    .Resources.PostAsync(resource);
            }
            catch (Exception ex)
            {
                throw new GraphException($"PostResourceAsync call: {ex.Message}", ex, classId, moduleId);
            }
        }

        /// <summary>
        /// Publishes a Module, changes the state of an educationModule from its original draft status to the published status
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <returns>EducationModule</returns>
        public static async Task<EducationModule> PublishAsync(
            GraphServiceClient client,
            string classId,
            string moduleId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules[moduleId]
                    .Publish
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"PublishAsync call: {ex.Message}", ex, classId, moduleId);
            }
        }

        /// <summary>
        /// Gets the list of resources belonging to the given module
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <returns>EducationModuleResourceCollectionResponse</returns>
        public static async Task<EducationModuleResourceCollectionResponse> GetModuleResourcesAsync(
            GraphServiceClient client,
            string classId,
            string moduleId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules[moduleId]
                    .Resources.GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetModuleResourcesAsync call: {ex.Message}", ex, classId, moduleId);
            }
        }

        /// <summary>
        /// Sets up the Resources Folder of Module
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <returns>EducationModule</returns>
        public static async Task<EducationModule> SetupResourcesFolder(
            GraphServiceClient client,
            string classId,
            string moduleId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules[moduleId]
                    .SetUpResourcesFolder
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"SetupResourcesFolder call: {ex.Message}", ex, classId, moduleId);
            }
        }

        /// <summary>
        /// Patch the module
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <param name="requestBody">Request body</param>
        /// <returns>EducationModule</returns>
        public static async Task<EducationModule> PatchAsync(
            GraphServiceClient client,
            string classId,
            string moduleId,
            EducationModule requestBody)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules[moduleId]
                    .PatchAsync(requestBody);
            }
            catch (Exception ex)
            {
                throw new GraphException($"PatchAsync call: {ex.Message}", ex, classId, moduleId, requestBody);
            }
        }

        /// <summary>
        /// Delete a module
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="moduleId">User module id</param>
        /// <returns></returns>
        public static async Task DeleteAsync(
            GraphServiceClient client,
            string classId,
            string moduleId)
        {
            try
            {
                await client.Education
                     .Classes[classId]
                     .Modules[moduleId]
                     .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"DeleteAsync call: {ex.Message}", ex, classId, moduleId);
            }
        }

    }
}
