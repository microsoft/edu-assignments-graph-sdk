﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace MicrosoftEduGraphSamples.workflows
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
                var assig = MicrosoftGraphSDK.Assignment.Create(graphClient, _config["classId"]);
                assignmentId = assig.Result.Id;

                // Teacher publishes the assignment to make it appears in the student's list
                assig = MicrosoftGraphSDK.Assignment.Publish(graphClient, _config["classId"], assignmentId);

                // Verify assignment state, publish is completed until state equals "Assigned"
                while (assig.Result.Status.ToString() != "Assigned" && retries <= MAX_RETRIES)
                {
                    assig = MicrosoftGraphSDK.Assignment.GetAssignment(graphClient, _config["classId"], assignmentId);

                    Thread.Sleep(2000); // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay.
                    retries++;
                }

                // Change to student account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

                // Get student id
                var student = MicrosoftGraphSDK.User.getUserInfo(graphClient);

                // Find the student submission within the assignment
                var submissions = MicrosoftGraphSDK.Submission.GetSubmissions(graphClient, _config["classId"], assignmentId);
                foreach (var sub in submissions.Result) // use submissions.Result.Value for beta
                {
                    // Break the loop when student submission is found
                    if (student.Result.Id == sub.SubmittedBy.User.Id)
                    {
                        submissionId = sub.Id;
                        break;
                    }
                }

                // Student submits his submission
                var submission = MicrosoftGraphSDK.Submission.Submit(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"{submission.Result.Id} - {submission.Result.Status}");

                // Check submit is completed, must reach the "Submitted" state.
                retries = 0;
                while (submission.Result.Status.ToString() != "Submitted" && retries <= MAX_RETRIES)
                {
                    submission = MicrosoftGraphSDK.Submission.GetSubmission(graphClient, _config["classId"], assignmentId, submissionId);

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                // Change to teacher account
                graphClient = MicrosoftGraphSDK.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

                // Teacher reassigns the submission back to the student
                submission = MicrosoftGraphSDK.Submission.Reassign(graphClient, _config["classId"], assignmentId, submissionId);
                Console.WriteLine($"{submission.Result.Id} - {submission.Result.Status}");

                // Check reassign is completed, must reach the "Reassigned" state.
                retries = 0;
                while (submission.Result.Status.ToString() != "Reassigned" && retries <= MAX_RETRIES)
                {
                    submission = MicrosoftGraphSDK.Submission
                        .GetSubmission_WithHeader(graphClient, _config["classId"], assignmentId, submissionId, "Prefer", "include-unknown-enum-members");

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                Console.WriteLine($"Final submission state is {submission.Result.Status}");
            }
            catch (Exception ex) {
                Console.WriteLine($"ReassignWorkflow: {ex.ToString()}");
            }
        }
    }
}