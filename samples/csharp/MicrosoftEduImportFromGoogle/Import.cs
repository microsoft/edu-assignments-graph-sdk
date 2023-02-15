using Azure.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftEduImportFromGoogle.Models;
using MicrosoftGraphSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<List<string>> MapAndCreateAssignments(CourseWork[] courseWorks, string classId)
        {
            List<string> assignmentsCreated = new List<string>();
            foreach(var courseWork in courseWorks)
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
            }
            return assignmentsCreated;
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
