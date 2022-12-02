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
        public async Task<IEnumerable<EducationAssignment>> GetMeAssignmentsFromNonArchivedClassesAsync()
        {
            try
            {
                var archivedTeams = new List<string>();

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Call to get user classes
                var joinedTeams = await graphClient.GetJoinedTeamsAsync();

                // Check to iterate over all classes
                foreach (var team in joinedTeams.Where(t => t.IsArchived == true))
                {
                    // Print the current class ID and name for the assignments
                    Console.WriteLine($"Class {team.Id} Display name: {team.DisplayName}");

                    // Keep archived classes ids
                    archivedTeams.Add(team.Id);
                }

                Console.WriteLine($"Getting assignments from MeAssignments Endpoint");
                var meAssignments = await MicrosoftGraphSDK.User.GetMeAssignmentsAsync(graphClient);

                // Exclude assignments from archived classes
                var finalList = meAssignments.ExceptBy(archivedTeams, x => x.ClassId );

                // Iterate over all the assignments
                foreach (var assignment in finalList)
                {
                    // Print all the assignments from meAssignments.
                    Console.WriteLine($"Assignment {assignment.Id} added to collection. Status: {assignment.Status} Display name: {assignment.DisplayName} ClassId: {assignment.ClassId}");
                }

                return finalList;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"AssignmentsFromNotArchivedClasses: {ex.ToString()}");
                return null;
            }
        }
    }
}
