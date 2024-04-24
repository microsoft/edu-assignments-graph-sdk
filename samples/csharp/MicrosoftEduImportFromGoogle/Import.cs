using Microsoft.Extensions.Configuration;
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
        /// <returns></returns>
        public void AuthorizeApp()
        {
            if (_config["microsoftAuthMethod"] == "delegated")
            {
                // App+user scenario
                this.graphServiceClient = GraphClient.GetDelegateClient(_config["microsoftClientId"]);
            }
            else {
                // App-only scenario
                this.graphServiceClient = GraphClient.GetApplicationClient(_config["microsoftTenantId"], _config["microsoftClientId"], _config["microsoftSecret"]);
            }
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
        /// Maps Google courseWorks to create assignments
        /// </summary>
        /// <param name="courseWorks">Array of courseWorks to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        public async Task<List<string>> MapCourseWorksToAssignments(CourseWork[] courseWorks, string classId, Export exporterInstance)
        {
            Console.WriteLine("* Importing coursework from Google Classroom into Microsoft Teams...");
            List<string> assignmentsCreated = new List<string>();
            foreach(var courseWork in courseWorks)
            {
                var createdAssignment = await Assignment.CreateSampleAssignmentAsync(graphServiceClient, classId,
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
                    await MapMaterialToResources(courseWork.Materials, createdAssignment, exporterInstance);
                }
            }
            return assignmentsCreated;
        }

        /// <summary>
        /// Maps Google courseWorkMaterials to create modules
        /// </summary>
        /// <param name="courseWorks">Array of courseWorkMaterials to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns>List<string></returns>
        public async Task<List<string>> MapCourseWorkMaterialsToModules(CourseWorkMaterials[] courseWorkMaterials, string classId, Export exporterInstance)
        {
            Console.WriteLine("* Importing coursework materials from Google Classroom into Microsoft Teams classwork...");
            List<string> modulesCreated = new List<string>();
            foreach (var courseWork in courseWorkMaterials)
            {
                var createdModule = await Module.CreateSampleAssignmentAsync(graphServiceClient, classId, courseWork.Title, courseWork.Description);
                modulesCreated.Add(createdModule.DisplayName);

                if (courseWork.Materials.Any() == true)
                {
                    await MapCourseWorkMaterialsToModuleResources(courseWork.Materials,classId, createdModule, exporterInstance);
                }
            }
            return modulesCreated;
        }

        /// <summary>
        /// Maps Google materials to create assignments resources
        /// </summary>
        /// <param name="courseWorks">List of Materials to import</param>
        /// <param name="createdAssignment">EducationAssignment</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns></returns>
        private async Task MapMaterialToResources(List<Material> materials, EducationAssignment createdAssignment, Export exporterInstance)
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
                            createdAssignment = await Assignment.SetupResourcesFolder(graphServiceClient, createdAssignment.ClassId, createdAssignment.Id);
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

                        await Assignment.PostResourceAsync(graphServiceClient, createdAssignment.ClassId, createdAssignment.Id, assignmentResource);
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
                    await Assignment.PostResourceAsync(graphServiceClient, createdAssignment.ClassId, createdAssignment.Id, assignmentResource);
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
                    await Assignment.PostResourceAsync(graphServiceClient, createdAssignment.ClassId, createdAssignment.Id, assignmentResource);
                }
            }
        }

        /// <summary>
        /// Maps Google courseWork materials to create module resources
        /// </summary>
        /// <param name="courseWorks">List of Materials to import</param>
        /// <param name="classId">User class id</param>
        /// <param name="createdModule">EducationModule</param>
        /// <param name="exporterInstance">Instance of the Export class</param>
        /// <returns></returns>
        private async Task MapCourseWorkMaterialsToModuleResources(List<Material> materials, string classId, EducationModule createdModule, Export exporterInstance)
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
                            createdModule = await Module.SetupResourcesFolder(graphServiceClient, classId, createdModule.Id);
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
                        await Module.PostResourceAsync(graphServiceClient, classId, createdModule.Id, moduleResource);
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
                    await Module.PostResourceAsync(graphServiceClient, classId, createdModule.Id, moduleResource);
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
                    await Module.PostResourceAsync(graphServiceClient, classId, createdModule.Id, moduleResource);
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
    }
}
