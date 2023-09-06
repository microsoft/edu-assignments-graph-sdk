﻿using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using MicrosoftEduImportFromGoogle.Models;
using MicrosoftGraphSDK;

namespace MicrosoftEduImportFromGoogle
{
    /// <summary>
    /// Microsoft endpoints needed to import assignments, classwork modules and resources
    /// </summary>
    internal class Import
    {
        private readonly IConfiguration _config;
        public GraphServiceClient graphServiceClient;
        public Import(IConfiguration configuration)
        {
            this._config = configuration;
        }

        /// <summary>
        /// Authorizes the application and creates a Microsoft Graph client
        /// </summary>
        /// <returns>Course[]</returns>
        public async Task AuthorizeApp()
        {
            this.graphServiceClient = await MicrosoftAuthenticator.GetApplicationClient(_config["microsoftTenantId"], _config["microsoftClientId"], _config["microsoftSecret"]);
        }

        /// <summary>
        /// Returns a list of Teams that the requesting user is permitted to view
        /// </summary>
        /// <param name="userId">User id</param>
        /// <returns>List<Microsoft.Graph.Beta.Models.Team></returns>
        public List<Microsoft.Graph.Beta.Models.Team> GetUserClasses(string userId)
        {
            Console.WriteLine("* Fetching classes from Microsoft Teams...");
            return graphServiceClient
                    .Users[userId]
                    .JoinedTeams
                    .GetAsync()
                    .Result.Value.ToList();
        }

        /// <summary>
        /// Returns a list of classes that the requesting application is permitted to view
        /// </summary>
        /// <returns>List<EducationClass></returns>
        public List<EducationClass> GetClasses()
        {
            Console.WriteLine("* Fetching classes from Microsoft Teams...");
            return graphServiceClient.Education.Classes
                    .GetAsync()
                    .Result.Value.ToList();
        }

        /// <summary>
        /// Maps Google entities to create assignments
        /// </summary>
        /// <param name="courseWorks">Array of courseWorks to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        public async Task<List<string>> MapAndCreateAssignments(CourseWork[] courseWorks, string classId, Export exporterInstance)
        {
			Console.WriteLine("* Importing coursework from Google Classroom into Microsoft Teams...");
			List<string> assignmentsCreated = new List<string>();
            foreach(var courseWork in courseWorks)
            {
                var createdAssignment = await CreateAssignmentAsync(classId,
                    new EducationAssignment
                    {
                        DisplayName = courseWork.Title,
                        Instructions = new EducationItemBody { Content = courseWork.Description },
                        DueDateTime = DateTime.Now.AddDays(7), // Default due date is 7 days from today
                    }
                );
                assignmentsCreated.Add(createdAssignment.DisplayName);
                if (courseWork.Materials?.Any() == true)
                {
                    await MapAndCreateResources(courseWork.Materials, createdAssignment, exporterInstance);
                }
            }
            return assignmentsCreated;
        }

        /// <summary>
        /// Maps Google entities to create modules
        /// </summary>
        /// <param name="courseWorks">Array of courseWorkMaterials to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        public async Task<List<string>> MapAndCreateModules(CourseWorkMaterials[] courseWorkMaterials, string classId, Export exporterInstance)
        {
            Console.WriteLine("* Importing coursework materials from Google Classroom into Microsoft Teams classwork...");
            List<string> modulesCreated = new List<string>();
            foreach (var courseWork in courseWorkMaterials)
            {
                var createdModule = await CreateModuleAsync(classId,
                    new EducationModule
                    {
                        DisplayName = courseWork.Title,
                        Description = courseWork.Description
                    }
                );
                modulesCreated.Add(createdModule.DisplayName);

                if (courseWork.Materials.Any() == true)
                {
                    await MapAndCreateModuleResources(courseWork.Materials,classId, createdModule, exporterInstance);
                }
            }
            return modulesCreated;
        }

