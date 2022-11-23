// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for submissions
    /// </summary>
    public class Submission
    {
        /// <summary>
        /// Retrieves a particular submission
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment in the class</param>
        /// <param name="submissionId">Student submission id</param>
        /// <returns>EducationSubmission</returns>
        public static async Task<EducationSubmission> GetSubmissionAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Submissions[submissionId]
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmissionAsync call: {ex.Message}", ex, classId, assignmentId, submissionId);
            }
        }

        /// <summary>
        /// List all the submissions associated with an assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment in the class</param>
        /// <returns>IEducationAssignmentSubmissionsCollectionPage</returns>
        public static async Task<IEducationAssignmentSubmissionsCollectionPage> GetSubmissionsAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Submissions
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmissionsAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Retrieves a particular submission, can specify a header value
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment in the class</param>
        /// <param name="submissionId">Student submission id</param>
        /// <param name="headerName">Header parameter name</param>
        /// <param name="headerValue">Value for the header parameter</param>
        /// <returns>EducationSubmission</returns>
        public static async Task<EducationSubmission> GetSubmission_WithHeaderAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId,
            string headerName,
            string headerValue)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Submissions[submissionId]
                    .Request()
                    .Header(headerName, headerValue)
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmission_WithHeaderAsync call: {ex.Message}", ex, classId, assignmentId, submissionId, headerName, headerValue);
            }
        }

        /// <summary>
        /// Changes the status of the submission from working to submitted
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment in the class</param>
        /// <param name="submissionId">Student submission id</param>
        /// <returns>EducationSubmission</returns>
        public static async Task<EducationSubmission> SubmitAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Submissions[submissionId]
                    .Submit()
                    .Request()
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"SubmitAsync call: {ex.Message}", ex, classId, assignmentId, submissionId);
            }
        }

        /// <summary>
        /// Reassign the submission to the student
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment in the class</param>
        /// <param name="submissionId">Student submission id</param>
        /// <returns>EducationSubmission</returns>
        public static async Task<EducationSubmission> ReassignAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string submissionId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Submissions[submissionId]
                    .Reassign()
                    .Request()
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"ReassignAsync call: {ex.Message}", ex, classId, assignmentId, submissionId);
            }
        }
    }
}
