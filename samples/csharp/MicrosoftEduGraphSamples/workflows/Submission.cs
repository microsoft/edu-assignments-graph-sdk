﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace MicrosoftEduGraphSamples.Workflows
{
    /// <summary>
    /// Contains all the workflows related to Submissions
    /// </summary>
    internal class Submission
    {
        private const int MAX_RETRIES = 10;
        private readonly IConfiguration _config;
        public Submission(IConfiguration configuration)
        {
            this._config = configuration;
        }

        /// <summary>
        /// Workflow to show process since assignment is created until reassign the submission to the student with feedback for review
        /// </summary>
        public void ReassignWorkflow()
        {
            try
            {
                int retries = 0;
                string assignmentId = string.Empty;
                string submissionId = string.Empty;

                // Get a Graph client using delegated permissions
                var graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Teacher creates a new assignment
                var assignment = MicrosoftGraphSDK.Assignment.CreateAsync(graphClient, _config["classId"]);
                assignmentId = assignment.Result.Id;
                Console.WriteLine($"Assignment created successfully {assignment.Result.Id} in state {assignment.Result.Status}");

                // Teacher publishes the assignment to make it appears in the student's list
                assignment = MicrosoftGraphSDK.Assignment.PublishAsync(graphClient, _config["classId"], assignmentId);
                Console.WriteLine($"Assignment {assignment.Result.Id} publish in process");

                // Verify assignment state, publish is completed until state equals "Assigned"
                while (assignment.Result.Status.ToString() != "Assigned" && retries <= MAX_RETRIES)
                {
                    // Print . in the log to show that the call is being retried.
                    Console.WriteLine(".");

                    assignment = MicrosoftGraphSDK.Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                    // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay.
                    Thread.Sleep(2000);
                    retries++;
                }
                Console.WriteLine($"Assignment {assignment.Result.Id} publish is completed. Status: {assignment.Result.Status}");

                // Change to student account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // Get the student submission
                var submissions = MicrosoftGraphSDK.Submission.GetSubmissionsAsync(graphClient, _config["classId"], assignmentId);
                if (submissions.Result.Count > 0) // submissions.Result.Value.Count (for Beta)
                {
                    submissionId = submissions.Result[0].Id;
                    Console.WriteLine($"Submission {submissionId} found for {_config["studentAccount"]}");
                }
                else
                {
                    throw new Exception($"No submission found for {_config["studentAccount"]} in {assignmentId} for class {_config["classId"]}");
                }

                // Student submits his submission
                var submission = MicrosoftGraphSDK.Submission.SubmitAsync(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Result.Id} in state {submission.Result.Status}");

                // Check submit is completed, must reach the "Submitted" state.
                retries = 0;
                while (submission.Result.Status.ToString() != "Submitted" && retries <= MAX_RETRIES)
                {
                    submission = MicrosoftGraphSDK.Submission.GetSubmissionAsync(graphClient, _config["classId"], assignmentId, submissionId);

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                // Change to teacher account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Teacher reassigns the submission back to the student
                submission = MicrosoftGraphSDK.Submission.ReassignAsync(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"Submission {submission.Result.Id} in state {submission.Result.Status}");

                // Check reassign is completed, must reach the "Reassigned" state.
                retries = 0;
                while (submission.Result.Status.ToString() != "Reassigned" && retries <= MAX_RETRIES)
                {
                    submission = MicrosoftGraphSDK.Submission
                        .GetSubmission_WithHeaderAsync(graphClient, _config["classId"], assignmentId, submissionId, "Prefer", "include-unknown-enum-members");

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                Console.WriteLine($"Submission {submissionId} reached {submission.Result.Status} state");
            }
            catch (Exception ex) {
                Console.WriteLine($"ReassignWorkflow: {ex.ToString()}");
            }
        }
    }
}
