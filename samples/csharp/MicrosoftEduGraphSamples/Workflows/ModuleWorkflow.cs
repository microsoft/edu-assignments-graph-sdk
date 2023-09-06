// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using MicrosoftGraphSDK;
using System.Resources;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the workflows related to Modules
    /// </summary>
    internal class ModuleWorkflow
    {
        private readonly IConfiguration _config;

        public ModuleWorkflow(IConfiguration configuration)
        {
            this._config = configuration;

            // Verify and throw exception for input values if null or empty
            try
            {
                if (string.IsNullOrEmpty(_config["classId"]))
                {
                    throw new Exception("Missing classId please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["tenantId"]))
                {
                    throw new Exception("Missing tenantId please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["secret"]))
                {
                    throw new Exception("Missing secret please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["appId"]))
                {
                    throw new Exception("Missing appId please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["teacherAccount"]))
                {
                    throw new Exception("Missing teacherAccount please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["studentAccount"]))
                {
                    throw new Exception("Missing studentAccount please check appconfig.json file.");
                }
                else if (string.IsNullOrEmpty(_config["password"]))
                {
                    throw new Exception("Missing password please check appconfig.json file.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
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

                // Create a draft module.
                var module = await MicrosoftGraphSDK.Module.CreateAsync(graphClient, _config["classId"], "Sample Module "+ DateTime.Now.ToString("dd/MM/yyyy HHmm"), "This is the first module created using Graph SDK.");
                Console.WriteLine($"New module has been created: {module.Id} - {module.DisplayName} - {module.Status}");
                await MicrosoftGraphSDK.Module.SetupResourcesFolder(graphClient, _config["classId"], module.Id);

                // Add a link resource.
                EducationModuleResource requestBody = new EducationModuleResource
                {
                    Resource = new EducationLinkResource
                    {
                        OdataType = "#microsoft.graph.educationLinkResource",
                        DisplayName = "Bing site",
                        Link = "https://www.bing.com",
                    },
                };
                // Adding a link resource
                var newResource = await MicrosoftGraphSDK.Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Add a new word document resource.
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationWordResource
                    {
                        OdataType = "#microsoft.graph.educationWordResource",
                        DisplayName = "test_word_file.docx",

                    },
                };
                // A new Word file is added
                newResource = await MicrosoftGraphSDK.Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Get the General channel
                var channels = await MicrosoftGraphSDK.Team.GetChannelsAsync(graphClient, _config["classId"]);

                // Add a channel resource
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationChannelResource
                    {
                        OdataType = "#microsoft.graph.educationChannelResource",
                        Url = "https://graph.microsoft.com/v1.0/teams/"+ _config["classId"] + "/channels/"+ channels.Value[0].Id,
                        DisplayName = "General",
                    },
                };
                // Added a channel resource
                newResource = await MicrosoftGraphSDK.Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Assignment resource
                var assignment = await MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                var assignmentId = assignment.Id;
                requestBody = new EducationModuleResource
                {
                    Resource = new EducationLinkedAssignmentResource
                    {
                        OdataType = "#microsoft.graph.educationLinkedAssignmentResource",
                        Url = "https://graph.microsoft.com/v1.0/education/classes/"+ _config["classId"] +"/assignments/"+assignmentId,
                    },
                };
                // Added an assignment resource
                newResource = await MicrosoftGraphSDK.Module.PostResourceAsync(graphClient, _config["classId"], module.Id.ToString(), requestBody);

                // Publish the module.
                module = await MicrosoftGraphSDK.Module.PublishAsync(graphClient, _config["classId"], module.Id.ToString());

                // Switch to student account
                graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // Get all module resources and log in the sample. (student).
                var resources = await MicrosoftGraphSDK.Module.GetModuleResourcesAsync(graphClient, _config["classId"], module.Id.ToString());

                // Iterate over all the resources values
                foreach ( var resource in resources.Value )
                {
                    Console.WriteLine($"Resource: {resource.Resource.DisplayName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClassworkAsync: {ex.ToString()}");
                return;
            }
        }
    }
}
