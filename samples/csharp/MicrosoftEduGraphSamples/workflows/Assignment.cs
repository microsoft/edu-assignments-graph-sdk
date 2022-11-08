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
        public void AssignmentsFromNotArchivedClasses ()
        {
            try
            {
                List<EducationAssignment> assignmentsFromNonArchivedClasses = new List<EducationAssignment>();

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                //Call to get user classes
                var joinedTeams = graphClient.GetJoinedTeamsAsync();

                //Check to iterate over all classes
                foreach(var team in joinedTeams.Result) 
                {
                    // Verify if isArchived property = false
                    if (team.IsArchived == false)
                    {
                        // Print the current class ID and name for the assignments
                        Console.WriteLine($"Class {team.Id} Display name: {team.DisplayName}");

                        // Call to Get Assignments using the current classId
                        var assignments = MicrosoftGraphSDK.Assignment.GetAssignmentsAsync(graphClient, team.Id);

                       // Iterate over all the assignments from that class
                        foreach (var assignment in assignments.Result)
                        {
                            // Call to add the remaining not archived assignments into a collection
                            assignmentsFromNonArchivedClasses.Add(assignment);

                            // Print all the assignments from no archived classes.
                            Console.WriteLine($"Assignment {assignment.Id} added to collection. Status: {assignment.Status} Display name: {assignment.DisplayName}");
                        }
                        Console.WriteLine($"Getting assignments from MeAssignments Endpoint");

                        //Call to Me assignments endpoint
                        var meAssignments = MicrosoftGraphSDK.User.GetMeAssignmentsAsync(graphClient);
                        
                        //Iterate over all the assignments
                        foreach (var assignment in meAssignments.Result)
                        {
                            //Add assignment to the list
                            assignmentsFromNonArchivedClasses.Add(assignment);

                            // Print all the assignments from meAssignments.
                            Console.WriteLine($"Assignment {assignment.Id} added to collection. Status: {assignment.Status} Display name: {assignment.DisplayName}");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"AssignmentsFromNotArchivedClasses: {ex.ToString()}");
            }
        }
    }
}
