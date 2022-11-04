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
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> GetAssignment(
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
                throw new GraphException($"GetAssignment call: {ex.Message}", classId, assignmentId);
            }
        }

        /// <summary>
        /// Get all the assignments from the class
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <returns>EducationAssignmentCollectionResponse</returns>
        public static async Task<EducationAssignmentCollectionResponse> GetAssignments(
            GraphServiceClient client,
            string classId
            )
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
                throw new GraphException($"GetAssignments call: {ex.Message}", classId);
            }
        }

        /// <summary>
        /// Creates a new assignment
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> Create(
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
                        Content = "Assignment created through Graph SDK beta"
                    },
                    Grading = new EducationAssignmentPointsGradeType
                    {
                        MaxPoints = 50f
                    },
                    AssignTo = new EducationAssignmentClassRecipient
                    {
                    },
                    Status = EducationAssignmentStatus.Draft,
                    AllowStudentsToAddResourcesToSubmission = true,
                    AddToCalendarAction = EducationAddToCalendarOptions.StudentsOnly
                };

                return await client.Education
                    .Classes[classId]
                    .Assignments
                    .PostAsync(
                        assignment,
                        requestConfiguration => requestConfiguration.Headers.Add("Prefer", "include-unknown-enum-members"));
            }
            catch (Exception ex)
            {
                throw new GraphException($"Create call: {ex.Message}", classId);
            }

        }

        /// <summary>
        /// Publishes an assignment, changes the state of an educationAssignment from its original draft status to the published status
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> Publish(
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
                throw new GraphException($"Publish call: {ex.Message}", classId, assignmentId);
            }
        }
    }
}
