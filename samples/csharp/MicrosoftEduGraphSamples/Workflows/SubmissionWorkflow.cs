// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Graph;

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
                if (submissions.Count > 0) // submissions.Result.Value.Count (for Beta)
                {
                    submissionId = submissions[0].Id;
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
                        .GetSubmission_WithHeaderAsync(graphClient, _config["classId"], assignmentId, submissionId, "Prefer", "include-unknown-enum-members");

                    Thread.Sleep(2000); // Wait two seconds between calls
                    retries++;
                }

                Console.WriteLine($"Submission {submissionId} reached {submission.Status} state");
            }
            catch (Exception ex) {
                Console.WriteLine($"ReassignWorkflow: {ex.ToString()}");
            }
        }
    }
}
