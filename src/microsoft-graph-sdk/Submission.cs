using Microsoft.Graph;

namespace microsoft_graph_sdk
{
    public class Submission
    {
        public static async Task<EducationSubmission> GetSubmission(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions[submissionId]
                .Request()
                .GetAsync();
        }

        public static async Task<IEducationAssignmentSubmissionsCollectionPage> GetSubmissions(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions
                .Request()
                .GetAsync();
        }

        public static async Task<IEducationAssignmentSubmissionsCollectionPage> GetSubmissions_ExpandSubmittedResources(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions
                .Request()
                .Expand("submittedResources")
                .GetAsync();
        }

        public static async Task<EducationSubmission> GetSubmission_WithHeader(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId,
            string headerName,
            string headerValue)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions[submissionId]
                .Request()
                .Header(headerName, headerValue)
                .GetAsync();
        }

        public static async Task<EducationSubmission> Submit(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions[submissionId]
                .Submit()
                .Request()
                .PostAsync();
        }

        public static async Task<EducationSubmission> Reassign(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions[submissionId]
                .Reassign()
                .Request()
                .PostAsync();
        }
    }
}
