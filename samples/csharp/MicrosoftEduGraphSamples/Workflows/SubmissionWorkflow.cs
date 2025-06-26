// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Models;
using Microsoft.Identity.Client;
using Microsoft.Kiota.Abstractions;
using MicrosoftEduGraphSamples.Utilities;
using MicrosoftGraphSDK;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the code samples related to Submissions, the process from assignment creation to reassignment to the student
    /// with feedback for review, including creating a batch request and getting the responses
    /// </summary>
    internal class SubmissionWorkflow
    {
        private const int MAX_RETRIES = 10;
        private readonly IConfiguration _config;

        public SubmissionWorkflow(IConfiguration configuration)
        {
            this._config = configuration;
            GlobalMethods.ValidateConfiguration(_config);
        }

        /// <summary>
        /// A code sample to show process since assignment is created until reassign the submission to the student with feedback for review
        /// </summary>
        public async Task ReassignWorkflow()
        {
            try
            {
                int retries = 0;
                string assignmentId = string.Empty;
                string submissionId = string.Empty;
                string submissionIdSelect = string.Empty;

                // Get a Graph client using delegated permissions
                var graphClientTeacherRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["teacherPassword"]);
                var graphClientStudentRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["studentPassword"]);

                // Teacher creates a new assignment
                var assignment = await Assignment.CreateSampleAssignmentAsync(graphClientTeacherRole, _config["classId"]);
                assignmentId = assignment.Id;
                Console.WriteLine($"Assignment created successfully {assignment.Id} in state {assignment.Status}");

                // Teacher publishes the assignment to make it appears in the student's list
                assignment = await GlobalMethods.PublishAssignmentsAsync(graphClientTeacherRole, assignment.Id);

                // Get the student submission using Expand outcomes
                var submissions = await Submission.GetSubmissionsWithExpandAsync(graphClientStudentRole, _config["classId"], assignmentId, "outcomes");
                if (submissions.Value.Count > 0)
                {
                    submissionId = submissions.Value[0].Id;
                    Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");
                }
                else
                {
                    throw new Exception($"No submission found for student {_config["studentAccount"]} in {assignmentId} for class {_config["classId"]}");
                }

                // Get the student submission using Select
                var submissionsSelect = await Submission.GetSubmissionsWithSelectAsync(graphClientStudentRole, _config["classId"], assignmentId, new string[] { "status", "id" });
                if (submissionsSelect.Value.Count > 0)
                {
                    submissionIdSelect = submissionsSelect.Value[0].Id;
                    Console.WriteLine($"Submission {submissionIdSelect} found for {_config["studentAccount"]}");
                }
                else
                {
                    throw new Exception($"No submission found for student {_config["studentAccount"]} in {assignmentId} for class {_config["classId"]}");
                }

                // Student submits their submission
                var submission = await Submission.SubmitAsync(graphClientStudentRole, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Id} in state {submission.Status}");

                // Check submit is completed, must reach the "Submitted" state.
                retries = 0;
                while (submission.Status != EducationSubmissionStatus.Submitted && retries <= MAX_RETRIES)
                {
                    submission = await Submission.GetSubmissionAsync(graphClientStudentRole, _config["classId"], assignmentId, submissionId);

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                // Get submission outcomes
                var submissionOutcomes = await Submission.GetSubmissionOutcomesAsync(
                    graphClientTeacherRole,
                    _config["classId"],
                    assignmentId,
                    submissionId);

                // Take the points outcome id
                var pointsOutcomeId = submissionOutcomes.Value.Where(x => x.OdataType == "#microsoft.graph.educationPointsOutcome").Select(x => x.Id).FirstOrDefault();

                // Create the points outcome body
                var pointsOutcome = new EducationPointsOutcome
                {
                    OdataType = "#microsoft.graph.educationPointsOutcome",
                    Points = new EducationAssignmentPointsGrade
                    {
                        OdataType = "#microsoft.graph.educationAssignmentPointsGrade",
                        Points = 90
                    }
                };

                // Update the submission points outcome
                var returned = await Submission.PatchOutcomeAsync(
                    graphClientTeacherRole,
                    _config["classId"],
                    assignmentId,
                    submissionId,
                    pointsOutcomeId,
                    pointsOutcome);
                Thread.Sleep(2000);
                Console.WriteLine($"Points outcome updated: {pointsOutcome.Points.Points}");

                // Teacher reassigns the submission back to the student
                submission = await Submission.ReassignAsync(graphClientTeacherRole, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Id} in state {submission.Status}");

                // Check reassign is completed, must reach the "Reassigned" state.
                retries = 0;
                while (submission.Status != EducationSubmissionStatus.Reassigned && retries <= MAX_RETRIES)
                {
                    submission = await Submission
                        .GetSubmissionWithHeaderAsync(graphClientTeacherRole, _config["classId"], assignmentId, submissionId, "Prefer", "include-unknown-enum-members");

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                Console.WriteLine($"Submission {submissionId} reached {submission.Status} state");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ReassignWorkflow: {ex.ToString()}");
            }
        }

        /// <summary>
        /// A code sample to create a batch request and get the responses
        /// </summary>
        public async Task BatchRequestWorkflow()
        {
            try
            {
                // Get a Graph client using delegated permissions
                var graphClientTeacherRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["teacherPassword"]);

                Console.WriteLine($"Getting top 20 assignments from MeAssignments Endpoint");

                // Batch is limited to 20 requests
                var meAssignments = await MicrosoftGraphSDK.User.GetMeAssignmentsWithTopAsync(graphClientTeacherRole, 20);

                // Build the batch
                var batchRequestContent = new BatchRequestContent(graphClientTeacherRole);

                Console.WriteLine($"Iterating over me assignments");
                foreach (var assignment in meAssignments.Value)
                {
                    // Use the request builder to generate a regular request to get the assignment submissions
                    var asgSubmissionsRequest = graphClientTeacherRole.Education
                                    .Classes[assignment.ClassId]
                                    .Assignments[assignment.Id]
                                    .Submissions
                                    .ToGetRequestInformation();

                    // Create HttpRequestMessage for the regular request
                    var eventsRequestMessage = await graphClientTeacherRole.RequestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(
                        asgSubmissionsRequest
                     );

                    // Adds each request to the batch
                    batchRequestContent.AddBatchRequestStep(
                        new BatchRequestStep(
                            // Use the current assignment as id for this step
                            assignment.Id,
                            // The step takes the HttpRequestMessage from the request
                            eventsRequestMessage)
                    );
                }

                // Build a return response object for our batch
                var returnedResponse = await graphClientTeacherRole.Batch.PostAsync(batchRequestContent);

                foreach (var assignment in meAssignments.Value)
                {
                    Console.WriteLine($"Getting assignment {assignment.Id} submissions");

                    // De-serialize the response based on return type
                    var submissionsResponse = await returnedResponse.GetResponseByIdAsync<EducationSubmissionCollectionResponse>(assignment.Id);

                    // Get and print submissions (if any)
                    if (submissionsResponse == null) continue;

                    // "Value" contains the request response
                    var submissions = submissionsResponse.Value;
                    foreach (var submission in submissions)
                    {
                        Console.WriteLine($"Assignment {assignment.Id}, submission: {submission.Id}, status: {submission.Status}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BatchRequestWorkflow: {ex.ToString()}");
            }
        }

        /// <summary>
        /// A code sample to create a submission feedback resource
        /// </summary>
        public async Task SubmissionFeedbackResource(bool appOnly = false)
        {
            const int MAX_RETRIES = 10;

            Console.WriteLine($"Create submission feedback resource");
            // Get a Graph client based on the appOnly parameter
            var graphClient = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["teacherPassword"]);

            //Create new assigment
            var assignment = await Assignment.CreateSampleAssignmentAsync(graphClient, _config["classId"]);
            var assignmentId = assignment.Id;
            Console.WriteLine($"Assignment created {assignmentId}");

            await Assignment.SetUpAssignmentFeedbackResourcesFolderAsync(graphClient, _config["classId"], assignmentId);
            Console.WriteLine("SetupResourceFolder creation successful");

            // Check feedback resource folder
            int retryNum = 0;
            while (assignment.FeedbackResourcesFolderUrl == null && retryNum <= MAX_RETRIES)
            {
                assignment = await Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);
                retryNum++;
            }
            Console.WriteLine($"Feedback resources folder: {assignment.FeedbackResourcesFolderUrl}");

            //Publish assignment
            assignment = await GlobalMethods.PublishAssignmentsAsync(graphClient, assignment.Id);

            //Get assignment submissions
            var submissions = await Submission.GetSubmissionsAsync(
                graphClient,
                _config["classId"],
                assignmentId);
            var submissionId = submissions.Value[0].Id;

            // Create a new submission feedback resource
            var feedbackResource = await Submission.CreateFeedbackResourceOutcomeAsync(
                graphClient,
                _config["classId"],
                assignmentId,
                submissionId);
            Thread.Sleep(2000);
            Console.WriteLine($"Feedback resource created: {feedbackResource.Id}");

            // Get submission outcomes
            var submissionOutcomes = await Submission.GetSubmissionOutcomesAsync(
                graphClient,
                _config["classId"],
                assignmentId,
                submissionId);

            // Take the points outcome id
            var pointsOutcomeId = submissionOutcomes.Value.Where(x => x.OdataType == "#microsoft.graph.educationPointsOutcome").Select(x => x.Id).FirstOrDefault();

            // Create the points outcome body
            var pointsOutcome = new EducationPointsOutcome
            {
                OdataType = "#microsoft.graph.educationPointsOutcome",
                Points = new EducationAssignmentPointsGrade
                {
                    OdataType = "#microsoft.graph.educationAssignmentPointsGrade",
                    Points = 90
                }
            };

            // Update the submission points outcome
            var returned = await Submission.PatchOutcomeAsync(
                graphClient,
                _config["classId"],
                assignmentId,
                submissionId,
                pointsOutcomeId,
                pointsOutcome);
            Thread.Sleep(2000);
            Console.WriteLine($"Points outcome updated: {pointsOutcome.Points.Points}");

            // Refresh list of submission outcomes
            submissionOutcomes = await Submission.GetSubmissionOutcomesAsync(
                graphClient,
                _config["classId"],
                assignmentId,
                submissionId);

            // Verify the new feedback resource is found
            bool resourceFound = false;
            foreach (var submissionResource in submissionOutcomes.Value)
            {
                Console.WriteLine($"Submission resource: {submissionResource.Id}");
                if (feedbackResource.Id == submissionResource.Id)
                {
                    resourceFound = true;
                    break;
                }
            }

            //Deleting the created assignment
            await Assignment.DeleteAsync(graphClient, _config["classId"], assignmentId);
            Console.WriteLine("Assignment deleted successfully " + assignmentId);
        }

        /// <summary>
        /// This sample demonstrates how to retrieve submissions that were modified within the past seven days in the class.
        /// Reference :: https://learn.microsoft.com/en-us/graph/api/educationclass-getrecentlymodifiedsubmissions?view=graph-rest-1.0&tabs=http
        /// </summary>
        /// <param name="appOnly">True value authenticates the graph client with application permissions only, otherwise it will be created with delegated permissions.</param> 
        /// <param name="numberOfAssignments">number of assignments to be created for sample purpose</param> 
        public async Task GetRecentlyModifiedSubmissions(bool appOnly = false, int numberOfAssignments = 10)
        {
            try
            {
                string submissionId = string.Empty;
                List<string> assignmentIds = new List<string>();

                // Get a Graph client based on the appOnly parameter
                var graphClientTeacherRole = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["teacherPassword"]);
                var graphClientStudentRole = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["studentPassword"]);

                for (int i = 0; i < numberOfAssignments; i++)
                {
                    // Create assignment
                    var draftAssignment = await Assignment.CreateSampleAssignmentAsync(graphClientTeacherRole, _config["classId"]);

                    // Store Assignment ID
                    assignmentIds.Add(draftAssignment.Id);
                    Console.WriteLine($"Assignment {i + 1} created successfully: ID = {draftAssignment.Id}, Status = {draftAssignment.Status}");

                    // Publishing each Assignment
                    draftAssignment = await GlobalMethods.PublishAssignmentsAsync(graphClientTeacherRole, draftAssignment.Id);
                    Console.WriteLine($"Assignment {i + 1} published successfully: ID = {draftAssignment.Id}, Status = {draftAssignment.Status}");
                    
                    // Get the student submission
                    var submissions = await Submission.GetSubmissionsAsync(graphClientStudentRole, _config["classId"], draftAssignment.Id);
                    if (submissions.Value.Count > 0)
                    {
                        submissionId = submissions.Value[0].Id;
                        Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");

                    }
                    else
                    {
                        throw new Exception($"No submission found for student {_config["studentAccount"]} in {draftAssignment.Id} for class {_config["classId"]}");
                    }

                    // Get the student submission with expand outcomes
                    var submissionsOutcome = await Submission.GetSubmissionsWithExpandAsync(graphClientStudentRole, _config["classId"], draftAssignment.Id, "outcomes");
                    if (submissionsOutcome.Value.Count > 0)
                    {
                        submissionId = submissionsOutcome.Value[0].Id;
                        Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");
                    }
                    else
                    {
                        throw new Exception($"No submission found for student {_config["studentAccount"]} in {draftAssignment.Id} for class {_config["classId"]}");
                    }

                    // Student submits their submission
                    var submission = await Submission.SubmitAsync(graphClientStudentRole, _config["classId"], draftAssignment.Id, submissionId);
                Console.WriteLine($"Submission {submission.Id} in state {submission.Status}");
                }

               // Get recentlyModifiedsubmission using orderby ascending
                var submissionsOrderbyAscending = await Submission.GetRecentlyModifiedSubmissionsWithOrderByAsync(graphClientTeacherRole, _config["classId"], "lastModifiedDateTime asc");
                Console.WriteLine("\nGetting RecentlyModifiedSubmissions with orderBy ascending Odata parameter");
                if (submissionsOrderbyAscending.Value.Count > 0)
                {
                    foreach (var individualSubmissions in submissionsOrderbyAscending.Value)
                    {
                        Console.WriteLine($"Submission ID: {individualSubmissions.Id}, Last Modified: {individualSubmissions.LastModifiedDateTime}");
                    }
                }
                else
                {
                    throw new Exception($"No submissions found when ordering by descending lastModifiedDateTime for assignment in class {_config["classId"]}.");
                }

                // Get recentlyModifiedsubmission using orderby descending
                var submissionsOrderbyDescending = await Submission.GetRecentlyModifiedSubmissionsWithOrderByAsync(graphClientTeacherRole, _config["classId"], "lastModifiedDateTime");
                Console.WriteLine("\nGetting RecentlyModifiedSubmissions with orderBy descending Odata parameter");
                if (submissionsOrderbyDescending.Value.Count > 0)
                {
                    foreach (var individualSubmissions in submissionsOrderbyDescending.Value)
                    {
                        Console.WriteLine($"Submission ID: {individualSubmissions.Id}, Last Modified DateTime: {individualSubmissions.LastModifiedDateTime}");
                    }
                }
                else
                {
                    throw new Exception($"No submissions found when ordering by descending lastModifiedDateTime for in class {_config["classId"]}.");
                }

                // Get recentlyModifiedsubmission using Top
                var submissionsTop = await Submission.GetRecentlyModifiedSubmissionsWithTopAsync(graphClientTeacherRole, _config["classId"], 2);
                Console.WriteLine("\nGetting RecentlyModifiedSubmissions with Top Odata parameter");
                if (submissionsTop.Value.Count == 2)
                {
                    foreach (var individualSubmissions in submissionsTop.Value)
                    {
                        Console.WriteLine($"Submission ID: {individualSubmissions.Id}, Last Modified DateTime: {individualSubmissions.LastModifiedDateTime}");
                    }
                }
                else
                {
                    throw new Exception($"No submissions found for given top value for in class {_config["classId"]}.");
                }

                // Get recentlyModifiedsubmission using count
                var submissionsCount = await Submission.GetRecentlyModifiedSubmissionsWithCountAsync(graphClientTeacherRole, _config["classId"], true);
                Console.WriteLine("\nGetting RecentlyModifiedSubmissions with Count Odata parameter");
                if (submissionsCount.Value.Count > 0)
                {
                    foreach (var individualSubmissions in submissionsCount.Value)
                    {
                        Console.WriteLine($"Submission ID: {individualSubmissions.Id}, Last Modified DateTime: {individualSubmissions.LastModifiedDateTime}");
                    }
                }
                else
                {
                    throw new Exception($"No submissions found for given count value in class {_config["classId"]}.");
                }

                DateTime FiveDaysAgo = DateTime.UtcNow.AddDays(-5);

                // Get recentlyModifiedsubmission using Filter
                var submissionsFilter = await Submission.GetRecentlyModifiedSubmissionsWithFilterAsync(graphClientTeacherRole, _config["classId"], $"lastModifiedDateTime gt {FiveDaysAgo.ToString("o")}");
                Console.WriteLine("\nGetting RecentlyModifiedSubmissions with Filter Odata parameter");
                if (submissionsFilter.Value.Count > 0)
                {
                    foreach (var individualSubmissions in submissionsFilter.Value)
                    {
                        Console.WriteLine($"Submission ID: {individualSubmissions.Id}, Last Modified DateTime: {individualSubmissions.LastModifiedDateTime}");
                    }
                }
                else
                {
                    throw new Exception($"No submissions found for given filter value in class {_config["classId"]}.");
                }

               //Delete Created Assigments
                Console.WriteLine("\nDeleting created assignments");
                foreach (var assignmentsId in assignmentIds)
                {
                    await Assignment.DeleteAsync(graphClientTeacherRole, _config["classId"], assignmentsId);
                    Console.WriteLine($"Assignment {assignmentsId} deleted successfully");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetRecentlyModifiedSubmissionsGetResponseAsync: {ex.ToString()}");
            }

        }

    }
}