        /// <summary>
        /// Maps Google entities to create assignments resources
        /// </summary>
        /// <param name="courseWorks">List of Materials to import</param>
        /// <param name="createdAssignment">EducationAssignment</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        private async Task MapAndCreateResources(List<Material> materials, EducationAssignment createdAssignment, Export exporterInstance)
        {
            foreach (var material in materials)
            {
                string fileName = null;
                Byte[] fileAsByteArray = new byte[10000];
                if (material.DriveFile != null)
                {
                    var sourceFileMetadata = await exporterInstance.GetGoogleDriveFileMetadata(material.DriveFile.DriveFile.Id);
                    if (sourceFileMetadata["mimeType"].Contains("drawing")) // Skip Google drawing resources, not supported in Microsoft
                        continue;
                    FileTypeDetails targetFileTypeDetails = Utilities.GetFileDetails(sourceFileMetadata["mimeType"]);
                    fileAsByteArray = await exporterInstance.GetGoogleDoc(material.DriveFile.DriveFile.Id, targetFileTypeDetails.FileMimeType, !string.IsNullOrEmpty(targetFileTypeDetails.FileExtension));

                    fileName = $"{material.DriveFile.DriveFile.Title}{targetFileTypeDetails.FileExtension}";
                    if (fileName != null)
                    {
                        if (createdAssignment.ResourcesFolderUrl == null)
                        {
                            createdAssignment = await graphServiceClient.Education.Classes[createdAssignment.ClassId].Assignments[createdAssignment.Id]
                            .SetUpResourcesFolder
                            .PostAsync();
                        }
                        string uploadUrl = $"{createdAssignment.ResourcesFolderUrl}:/{fileName}:/content";
                        string[] urlSegments = createdAssignment.ResourcesFolderUrl.Split('/');
                        string driveId = urlSegments[urlSegments.Length - 3];
                        string itemId = urlSegments[urlSegments.Length - 1];
                        DriveItem driveItem;
                        using (var fileStream = new MemoryStream(fileAsByteArray))
                        {
                            driveItem = await graphServiceClient.Drives[driveId]
                            .Items[itemId]
                            .ItemWithPath(fileName)
                            .Content
                            .PutAsync(fileStream);
                        }
                        string assignmentFileUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItem.Id}";

                        EducationAssignmentResource assignmentResource = new EducationAssignmentResource() { DistributeForStudentWork = material.DriveFile.ShareMode == "STUDENT_COPY" };
                        EducationResource educationResource = GetEducationResource(sourceFileMetadata["mimeType"], assignmentFileUrl, fileName);
                        assignmentResource.Resource = educationResource;
                        await graphServiceClient.Education.Classes[createdAssignment.ClassId]
                            .Assignments[createdAssignment.Id]
                            .Resources
                            .PostAsync(assignmentResource);
                    }
                }
                else if (material.Link != null)
                {
                    EducationAssignmentResource assignmentResource = new EducationAssignmentResource() { DistributeForStudentWork = false };
                    EducationResource educationResource = new EducationLinkResource
                    {
                        Link = material.Link.Url,
                        DisplayName = material.Link.Title
                    };
                    assignmentResource.Resource = educationResource;
                    await graphServiceClient.Education.Classes[createdAssignment.ClassId]
                            .Assignments[createdAssignment.Id]
                            .Resources
                            .PostAsync(assignmentResource);
                }
                else if (material.YoutubeVideo != null)
                {
                    EducationAssignmentResource assignmentResource = new EducationAssignmentResource() { DistributeForStudentWork = false };
                    EducationResource educationResource = new EducationLinkResource
                    {
                        Link = material.YoutubeVideo.AlternateLink,
                        DisplayName = material.YoutubeVideo.Title
                    };
                    assignmentResource.Resource = educationResource;
                    await graphServiceClient.Education.Classes[createdAssignment.ClassId]
                            .Assignments[createdAssignment.Id]
                            .Resources
                            .PostAsync(assignmentResource);
                }
            }
        }

