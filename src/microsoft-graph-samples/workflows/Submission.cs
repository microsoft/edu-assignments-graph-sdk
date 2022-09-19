// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace MicrosoftEduGraphSamples.workflows
{
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
            int retries = 0;

            // Get a Graph client using delegated permissions
            var graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

            // Teacher creates a new assignment
            var assig = microsoft_graph_sdk.Assignment.Create(graphClient, _config["classId"]);
            _config["assignmentId"] = assig.Result.Id;

            // Teacher publishes the assignment to make it appears in the student's list
            assig = microsoft_graph_sdk.Assignment.Publish(graphClient, _config["classId"], _config["assignmentId"]);

            // Verify assignment state, publish is completed until state equals "Assigned"
            while (assig.Result.Status.ToString() != "Assigned" && retries <= MAX_RETRIES)
            {
                assig = microsoft_graph_sdk.Assignment.GetAssignment(graphClient, _config["classId"], assig.Result.Id);

                Thread.Sleep(2000); // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time. The sleep here is just an example to emulate the delay.
                retries++;
            }

            // Change to student account
            graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

            // Get student id
            var student = microsoft_graph_sdk.User.getUserInfo(graphClient);

            // Find the student submission within the assignment
            var submissions = microsoft_graph_sdk.Submission.GetSubmissions(graphClient, _config["classId"], _config["assignmentId"]);
            foreach (var sub in submissions.Result) // use submissions.Result.Value for beta
            {
                // Break the loop when student submission is found
                if (student.Result.Id == sub.SubmittedBy.User.Id) {
                    _config["submissionId"] = sub.Id;
                    break;
                }
            }

            // Student submits his submission
            var submission = microsoft_graph_sdk.Submission.Submit(graphClient, _config["classId"], _config["assignmentId"], _config["submissionId"]);
            Console.WriteLine($"{submission.Result.Id} - {submission.Result.Status}");

            // Check submit is completed, must reach the "Submitted" state.
            retries = 0;
            while (submission.Result.Status.ToString() != "Submitted" && retries <= MAX_RETRIES) {
                submission = microsoft_graph_sdk.Submission.GetSubmission(graphClient, _config["classId"], _config["assignmentId"], submission.Result.Id);

                Thread.Sleep(2000); // Wait two seconds between calls
                retries++;
            }

            // Change to teacher account
            graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

            // Teacher reassigns the submission back to the student
            submission = microsoft_graph_sdk.Submission.Reassign(graphClient, _config["classId"], _config["assignmentId"], _config["submissionId"]);
            Console.WriteLine($"{submission.Result.Id} - {submission.Result.Status}");

            // Check reassign is completed, must reach the "Reassigned" state.
            retries = 0;
            while (submission.Result.Status.ToString() != "Reassigned" && retries <= MAX_RETRIES)
            {
                submission = microsoft_graph_sdk.Submission
                    .GetSubmission_WithHeader(graphClient, _config["classId"], _config["assignmentId"], submission.Result.Id, "Prefer", "include-unknown-enum-members");

                Thread.Sleep(2000); // Wait two seconds between calls
                retries++;
            }

            Console.WriteLine($"Final submission state is {submission.Result.Status}");
        }
    }
}
