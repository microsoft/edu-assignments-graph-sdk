using Microsoft.Extensions.Configuration;

namespace microsoft_graph_samples.workflows
{
    internal class submission_reassign
    {
        private readonly IConfiguration _config;

        public submission_reassign(IConfiguration configuration)
        {
            this._config = configuration;
        }

        public void workflow()
        {
            // Get a Graph client using delegated permissions
            var graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

            // Teacher creates a new assignment
            var assig = microsoft_graph_sdk.Assignment.Create(graphClient, _config["classId"]);
            _config["assignmentId"] = assig.Result.Id;

            // Teacher publishes the assignment to make it appears in the student's list
            assig = microsoft_graph_sdk.Assignment.Publish(graphClient, _config["classId"], _config["assignmentId"]);

            // Verify assignment state, publish is completed until state equals "Assigned"
            while (assig.Result.Status.ToString() != "Assigned")
            {
                assig = microsoft_graph_sdk.Assignment.GetAssignment(graphClient, _config["classId"], assig.Result.Id);

                Thread.Sleep(2000); // Wait two seconds between calls
            }

            // Change to student account
            graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["studentAccount"], _config["password"]);

            // Get student id
            var student = microsoft_graph_sdk.User.getUserInfo(graphClient);

            // Find the student submission within the assignment
            var submissions = microsoft_graph_sdk.Submission.GetSubmissions(graphClient, _config["classId"], _config["assignmentId"]);
            foreach (var sub in submissions.Result)
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
            while (submission.Result.Status.ToString() != "Submitted") {
                submission = microsoft_graph_sdk.Submission.GetSubmission(graphClient, _config["classId"], _config["assignmentId"], submission.Result.Id);

                Thread.Sleep(2000); // Wait two seconds between calls
            }

            // Change to teacher account
            graphClient = microsoft_graph_sdk.GraphClient.GetDelegateClient(_config["tenantId"], _config["appId"], _config["teacherAccount"], _config["password"]);

            // Teacher reassigns the submission back to the student
            submission = microsoft_graph_sdk.Submission.Reassign(graphClient, _config["classId"], _config["assignmentId"], _config["submissionId"]);
            Console.WriteLine($"{submission.Result.Id} - {submission.Result.Status}");

            // Check reassign is completed, must reach the "Reassigned" state.
            while (submission.Result.Status.ToString() != "Reassigned")
            {
                submission = microsoft_graph_sdk.Submission
                    .GetSubmission_WithHeader(graphClient, _config["classId"], _config["assignmentId"], submission.Result.Id, "Prefer", "include-unknown-enum-members");

                Thread.Sleep(2000); // Wait two seconds between calls
            }

            Console.WriteLine($"Final submission state is ${submission.Result.Status}");
        }
    }
}
