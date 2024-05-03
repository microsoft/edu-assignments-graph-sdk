// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
using MicrosoftEduGraphSamples.Utilities;
using MicrosoftGraphSDK;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains the code samples related to Assignments, include getting assignments from all classes, 
    /// checking user details for assignments, getting user classes, and excluding assignments from archived and deleted classes.
    /// </summary>
    internal class AssignmentWorkflow
    {
        private readonly IConfiguration _config;

        public AssignmentWorkflow(IConfiguration configuration)
        {
            this._config = configuration;
            GlobalMethods.ValidateConfiguration(_config);
        }

        /// <summary>
        /// A code sample to get assignments from all the classes which are not archived
        /// </summary>
        /// <param name="isTeacher">True value accepts Teacher account and false for Student account</param> 
        public async Task<IEnumerable<EducationAssignment>> GetMeAssignmentsFromNonArchivedClassesAsync(bool isTeacher = true)
        {
            try
            {
                // Check user details for assignments
                string userAccount = isTeacher ? _config["teacherAccount"] : _config["studentAccount"];

                // Get a Graph client using delegated permissions
                var graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], userAccount, _config["password"]);

                // Call to get user classes
                var joinedTeams = await graphClient.GetJoinedTeamsAsync();

                Console.WriteLine($"Getting assignments from MeAssignments Endpoint for {userAccount}");
                var meAssignments = await MicrosoftGraphSDK.User.GetMeAssignmentsAsync(graphClient);

                // Exclude assignments from archived and deleted classes
                var finalList = meAssignments.Value.Join(                 // First source
                    joinedTeams.Value.Where(t => t.IsArchived == false),  // Second source with filter applied to discard archived classes
                    assignment => assignment.ClassId,               // Key selector for me assignments
                    team => team.Id,                                // Key selector for joined teams
                    (assignment, team) => assignment);              // Expression to formulate the result

                // Iterate over all the assignments
                foreach (var assignment in finalList)
                {
                    // Print all the assignments from meAssignments
                    Console.WriteLine($"Assignment {assignment.Id} added to collection. Status: {assignment.Status} Display name: {assignment.DisplayName} ClassId: {assignment.ClassId}");
                }

                return finalList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignmentsFromNotArchivedClasses: {ex.ToString()}");
                return null;
            }
        }
        /// <summary>
        /// A code sample to get assignments evolvable enums, Evolvable enums is a mechanism that Microsoft Graph API uses to add new members to existing enumerations without causing a breaking change for applications.
        /// Reference :: https://learn.microsoft.com/en-us/graph/best-practices-concept#handling-future-members-in-evolvable-enumerations
        /// </summary>
        /// <param name></param> 
        public async Task AssignmentsEvolvableEnumsAsync()
        {
            try
            {
                string assignmentId = string.Empty;
                string submissionId = string.Empty;

                // Get a Graph client using delegated permissions
                var graphClient = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Create assignment to verify inactive state
                var assignmentInactive = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                assignmentId = assignmentInactive.Id;
                Console.WriteLine($"Assignment created successfully {assignmentInactive.Id} in state {assignmentInactive.Status}");

                // Create assignment to verify assigned state
                var assignmentAssigned = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                Console.WriteLine($"Assignment created successfully {assignmentAssigned.Id} in state {assignmentAssigned.Status}");

                // Create assignment to verify draft state
                var assignmentDraft = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                Console.WriteLine($"Assignment created successfully {assignmentDraft.Id} in state {assignmentDraft.Status}");

                // Publishing an Assignment
                assignmentInactive = await GlobalMethods.PublishAssignmentsAsync(graphClient, assignmentInactive.Id);

                // Deactivate the Assignment
                assignmentInactive = await Assignment.DeactivateAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignmentInactive.Id} Deactivated");

                // Publishing an Assignment
                assignmentAssigned = await GlobalMethods.PublishAssignmentsAsync(graphClient, assignmentAssigned.Id);
               
                // Verifying that you have an Inactive, Assigned and Draft assignments
                if (assignmentInactive.Status == EducationAssignmentStatus.Inactive)
                {
                    Console.WriteLine($"Inactive Assignment Found: {assignmentInactive.Id}");
                }

                if (assignmentAssigned.Status == EducationAssignmentStatus.Assigned)
                {
                    Console.WriteLine($"Assigned Assignment Found: {assignmentAssigned.Id}");
                }

                if (assignmentDraft.Status == EducationAssignmentStatus.Draft)
                {
                    Console.WriteLine($"Draft Assignment Found: {assignmentDraft.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignmentsEvolvableEnumsAsync: {ex.ToString()}");
            }
        }

        /// <summary>
        /// A code sample to create a new assignment, followed by an update to the assignment display name (aka title)
        /// </summary>
        /// <param name="appOnly">True value authenticates the graph client with application permissions only, otherwise it will be created with delegated permissions.</param> 
        public async Task CreateAndPatchAssignmentAsync(bool appOnly = false)
        {
            try
            {
                string assignmentId = string.Empty;

                // Get a Graph client based on the appOnly parameter
                var graphClient = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Create assignment
                var assignment = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                assignmentId = assignment.Id;
                Console.WriteLine($"Assignment created successfully {assignment.Id} in state {assignment.Status}");

                //Updating a draft assignment
                var requestBody = new EducationAssignment
                {
                    DisplayName = "Reading and review test updated",
                };
                assignment = await Assignment.PatchAsync(graphClient, _config["classId"], assignmentId, requestBody);
                
                //Verifying whether the DisplayName parameter is updated for the draft assignment.
                assignment = await Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                if(assignment.DisplayName.Contains("updated"))
                {
                    Console.WriteLine($"DisplayName updated successfully {assignment.Id} DisplayName {assignment.DisplayName}");
                }

                await Assignment.DeleteAsync(graphClient, _config["classId"], assignmentId);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateAndPatchAssignmentAsync: {ex.ToString()}");
            }
        }
 
        /// <summary>
        /// A code sample to create Assignment resources under the assignment resource folder
        /// </summary>
        /// <param name="appOnly">True value authenticates the graph client with application permissions only, otherwise it will be created with delegated permissions.</param> 
        public async Task AssignmentResourceAsync(bool appOnly = false)
        {
            try
            {
                string assignmentId = string.Empty;

                // Get a Graph client based on the appOnly parameter
                var graphClient = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Create assignment
                var assignment = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
                assignmentId = assignment.Id;
                Console.WriteLine($"Assignment created successfully with Id: {assignment.Id} in state: {assignment.Status}");

                // Set Up Assignment Resources Folder
                await Assignment.SetupResourcesFolderAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine("SetupResourceFolder creation successful");

                // Adding new Word resource to assignment
                var requestBody = new EducationAssignmentResource
                {
                    DistributeForStudentWork = false,
                    Resource = new EducationWordResource
                    {
                        OdataType = "microsoft.graph.educationWordResource",
                        DisplayName = "New Word Document.docx",                        
                    },
                };

                var resource = await Assignment.PostResourceAsync(graphClient, _config["classId"], assignmentId, requestBody);

                Console.WriteLine($"Resource created successfully with id : {resource.Id}, Display Name : {resource.Resource.DisplayName}");

                //Deleting the created assignment
                await Assignment.DeleteAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine("Assignment deleted successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignmentResourceAsync: {ex.ToString()}");
            }
        }
    }    
}
