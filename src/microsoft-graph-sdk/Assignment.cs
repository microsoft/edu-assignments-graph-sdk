using Microsoft.Graph;

namespace microsoft_graph_sdk
{
    public class Assignment
    {
        public static async Task<EducationAssignment> GetAssignment(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Request()
                .GetAsync();
        }

        public static async Task<EducationAssignment> GetAssignment_WithHeader(
            GraphServiceClient client,
            string classId,
            string assignmentId,
            string headerName,
            string headerValue)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Request()
                .Header(headerName, headerValue)
                .GetAsync();
        }

        public static async Task<IEducationClassAssignmentsCollectionPage> GetAssignments(
            GraphServiceClient client,
            string classId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments
                .Request()
                .GetAsync();
        }

        public static async Task<IEducationUserAssignmentsCollectionPage> GetMeAssignments(
            GraphServiceClient client)
        {
            return await client.Education
                .Me
                .Assignments
                .Request()
                .GetAsync();
        }

        public static async Task<IEducationUserAssignmentsCollectionPage> GetUserAssignments(
            GraphServiceClient client,
            string userId)
        {
            return await client.Education
                .Users[userId]
                .Assignments
                .Request()
                .GetAsync();
        }

        public static async Task<EducationAssignment> Create(
            GraphServiceClient client,
            string classId)
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

        public static async Task<EducationAssignment> Publish(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            return await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Publish()
                .Request()
                .PostAsync();
        }

        public static async Task Delete(
            GraphServiceClient client,
            string classId,
            string assignmentId)
        {
            await client.Education
                .Classes[classId]
                .Assignments[assignmentId]
                .Request()
                .DeleteAsync();
        }
    }
}
