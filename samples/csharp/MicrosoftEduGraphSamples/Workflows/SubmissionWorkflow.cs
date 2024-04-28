// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Models;
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

                // Get a Graph client using delegated permissions
                var graphClientTeacherRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);
                var graphClientStudentRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // Teacher creates a new assignment
                var assignment = await Assignment.CreateSampleAsync(graphClientTeacherRole, _config["classId"]);
                assignmentId = assignment.Id;
                Console.WriteLine($"Assignment created successfully {assignment.Id} in state {assignment.Status}");

                // Teacher publishes the assignment to make it appears in the student's list
                assignment = await GlobalMethods.PublishAssignmentsAsync(graphClientTeacherRole, assignment.Id);

                // Change to student account
                

                // Get the student submission
                var submissions = await Submission.GetSubmissionsWithExpandAsync(graphClientStudentRole, _config["classId"], assignmentId, "outcomes");
                if (submissions.Value.Count > 0)
                {
                    submissionId = submissions.Value[0].Id;
                    Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");
                }
                else
                {
                    throw new Exception($"No submission found for {_config["studentAccount"]} in {assignmentId} for class {_config["classId"]}");
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

                // Change to teacher account
                graphClientTeacherRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

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
            catch (Exception ex) {
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
                var graphClientTeacherRole = GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

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
            var graphClient = appOnly ? GraphClient.GetApplicationClient(_config["tenantId"], _config["appId"], _config["secret"]) : GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

            //Create new assigment
            var assignment = await Assignment.CreateSampleAsync(graphClient, _config["classId"]);
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
            Thread.Sleep(7000);

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
            Console.WriteLine("Assignment deleted successfully");
        }
    }
}