        /// <summary>
        /// Maps Google entities to create module resources
        /// </summary>
        /// <param name="courseWorks">List of Materials to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="createdModule">EducationModule</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        private async Task MapAndCreateModuleResources(List<Material> materials, string classId, EducationModule createdModule, Export exporterInstance)
        {
            foreach (var material in materials)
            {
                string fileName = null;
                Byte[] fileAsByteArray = new byte[10000];
                if (material.DriveFile != null)
                {
                    var sourceFileMetadata = await exporterInstance.GetGoogleDriveFileMetadata(material.DriveFile.DriveFile.Id);
                    if (sourceFileMetadata["mimeType"].Contains("drawing")) // Skip Google drawing resources, not supported in Microsoft
                        continue;
                    FileTypeDetails targetFileTypeDetails = Utilities.GetFileDetails(sourceFileMetadata["mimeType"]);
                    fileAsByteArray = await exporterInstance.GetGoogleDoc(material.DriveFile.DriveFile.Id, targetFileTypeDetails.FileMimeType, !string.IsNullOrEmpty(targetFileTypeDetails.FileExtension));

                    fileName = $"{material.DriveFile.DriveFile.Title}{targetFileTypeDetails.FileExtension}";
                    if (fileName != null)
                    {
                        if (createdModule.ResourcesFolderUrl == null)
                        {
                            createdModule = await graphServiceClient.Education.Classes[classId].Modules[createdModule.Id]
                            .SetUpResourcesFolder
                            .PostAsync();
                        }
                        string uploadUrl = $"{createdModule.ResourcesFolderUrl}:/{fileName}:/content";
                        string[] urlSegments = createdModule.ResourcesFolderUrl.Split('/');
                        string driveId = urlSegments[urlSegments.Length - 3];
                        string itemId = urlSegments[urlSegments.Length - 1];
                        DriveItem driveItem;
                        using (var fileStream = new MemoryStream(fileAsByteArray))
                        {
                            driveItem = await graphServiceClient.Drives[driveId]
                            .Items[itemId]
                            .ItemWithPath(fileName)
                            .Content
                            .PutAsync(fileStream);
                        }
                        string fileUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItem.Id}";

                        EducationModuleResource moduleResource = new EducationModuleResource();
                        EducationResource educationResource = GetEducationResource(sourceFileMetadata["mimeType"], fileUrl, fileName);
                        moduleResource.Resource = educationResource;
                        await graphServiceClient.Education.Classes[classId]
                            .Modules[createdModule.Id]
                            .Resources
                            .PostAsync(moduleResource);
                    }
                }
                else if (material.Link != null)
                {
                    EducationModuleResource moduleResource = new EducationModuleResource();
                    EducationResource educationResource = new EducationLinkResource
                    {
                        Link = material.Link.Url,
                        DisplayName = material.Link.Title
                    };
                    moduleResource.Resource = educationResource;
                    await graphServiceClient.Education.Classes[classId]
                            .Modules[createdModule.Id]
                            .Resources
                            .PostAsync(moduleResource);
                }
                else if (material.YoutubeVideo != null)
                {
                    EducationModuleResource moduleResource = new EducationModuleResource();
                    EducationResource educationResource = new EducationLinkResource
                    {
                        Link = material.YoutubeVideo.AlternateLink,
                        DisplayName = material.YoutubeVideo.Title
                    };
                    moduleResource.Resource = educationResource;
                    await graphServiceClient.Education.Classes[classId]
                            .Modules[createdModule.Id]
                            .Resources
                            .PostAsync(moduleResource);
                }
            }
        }

        /// <summary>
        /// Gets the proper educationResource type
        /// </summary>
        /// <param name="mimeType">Resource mime type</param>
        /// <param name="fileUrl">File url from drive</param>
        /// <param name="displayName">Display name for the resource</param>
        /// <returns>EducationResource</returns>
        private EducationResource GetEducationResource(string mimeType, string fileUrl, string displayName)
        {
            EducationResource educationResource;
            switch (mimeType)
            {
                case "application/vnd.google-apps.document":
                    educationResource = new EducationWordResource
                    {
                        FileUrl = fileUrl,
                        DisplayName = displayName,
                    };
                    break;
                case "application/vnd.google-apps.presentation":
                    educationResource = new EducationPowerPointResource
                    {
                        FileUrl = fileUrl,
                        DisplayName = displayName
                    };
                    break;
                case "application/vnd.google-apps.spreadsheet":
                    educationResource = new EducationExcelResource
                    {
                        FileUrl = fileUrl,
                        DisplayName = displayName
                    };
                    break;
                case string a when a.Contains("image/"):
                    educationResource = new EducationMediaResource
                    {
                        FileUrl = fileUrl,
                        DisplayName = displayName
                    };
                    break;
                default:
                    educationResource = new EducationFileResource
                    {
                        FileUrl = fileUrl,
                        DisplayName = displayName,
                    };
                    break;
            }
            return educationResource;
        }

        /// <summary>
        /// Creates a new assignment
        /// </summary>
        /// <param name="classId">User class id</param>
        /// <param name="educationAssignment">EducationAssignment object</param>
        /// <returns>EducationAssignment</returns>
        public async Task<EducationAssignment> CreateAssignmentAsync(
            string classId,
            EducationAssignment educationAssignment)
        {
            try
            {
                return await graphServiceClient.Education
                    .Classes[classId]
                    .Assignments
                    .PostAsync(educationAssignment, requestConfig => {
                        requestConfig.Headers.Add(
                            "Prefer", "include-unknown-enum-members");
                    });
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAssignmentAsync call: {ex.Message}", ex, classId);
            }
        }

        /// <summary>
        /// Creates a new module
        /// </summary>
        /// <param name="classId">User class id</param>
        /// <param name="educationModule">EducationModule object</param>
        /// <returns>EducationModule</returns>
        public async Task<EducationModule> CreateModuleAsync(
            string classId,
            EducationModule educationModule)
        {
            try
            {
                return await graphServiceClient.Education
                    .Classes[classId]
                    .Modules
                    .PostAsync(educationModule);
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateModuleAsync call: {ex.Message}", ex, classId);
            }
        }
    }
}
