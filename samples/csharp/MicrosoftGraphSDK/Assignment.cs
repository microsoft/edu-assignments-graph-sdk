// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;

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
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
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
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignmentAsync call: {ex.Message}", classId, assignmentId);
            }
        }

        /// <summary>
        /// Get all the assignments from the class
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <returns>IEducationClassAssignmentsCollectionPage</returns>
        public static async Task<IEducationClassAssignmentsCollectionPage> GetAssignmentsAsync(
            GraphServiceClient client,
            string classId
            )
        {
            try
            {
                return await client.Education
                    .Classes[classId]
                    .Assignments
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignmentsAsync call: {ex.Message}", classId);
            }
        }

        /// <summary>
        /// Creates a new assignment
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
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
                    .Request()
                    .Header("Prefer", "include-unknown-enum-members")
                    .AddAsync(assignment);
            }
            catch (Exception ex)
            {
                throw new GraphException($"CreateAsync call: {ex.Message}", classId);
            }
        }

        /// <summary>
        /// Publishes an assignment, changes the state of an educationAssignment from its original draft status to the published status
        /// </summary>
        /// <param name="client">Microsoft Graph service client</param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
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
                    .Publish()
                    .Request()
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"PublishAsync call: {ex.Message}", classId, assignmentId);
            }
        }
    }
}
