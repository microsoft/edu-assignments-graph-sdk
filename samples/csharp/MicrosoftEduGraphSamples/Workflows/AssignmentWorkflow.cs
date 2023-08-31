﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
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
        /// Workflow to get assignments from all the classes which are not archived
        /// </summary>
        /// <param name="isTeacher">True value accepts Teacher account and false for Student account</param> 
        public async Task<IEnumerable<EducationAssignment>> GetMeAssignmentsFromNonArchivedClassesAsync(bool isTeacher = true)
        {
            try
            {
                //Check user details for assignments
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
                    // Print all the assignments from meAssignments.
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
    }
}
