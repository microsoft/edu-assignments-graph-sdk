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
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignment call: {ex.Message}", classId, assignmentId);
            }
        }

        /// <summary>
        /// Get the properties and relationships of an assignment and pass a header value
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <param name="headerName"></param>
        /// <param name="headerValue"></param>
        /// <returns>EducationAssignment</returns>
        public static async Task<EducationAssignment> GetAssignment_WithHeader(
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
                    .Request()
                    .Header(headerName, headerValue)
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetAssignment_WithHeader call: {ex.Message}", classId, assignmentId, headerName, headerValue);
            }
        }

        /// <summary>
        /// Retrieve a list of assignment objects within a class
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <returns>IEducationClassAssignmentsCollectionPage</returns>
        public static async Task<IEducationClassAssignmentsCollectionPage> GetAssignments(
            GraphServiceClient client,
            string classId)
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
                throw new GraphException($"GetAssignments call: {ex.Message}", classId);
            }
        }

        /// <summary>
        /// Retrieve a list of assignment objects from current user
        /// </summary>
        /// <param name="client"></param>
        /// <returns>IEducationUserAssignmentsCollectionPage</returns>
        public static async Task<IEducationUserAssignmentsCollectionPage> GetMeAssignments(
            GraphServiceClient client)
        {
            try
            {
                return await client.Education
                    .Me
                    .Assignments
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetMeAssignments call: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieve a list of assignment objects from current user
        /// </summary>
        /// <param name="client"></param>
        /// <param name="userId"></param>
        /// <returns>IEducationUserAssignmentsCollectionPage</returns>
        public static async Task<IEducationUserAssignmentsCollectionPage> GetUserAssignments(
            GraphServiceClient client,
            string userId)
        {
            try
            {
                return await client.Education
                    .Users[userId]
                    .Assignments
                    .Request()
                    .GetAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"GetUserAssignments call: {ex.Message}", userId);
            }
        }

        /// <summary>
        /// Creates a new assignmen
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
                    .Publish()
                    .Request()
                    .PostAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"Publish call: {ex.Message}", classId, assignmentId);
            }
        }

        /// <summary>
        /// Deletes an existing assignment
        /// </summary>
        /// <param name="client"></param>
        /// <param name="classId"></param>
        /// <param name="assignmentId"></param>
        /// <returns></returns>
        public static async Task Delete(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            try
            {
                await client.Education
                    .Classes[classId]
                    .Assignments[assignmentId]
                    .Request()
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new GraphException($"Delete call: {ex.Message}", classId, assignmentId);
            }
        }
    }
}
