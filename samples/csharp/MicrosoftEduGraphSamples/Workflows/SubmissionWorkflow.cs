// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Beta.Models;
using MicrosoftEduGraphSamples.Utilities;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the workflows related to Submissions
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
        /// Workflow to show process since assignment is created until reassign the submission to the student with feedback for review
        /// </summary>
        public async Task ReassignWorkflow()
        {
            try
            {
                int retries = 0;
                string assignmentId = string.Empty;
                string submissionId = string.Empty;

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Teacher creates a new assignment
                var assignment = await MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                assignmentId = assignment.Id;
                Console.WriteLine($"Assignment created successfully {assignment.Id} in state {assignment.Status}");

                // Teacher publishes the assignment to make it appears in the student's list
                assignment = await MicrosoftGraphSDK.Assignment.PublishAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignment.Id} publish in process");

                // Verify assignment state, publish is completed until state equals "Assigned"
                while (assignment.Status != EducationAssignmentStatus.Assigned && retries <= MAX_RETRIES)
                {
                    // Print . in the log to show that the call is being retried.
                    Console.WriteLine(".");

                    assignment = await MicrosoftGraphSDK.Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                    // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay.
                    Thread.Sleep(2000);
                    retries++;
                }
                Console.WriteLine($"Assignment {assignment.Id} publish is completed. Status: {assignment.Status}");

                // Change to student account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // Get the student submission
                var submissions = await MicrosoftGraphSDK.Submission.GetSubmissionsAsync(graphClient, _config["classId"], assignmentId);
                if (submissions.Value.Count > 0)
                {
                    submissionId = submissions.Value[0].Id;
                    Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");
                }
                else
                {
                    throw new Exception($"No submission found for {_config["studentAccount"]} in {assignmentId} for class {_config["classId"]}");
                }

                // Student submits his submission
                var submission = await MicrosoftGraphSDK.Submission.SubmitAsync(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Id} in state {submission.Status}");

                // Check submit is completed, must reach the "Submitted" state.
                retries = 0;
                while (submission.Status != EducationSubmissionStatus.Submitted && retries <= MAX_RETRIES)
                {
                    submission = await MicrosoftGraphSDK.Submission.GetSubmissionAsync(graphClient, _config["classId"], assignmentId, submissionId);

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                // Change to teacher account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Teacher reassigns the submission back to the student
                submission = await MicrosoftGraphSDK.Submission.ReassignAsync(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Id} in state {submission.Status}");

                // Check reassign is completed, must reach the "Reassigned" state.
                retries = 0;
                while (submission.Status != EducationSubmissionStatus.Reassigned && retries <= MAX_RETRIES)
                {
                    submission = await MicrosoftGraphSDK.Submission
                        .GetSubmissionWithHeaderAsync(graphClient, _config["classId"], assignmentId, submissionId, "Prefer", "include-unknown-enum-members");

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
        /// Workflow to create a batch request and get the responses
        /// </summary>
        public async Task BatchRequestWorkflow()
        {
            try
            {
                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                Console.WriteLine($"Getting top 20 assignments from MeAssignments Endpoint");

                // Batch is limited to 20 requests
                var meAssignments = await MicrosoftGraphSDK.User.GetMeAssignmentsWithTopAsync(graphClient, 20);

                // Build the batch
                var batchRequestContent = new BatchRequestContent(graphClient);

                Console.WriteLine($"Iterating over me assignments");
                foreach (var assignment in meAssignments.Value)
                {
                    // Use the request builder to generate a regular request to get the assignment submissions
                    var asgSubmissionsRequest = graphClient.Education
                                    .Classes[assignment.ClassId]
                                    .Assignments[assignment.Id]
                                    .Submissions
                                    .ToGetRequestInformation();

                    // Create HttpRequestMessage for the regular request
                    var eventsRequestMessage = await graphClient.RequestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(
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
                var returnedResponse = await graphClient.Batch.PostAsync(batchRequestContent);

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
    }
}
