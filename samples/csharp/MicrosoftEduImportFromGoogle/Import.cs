using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.SecurityNamespace;
using Microsoft.IdentityModel.Tokens;
using MicrosoftEduImportFromGoogle.Models;
using MicrosoftGraphSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace MicrosoftEduImportFromGoogle
{
    internal class Import
    {
        private readonly IConfiguration _config;
        public GraphServiceClient graphServiceClient;
        public Import(IConfiguration configuration)
        {
            this._config = configuration;
        }
        public async Task AuthorizeApp()
        {
            this.graphServiceClient = await MicrosoftAuthenticator.InitializeMicrosoftGraphClient(_config["microsoftClientId"]);

        }

        public List<EducationClass> GetMeClasses()
        {
            return graphServiceClient.Education.Me.Classes
                    .Request()
                    .GetAsync()
                    .Result.ToList();

        }

        public async Task<List<string>> MapAndCreateAssignments(CourseWork[] courseWorks, string classId, Export exporterInstance)
        {
            List<string> assignmentsCreated = new List<string>();
            foreach (var courseWork in courseWorks)
            {
                var createdAssignment = await CreateAssignmentAsync(classId,
                    new EducationAssignment
                    {
                        DisplayName = courseWork.Title,
                        Instructions = new EducationItemBody { Content = courseWork.Description },
                        DueDateTime = DateTime.Now.AddDays(7),//revisit
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
        

        private async Task MapAndCreateResources(List<Material> materials, EducationAssignment createdAssignment, Export exporterInstance)
        {
            foreach (var material in materials)
            {
                string fileName = null;
                Byte[] fileAsByteArray = new byte[10000];
                if (material.DriveFile != null)
                {
                    var sourceFileMetadata = await exporterInstance.GetGoogleDriveFileMetadata(material.DriveFile.DriveFile.Id);
                    FileTypeDetails targetFileTypeDetails = Utilities.GetFileDetails(sourceFileMetadata["mimeType"]);
                    fileAsByteArray = await exporterInstance.GetGoogleDoc(material.DriveFile.DriveFile.Id, targetFileTypeDetails.FileMimeType, !string.IsNullOrEmpty(targetFileTypeDetails.FileExtension));
                    
                    fileName = $"{material.DriveFile.DriveFile.Title}{targetFileTypeDetails.FileExtension}";
                    if (fileName != null)
                    {
                        if(createdAssignment.ResourcesFolderUrl == null)
                        {
                            createdAssignment = await graphServiceClient.Education.Classes[createdAssignment.ClassId].Assignments[createdAssignment.Id]
                            .SetUpResourcesFolder()
                            .Request()
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
                            .Request()
                            .PutAsync<DriveItem>(fileStream);
                        }
                        string assignmentFileUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{driveItem.Id}";
                        
                        EducationAssignmentResource assignmentResource = new EducationAssignmentResource() { DistributeForStudentWork = material.DriveFile.ShareMode == "STUDENT_COPY" };
                        EducationResource educationResource = GetEducationResource(sourceFileMetadata["mimeType"], assignmentFileUrl, fileName);
                        assignmentResource.Resource = educationResource;
                        await graphServiceClient.Education.Classes[createdAssignment.ClassId]
                            .Assignments[createdAssignment.Id]
                            .Resources
                            .Request()
                            .AddAsync(assignmentResource);
                    }
                }
                else if(material.Link != null)
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
                            .Request()
                            .AddAsync(assignmentResource);
                }
                else if(material.YoutubeVideo!= null)
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
                            .Request()
                            .AddAsync(assignmentResource);
                }
            }
        }
        private EducationResource GetEducationResource(string mimeType, string fileUrl, string displayName)
        {
            EducationResource educationResource;
            switch(mimeType)
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

        public async Task<EducationAssignment> CreateAssignmentAsync(
            string classId,
            EducationAssignment educationAssignment)
        
        {
            try
            {
                return await graphServiceClient.Education
                    .Classes[classId]
                    .Assignments
                    .Request()
                    .Header("Prefer", "include-unknown-enum-members")
                    .AddAsync(educationAssignment);
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAsync call: {ex.Message}", ex, classId);
            }
        }
    }
}
