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
        /// Get all the assignments for the given class
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
        /// Deactivate an assignment, changes the state of an educationAssignment from its original draft status to the Inactive status
        /// Reference :: https://learn.microsoft.com/en-us/graph/assignments-states-transition
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId">User class id</param>
        /// <param name="assignmentId">Assignment id in the class</param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> DeactivateAsync(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Deactivate.PostAsync(requestConfig =>
                    {
                        requestConfig.Headers.Add(
                            "Prefer", "include-unknown-enum-members");
                    });
            }
            catch (Exception ex)
            {
                throw new GraphException($"DeactivateAsync call: {ex.Message}", ex, classId, assignmentId);
            }
        }
    }
}
