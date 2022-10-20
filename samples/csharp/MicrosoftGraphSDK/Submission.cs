﻿// Copyright (c) Microsoft Corporation. All rights reserved.
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
                throw new GraphException($"GetSubmission call: {ex.Message}", classId, assignmentId, submissionId);
            }
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
                throw new GraphException($"GetSubmissions call: {ex.Message}", classId, assignmentId);
            }
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
                throw new GraphException($"GetSubmission_WithHeader call: {ex.Message}", classId, assignmentId, submissionId, headerName, headerValue);
            }
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
                throw new GraphException($"Submit call: {ex.Message}", classId, assignmentId, submissionId);
            }
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
                throw new GraphException($"Reassign call: {ex.Message}", classId, assignmentId, submissionId);
            }
        }
    }
}