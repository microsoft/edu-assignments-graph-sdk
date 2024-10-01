using Microsoft.Extensions.Configuration;
using Microsoft.Graph.Beta.Models;
using Microsoft.Graph.Beta;

namespace MicrosoftEduGraphSamples.Utilities
{
    /// <summary>
    /// Contains all the reusuable methods for code samples related to Assignments,Modules and Submissions.
    /// </summary>
    internal class GlobalMethods
    {
        private static IConfiguration _config;
        private const int MAX_RETRIES = 10;

        /// <summary>
        ///Verifies and throw exception for input values if null or empty
        /// </summary>
        public static void ValidateConfiguration(IConfiguration configuration)
        {
            _config = configuration;

            // Verify and throw exception for input values if null or empty
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
            else if (string.IsNullOrEmpty(_config["teacherpassword"]))
            {
                throw new Exception("Missing teacher password please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["studentpassword"]))
            {
                throw new Exception("Missing student password please check appconfig.json file.");
            }
        }

        /// <summary>
        /// A code sample to Publish Assignments
        /// </summary>
        /// <param name="assignmentId">Assignment id</param>
        public static async Task<EducationAssignment> PublishAssignmentsAsync(GraphServiceClient graphClient, string assignmentId)
        {
            int retries = 0;

            // Teacher publishes the assignment to make it appears in the student's list
            var assignment = await MicrosoftGraphSDK.Assignment.PublishAsync(graphClient, _config["classId"], assignmentId);
            Console.WriteLine($"Assignment {assignment.Id} publish in process");

            // Verify assignment state, publish is not completed until state equals "Assigned"
            while (assignment.Status != EducationAssignmentStatus.Assigned && retries <= MAX_RETRIES)
            {
                // Print . in the log to show that the call is being retried
                Console.WriteLine(".");

                assignment = await MicrosoftGraphSDK.Assignment.GetAssignmentAsync(graphClient, _config["classId"], assignmentId);

                // If you are calling this code pattern in Backend agent of your service, then you want to retry the work after some time using a retry policy, such as linear or exponential. The sleep here is just an example to emulate the delay.
                Thread.Sleep(2000);
                retries++;
            }
            Console.WriteLine($"Assignment {assignment.Id} publish is completed. Status: {assignment.Status}");
            return assignment;
        }
    }
}
