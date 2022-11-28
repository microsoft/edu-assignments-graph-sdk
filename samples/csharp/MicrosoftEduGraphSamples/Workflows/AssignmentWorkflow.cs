// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftGraphSDK;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the workflows related to Assignments
    /// </summary>
    internal class AssignmentWorkflow
    {
        private readonly IConfiguration _config;

        public AssignmentWorkflow(IConfiguration configuration)
        {
            this._config = configuration;
        }

        /// <summary>
        /// Workflow to get assignments from all the classes which are not archived
        /// </summary>
        public async Task PrintNotArchivedClassesAssignmentsAsync()
        {
            try
            {
                List<EducationAssignment> assignmentsFromNonArchivedClasses = new List<EducationAssignment>();

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                //Call to get user classes
                var joinedTeams = await graphClient.GetJoinedTeamsAsync();

                //Check to iterate over all classes
                foreach(var team in joinedTeams.Where(t => t.IsArchived == false))
                {
                    // Print the current class ID and name for the assignments
                    Console.WriteLine($"Class {team.Id} Display name: {team.DisplayName}");

                    // Call to Get Assignments using the current classId
                    var assignments = await MicrosoftGraphSDK.Assignment.GetAssignmentsAsync(graphClient, team.Id);

                    // Iterate over all the assignments from that class
                    foreach (var assignment in assignments)
                    {
                        // Call to add the remaining not archived assignments into a collection
                        assignmentsFromNonArchivedClasses.Add(assignment);
                    }
                }

                Console.WriteLine($"Getting assignments from MeAssignments Endpoint");
                var meAssignments = await MicrosoftGraphSDK.User.GetMeAssignmentsAsync(graphClient);

                //Join meAssignments with assignmentsFromNonArchivedClasses excluding repeated assignments
                var finalList = assignmentsFromNonArchivedClasses.Union(meAssignments);

                //Iterate over all the assignments
                foreach (var assignment in finalList)
                {
                    // Print all the assignments from meAssignments.
                    Console.WriteLine($"Assignment {assignment.Id} added to collection. Status: {assignment.Status} Display name: {assignment.DisplayName}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"AssignmentsFromNotArchivedClasses: {ex.ToString()}");
            }
        }
    }
}
