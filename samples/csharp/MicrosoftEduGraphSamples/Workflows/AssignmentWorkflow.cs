// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
using MicrosoftEduGraphSamples.Utilities;
using MicrosoftGraphSDK;
using Microsoft.Graph.Beta;


namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the workflows related to Assignments, include getting assignments from all classes, 
    /// checking user details for assignments, getting user classes, and excluding assignments from archived and deleted classes.
    /// </summary>
    internal class AssignmentWorkflow
    {
        private readonly IConfiguration _config;
        private const int MAX_RETRIES = 10; // Maximum number of retries for long running operations
        
        public AssignmentWorkflow(IConfiguration configuration)
        {
            this._config = configuration;
            GlobalMethods.ValidateConfiguration(_config);
        }

        /// <summary>
        /// Workflow to get assignments from all the classes which are not archived
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
        /// Workflow to get assignments evolvable enums
        /// </summary>
        /// <param name></param> 
        public async Task AssignmentsEvolvableEnums()
        {
            try
            {
                int retries = 0;
                string assignmentId = string.Empty;
                string submissionId = string.Empty;

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Create a draft assignment with displayName = "Verify assignments states [inactive]"
                var assignmentInactive = await MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                assignmentId = assignmentInactive.Id;
                Console.WriteLine($"Assignment created successfully {assignmentInactive.Id} in state {assignmentInactive.Status}");

                // Create a draft assignment with displayName = "Verify assignments states [assigned]"
                var assignmentAssigned = await MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                Console.WriteLine($"Assignment created successfully {assignmentAssigned.Id} in state {assignmentAssigned.Status}");

                // Create a draft assignment with displayName = "Verify assignments states [draft]"
                var assignmentDraft = await MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                Console.WriteLine($"Assignment created successfully {assignmentDraft.Id} in state {assignmentDraft.Status}");

                // Teacher publishes the assignment to make it appears in the student's list
                assignmentInactive = await MicrosoftGraphSDK.Assignment.PublishAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignmentInactive.Id} publish in process");

                // Verify assignment state, publish is completed until state equals "Assigned"
                while (assignmentInactive.Status != EducationAssignmentStatus.Assigned && retries <= MAX_RETRIES)
                {
                    // Print . in the log to show that the call is being retried
                    Console.WriteLine(".");

                    assignmentInactive = await MicrosoftGraphSDK.Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                    // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay
                    Thread.Sleep(2000);
                    retries++;
                }
                Console.WriteLine($"Assignment {assignmentInactive.Id} publish is completed. Status: {assignmentInactive.Status}");

                // Deactivate the Assignment
                assignmentInactive = await MicrosoftGraphSDK.Assignment.DeactivateAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignmentInactive.Id} Deactivated");

                // Publishing an Assignment
                assignmentId = assignmentAssigned.Id;
                assignmentAssigned = await MicrosoftGraphSDK.Assignment.PublishAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignmentAssigned.Id} publish in process");

                // Verify assignment state, publish is completed until state equals "Assigned"
                while (assignmentAssigned.Status != EducationAssignmentStatus.Assigned && retries <= MAX_RETRIES)
                {
                    // Print . in the log to show that the call is being retried
                    Console.WriteLine(".");

                    assignmentAssigned = await MicrosoftGraphSDK.Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                    // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay
                    Thread.Sleep(2000);
                    retries++;
                }
                Console.WriteLine($"Assignment {assignmentAssigned.Id} publish is completed. Status: {assignmentAssigned.Status}");

                // Verifying that you have an Inactive, Assigned and Draft assignments
                if (assignmentInactive.Status == EducationAssignmentStatus.Inactive)
                {
                    Console.WriteLine($"Inactive Assignment Found: {assignmentId}");
                }

                if (assignmentAssigned.Status == EducationAssignmentStatus.Assigned)
                {
                    Console.WriteLine($"Assigned Assignment Found: {assignmentId}");
                }

                if (assignmentDraft.Status == EducationAssignmentStatus.Draft)
                {
                    Console.WriteLine($"Draft Assignment Found: {assignmentId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignmentsEvolvableEnums: {ex.ToString()}");
            }
        }
    }
}
