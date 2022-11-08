// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;
using Microsoft.Net.Http.Headers;

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
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
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
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmissionAsync call: {ex.Message}", classId, assignmentId, submissionId);
            }
        }

        /// <summary>
        /// List all the submissions associated with an assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <returns>EducationSubmissionCollectionResponse</returns>
        public static async Task<EducationSubmissionCollectionResponse> GetSubmissionsAsync(
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
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmissionsAsync call: {ex.Message}", classId, assignmentId);
            }
        }

        /// <summary>
        /// Retrieves a particular submission, can specify a header value
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
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
                    .GetAsync(requestConfiguration =>
                        requestConfiguration.Headers.Add(headerName, headerValue));
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetSubmission_WithHeaderAsync call: {ex.Message}", classId, assignmentId, submissionId, headerName, headerValue);
            }
        }

        /// <summary>
        /// Changes the status of the submission from working to submitted
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
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
                    .Submit
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"SubmitAsync call: {ex.Message}", classId, assignmentId, submissionId);
            }
        }

        /// <summary>
        /// Reassign the submission to the student
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="submissionId"></param>
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
                    .Reassign
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"ReassignAsync call: {ex.Message}", classId, assignmentId, submissionId);
            }
        }
    }
}
