// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph.Beta;
using Microsoft.Graph.Beta.Models;

namespace MicrosoftGraphSDK
{
    /// <summary>
    /// Graph SDK endpoints for assignments
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// Get the properties and relationships of an assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id in the class</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> GetAssignmentAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignmentAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Get the properties and relationships of an assignment, can specify header value
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id in the class</param>
        /// <param name="headerName">Header parameter name</param>
        /// <param name="headerValue">Value for the header parameter</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> GetAssignmentWithHeadersAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string headerName,
            string headerValue)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .GetAsync(requestConfig =>
                    {
                        requestConfig.Headers.Add(
                            headerName, headerValue);
                    });
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignmentWithHeadersAsync call: {ex.Message}", ex, classId, assignmentId, headerName, headerValue);
            }
        }

        /// <summary>
        /// Get all the assignments from the class
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <returns>EducationAssignmentCollectionResponse</returns>
        public static async Task<EducationAssignmentCollectionResponse> GetAssignmentsAsync(
            GraphServiceClient client,
            string classId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignmentsAsync call: {ex.Message}", ex, classId);
            }
        }

        /// <summary>
        /// Creates a new assignment
        /// </summary>
        /// <param name="classId">User class id</param>
        /// <param name="educationAssignment">EducationAssignment object</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> CreateAsync(
            GraphServiceClient client,
            string classId,
            EducationAssignment educationAssignment)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments
                    .PostAsync(educationAssignment);
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAsync call: {ex.Message}", ex, classId);
            }
        }

        /// <summary>
        /// Creates a new assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> CreateAsync(
            GraphServiceClient client,
            string classId)
        {
            try
            {
                var assignment = new EducationAssignment
                {
                    DueDateTime = DateTimeOffset.Parse(DateTime.Now.AddDays(10).ToString()),
                    DisplayName = $"Graph SDK assignment {DateTime.Now.ToString("dd/MM/yyyy HHmm")}",
                    Instructions = new EducationItemBody
                    {
                        ContentType = BodyType.Text,
                        Content = "Assignment created through Graph SDK"
                    },
                    Grading = new EducationAssignmentPointsGradeType
                    {
                        OdataType = "#microsoft.graph.educationAssignmentPointsGradeType",
                        MaxPoints = 50f
                    },
                    AssignTo = new EducationAssignmentClassRecipient
                    {
                        OdataType = "#microsoft.graph.educationAssignmentClassRecipient"
                    },
                    Status = EducationAssignmentStatus.Draft,
                    AllowStudentsToAddResourcesToSubmission = true,
                    AddToCalendarAction = EducationAddToCalendarOptions.StudentsOnly
                };

                return await client.Education
                    .Classes[classId]
                    .Assignments
                    .PostAsync(assignment, requestConfig => {
                            requestConfig.Headers.Add(
                                "Prefer", "include-unknown-enum-members");
                        });
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAsync call: {ex.Message}", ex, classId);
            }
        }

        /// <summary>
        /// Publishes an assignment, changes the state of an educationAssignment from its original draft status to the published status
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id in the class</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> PublishAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Publish
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"PublishAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Post an assignment resource
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id</param>
        /// <returns>EducationAssignmentResource</returns>
        public static async Task<EducationAssignmentResource> PostResourceAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            EducationAssignmentResource resource)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Resources.PostAsync(resource);
            }
            catch (Exception ex)
            {
                throw new GraphException($"PostResourceAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Sets up the assignment resources folder
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> SetupResourcesFolder(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .SetUpResourcesFolder
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"SetupResourcesFolder call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Creates a SharePoint folder to upload feedback resources
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> SetupFeedbackResourcesFolderAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .SetUpFeedbackResourcesFolder
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"SetupFeedbackResourcesFolderAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }

        /// <summary>
        /// Deletes an existing assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id</param>
        /// <returns></returns>
        public static async Task DeleteAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"DeleteAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }
    }
}
