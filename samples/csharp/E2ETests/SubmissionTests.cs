using Microsoft.Graph.Beta;
using NUnit.Framework;
using FluentAssertions;
using log4net;
using MicrosoftGraphSDK;
using System.Reflection;
using Microsoft.Graph.Beta.Models;

namespace E2ETests
{
    public class SubmissionTests
    {
        private GraphServiceClient client;
        Configuration config = new Configuration();
        private readonly ILog _log;

        public SubmissionTests()
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        [OneTimeSetUp]
        public void GetClient()
        {
            _log.Info("Get Delegated client");
            client = GraphClient.GetDelegateClient(config._tenantId, config._appId, config._teacherAccount, config._password);
        }

        [Test]
        public async Task GetSubmissionAsync()
        {
            var classId = config._classId;
            // Pick a randomn assignment
            var assignments = await Assignment.GetAssignmentsAsync(client, config._classId);
            var assignmentId = assignments.Value[0].Id;

            // Pick a random submission from the assignment
            var submissions = await Submission.GetSubmissionsAsync(client, config._classId, assignmentId);
            var submissionId = submissions.Value[0].Id;

            // Call get submission
            _log.Info($"Getting submission({submissionId}) from class({classId}) - assignment({assignmentId})");
            var submission = await Submission.GetSubmissionAsync(
                client,
                classId,
                assignmentId,
                submissionId);

            // Submission properties
            List<string> expectedProperties = new List<string>() {
                "id",
                "reassignedby",
                "reassigneddatetime",
                "recipient",
                "resourcesfolderurl",
                "returnedby",
                "returneddatetime",
                "status",
                "submittedby",
                "submitteddatetime",
                "unsubmittedby",
                "unsubmitteddatetime",
                "weburl"};

            Type submissionType = submission.GetType();
            IList<PropertyInfo> properties = new List<PropertyInfo>(submissionType.GetProperties());

            foreach (PropertyInfo property in properties)
            {
                if (expectedProperties.Contains(property.Name.ToLower()))
                {
                    _log.Info($"Property ( {property.Name} : {property.GetValue(submission, null)}) was found.");
                    property.Name.Should().NotBeNull($"Property {property.Name} was found.");
                }
            }

            submission.Id.Should().NotBeNull("because submission id is valid.");
        }

        [Test]
        public async Task GetSubmissionsAsync()
        {
            var classId = config._classId;

            // Pick a randomn assignment
            var assignments = await Assignment.GetAssignmentsAsync(client, config._classId);
            var assignmentId = assignments.Value[0].Id;

            _log.Info($"Getting submissions from class({classId}) - assignment({assignmentId})");
            var submissions = await Submission.GetSubmissionsAsync(
                client,
                classId,
                assignmentId);

            submissions.Should().NotBeNull("because the assignment always have at least one submission.");
        }

        [Test]
        public async Task ReassignSubmission()
        {
            var classId = config._classId;

            _log.Info($"Reassign submission process");

            //Create new assigment
            var assignment = await Assignment.CreateAsync(
                client,
                classId);
            var assignmentId = assignment.Id;

            //Publish assignment
            assignment = await Assignment.PublishAsync(
                client,
                classId,
                assignmentId);
            Thread.Sleep(7000);

            //Change to Student account
            client = GraphClient.GetDelegateClient(config._tenantId, config._appId, config._studentAccount, config._password);

            //Get assignment submissions
            var submissions = await Submission.GetSubmissionsAsync(
                client,
                classId,
                assignmentId);
            var submissionId = submissions.Value[0].Id;

            //Submit submission
            var submission = await Submission.SubmitAsync(client, classId, assignmentId, submissionId);
            Thread.Sleep(3000);

            //Change to teacher account
            client = GraphClient.GetDelegateClient(config._tenantId, config._appId, config._teacherAccount, config._password);

            //Reassign submission
            submission = await Submission.ReassignAsync(client, classId, assignmentId, submissionId);
            Thread.Sleep(3000);

            //Get submission
            submission = await Submission.GetSubmissionWithHeaderAsync(
                client,
                classId,
                assignmentId,
                submissionId,
                "Prefer",
                "include-unknown-enum-members");

            //Delete current assignment
            await Assignment.DeleteAsync(client, classId, assignmentId);

            submission.Should().NotBeNull("because the assignment always have at least one submission.");
            submission.Status.Should().Be(EducationSubmissionStatus.Reassigned, "because it was reassigned");
        }

