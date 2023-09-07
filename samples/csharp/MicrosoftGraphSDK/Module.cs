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
        /// <param name="educationModule">EducationModule object</param>
        /// <returns>EducationModule</returns>
        public static async Task<EducationModule> CreateAsync(
            GraphServiceClient client,
            string classId,
            EducationModule educationModule)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Modules.PostAsync(educationModule);
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAsync call: {ex.Message}", ex, classId, educationModule);
            }
        }

        /// <summary>
        /// Post the Resources of Module
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

    }
}