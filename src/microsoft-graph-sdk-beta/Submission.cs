using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

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
                .GetAsync();
        }

        public static async Task<EducationSubmissionCollectionResponse> GetSubmissions(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions
                .GetAsync();
        }

        public static async Task<EducationSubmissionCollectionResponse> GetSubmissions_Expand(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string expand)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions
                // TODO: how to pass expand options?
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
                .GetAsync(requestConfiguration => 
                    requestConfiguration.Headers.Add(headerName, headerValue));
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
                .Submit
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
                .Reassign
                .PostAsync();
        }
    }
}