        [Test]
        public async Task FeedbackResource()
        {
            const int MAX_RETRIES = 10;
            var classId = config._classId;

            _log.Info($"Create submission feedback resource");

            //Create new assigment
            var assignment = await Assignment.CreateAsync(
                client,
                classId);
            var assignmentId = assignment.Id;
            _log.Info($"Assignment created {assignmentId}");

            // Setup assignment feedback resources folder
            _ = await Assignment.SetupFeedbackResourcesFolderAsync(
                client,
                classId,
                assignmentId);

            // Check feedback resource folder
            int retryNum = 0;
            while (assignment.FeedbackResourcesFolderUrl == null && retryNum <= MAX_RETRIES)
            {
                assignment = await Assignment.GetAssignmentAsync(client, classId, assignmentId);
                retryNum++;
            }
            _log.Info($"Feedback resources folder: {assignment.FeedbackResourcesFolderUrl}");

            //Publish assignment
            assignment = await Assignment.PublishAsync(
                client,
                classId,
                assignmentId);
            Thread.Sleep(7000);

            //Get assignment submissions
            var submissions = await Submission.GetSubmissionsAsync(
                client,
                classId,
                assignmentId);
            var submissionId = submissions.Value[0].Id;

            // Create a new submission feedback resource
            var educationOutcome = new EducationFeedbackResourceOutcome
            {
                OdataType = "#microsoft.graph.educationFeedbackResourceOutcome",
                FeedbackResource = new EducationWordResource
                {
                    OdataType = "#microsoft.graph.educationWordResource",
                    DisplayName = "FeedbackResourceDoc.docx"
                },
            };

            // Attach the new submission feedback resource
            var feedbackResource = await Submission.CreateFeedbackResourceOutcomeAsync(
                client,
                classId,
                assignmentId,
                submissionId,
                educationOutcome);
            Thread.Sleep(2000);
            _log.Info($"Feedback resource created: {feedbackResource.Id}");

            // Get submission outcomes
            var submissionOutcomes = await Submission.GetSubmissionOutcomesAsync(
                client,
                classId,
                assignmentId,
                submissionId);

            // Take the points outcome id
            var pointsOutcomeId = submissionOutcomes.Value.Where(x => x.OdataType == "#microsoft.graph.educationPointsOutcome").Select(x => x.Id).FirstOrDefault();

            // Create the points outcome body
            var pointsOutcome = new EducationPointsOutcome
            {
                OdataType = "#microsoft.graph.educationPointsOutcome",
                Points = new EducationAssignmentPointsGrade
                {
                    OdataType = "#microsoft.graph.educationAssignmentPointsGrade",
                    Points = 90
                }
            };

            // Update the submission points outcome
            var returned = await Submission.PatchOutcomeAsync(
                client,
                classId,
                assignmentId,
                submissionId,
                pointsOutcomeId,
                pointsOutcome);
            Thread.Sleep(2000);
            _log.Info($"Points outcome updated: {pointsOutcome.Points.Points}");

            // Refresh list of submission outcomes
            submissionOutcomes = await Submission.GetSubmissionOutcomesAsync(
                client,
                classId,
                assignmentId,
                submissionId);

            // Verify the new feedback resource is found
            bool resourceFound = false;
            foreach (var submissionResource in submissionOutcomes.Value)
            {
                _log.Info($"Submission resource: {submissionResource.Id}");
                if (feedbackResource.Id == submissionResource.Id)
                {
                    resourceFound = true;
                    break;
                }
            }

            //Delete current assignment
            await Assignment.DeleteAsync(client, classId, assignmentId);

            resourceFound.Should().Be(true, "feedback resource was recently created.");
        }
    }
}
