// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;

namespace microsoft_graph_sdk
{
    public class Submission
    {
        /// <summary>
        /// Retrieves a particular submission
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <returns>EducationSubmission</returns>
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

        /// <summary>
        /// List all the submissions associated with an assignment
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <returns>IEducationAssignmentSubmissionsCollectionPage</returns>
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

        /// <summary>
        /// List all the submissions associated with an assignment, can specify expand options
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="expand"></param>
        /// <returns>IEducationAssignmentSubmissionsCollectionPage</returns>
        public static async Task<IEducationAssignmentSubmissionsCollectionPage> GetSubmissions_Expand(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string expand)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Submissions
                .Request()
                .Expand(expand)
                .GetAsync();
        }

        /// <summary>
        /// Retrieves a particular submission, can specify a header value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns>EducationSubmission</returns>
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

        /// <summary>
        /// Changes the status of the submission from working to submitted
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <returns>EducationSubmission</returns>
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

        /// <summary>
        /// Reassign the submission to the student
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <returns>EducationSubmission</returns>
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
