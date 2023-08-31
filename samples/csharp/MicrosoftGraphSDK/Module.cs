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
        public static async Task<EducationModule> CreateAsync(
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
                throw new GraphException($"CreateAsync call: {ex.Message}", ex, classId);
            }
        }

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
                throw new GraphException($"PostResourceAsync call: {ex.Message}", ex, classId);
            }
        }

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
                throw new GraphException($"PublishAsync call: {ex.Message}", ex, classId);
            }
        }

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
                throw new GraphException($"GetModuleResourcesAsync call: {ex.Message}", ex, classId);
            }
        }
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
                throw new GraphException($"SetupResourcesFolder call: {ex.Message}", ex, classId);
            }
        }

    }
}
