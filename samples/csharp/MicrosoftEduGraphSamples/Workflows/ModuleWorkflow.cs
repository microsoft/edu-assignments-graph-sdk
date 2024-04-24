// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
using MicrosoftEduGraphSamples.Utilities;
using MicrosoftGraphSDK;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// The Workflow related to modules include creating a draft, publishing, setting up resources, adding link resources, 
    /// Word document resources, channel resources, assignment resources, patching, and deleting as a teacher.
    /// </summary>
    internal class ModuleWorkflow
    {
        private readonly IConfiguration _config;

        public ModuleWorkflow(IConfiguration configuration)
        {
            this._config = configuration;
            GlobalMethods.ValidateConfiguration(_config);
        }

        /// <summary>
        /// Workflow to create and publish the module
        /// </summary>
        public async Task ClassworkAsync()
        {
            try
            {
                // Get a Graph client using delegated permissions
                var graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Create a draft module
                var module = await Module.CreateSampleAssignmentAsync(graphClient, _config["classId"], "Sample Module " + DateTime.Now.ToString("dd/MM/yyyy HHmm"), "This Classwork module was created with Microsoft Graph SDK.");
                Console.WriteLine($"New module has been created: {module.Id} - {module.DisplayName} - {module.Status}");

                // Set up a resources folder
                await Module.SetupResourcesFolder(graphClient, _config["classId"], module.Id);

                // Add a link resource
                EducationModuleResource requestBody = new EducationModuleResource
                {
                    Resource = new EducationLinkResource
                    {
                        OdataType = "#microsoft.graph.educationLinkResource",
                        DisplayName = "Bing site",
                        Link = "https://www.bing.com",
                    },
                };
                var newResource = await Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Add a Word document resource
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationWordResource
                    {
                        OdataType = "#microsoft.graph.educationWordResource",
                        DisplayName = "test_word_file.docx",
                    },
                };
                newResource = await Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Add a channel resource
                var channels = await MicrosoftGraphSDK.Team.GetChannelsAsync(graphClient, _config["classId"]);
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationChannelResource
                    {
                        OdataType = "#microsoft.graph.educationChannelResource",
                        Url = "https://graph.microsoft.com/v1.0/teams/" + _config["classId"]  + "/channels/" + channels.Value[0].Id,
                        DisplayName = "General",
                    },
                };
                newResource = await Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Add a Assignment resource
                var assignment = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationLinkedAssignmentResource
                    {
                        OdataType = "#microsoft.graph.educationLinkedAssignmentResource",
                        Url = "https://graph.microsoft.com/v1.0/education/classes/" + _config["classId"] + "/assignments/" + assignment.Id,
                    },
                };
                newResource = await Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Patch the module
                var updateBody = new EducationModule
                {
                    DisplayName = module.DisplayName + " Updated",
                    Description = module.Description + " updated",
                };
                module = await Module.PatchAsync(graphClient, _config["classId"], module.Id.ToString(), updateBody);
                Console.WriteLine($"Module has been Patched: {module.DisplayName}");

                // Publish the module
                module = await Module.PublishAsync(graphClient, _config["classId"], module.Id.ToString());

                // Switch to student account
                graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // As Student, get module resources
                var resources = await Module.GetModuleResourcesAsync(graphClient, _config["classId"], module.Id.ToString());
                foreach ( var resource in resources.Value )
                {
                    Console.WriteLine($"Resource: {resource.Resource.DisplayName}");
                }

                // Switch to Teacher account
                graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // As Teacher, delete the module
                await Module.DeleteAsync(graphClient, _config["classId"], module.Id.ToString());
                Console.WriteLine($"Module has been Deleted: {module.DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClassworkAsync: {ex.ToString()}");
                return;
            }
        }
    }
}
