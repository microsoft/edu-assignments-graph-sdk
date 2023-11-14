using Microsoft.Graph.Beta;
using NUnit.Framework;
using FluentAssertions;
using log4net;
using MicrosoftEduGraphSamples;

namespace E2ETests
{
    public class AssignmentTests
    {
        private readonly ILog _log;
        public AssignmentTests()
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private GraphServiceClient client;
        Configuration config = new Configuration();

        [OneTimeSetUp]
        public void GetClient()
        {
            _log.Info("Get Delegated client");
            client = MicrosoftGraphSDK.GraphClient.GetDelegateClient(config._tenantId, config._appId, config._teacherAccount, config._password);
        }

        [Test]
        public async Task GetAssignment()
        {
            var classId = config._classId;
            var assignmentId = config._assignmentId;

            _log.Info($"Getting assignment({assignmentId}) from class({classId})");

            var assignment = await MicrosoftGraphSDK.Assignment.GetAssignmentAsync(
                client,
                classId,
                assignmentId);

            assignment.Should().NotBeNull("because we are sending valid AssignmentId");
        }

        [Test]
        public async Task GetAssignments()
        {
            var classId = config._classId;

            _log.Info($"Getting assignments from class({classId})");

            var assignments = await MicrosoftGraphSDK.Assignment.GetAssignmentsAsync(
                client,
                classId);

            assignments.Should().NotBeNull("because we are sending classId with assignments.");
            assignments.Value.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetMeAssignments()
        {
            _log.Info($"Getting me assignments");

            var assignments = await MicrosoftGraphSDK.User.GetMeAssignmentsAsync(
                client);

            assignments.Should().NotBeNull("because current teacher has assignments.");
            assignments.Value.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task GetUserAssignments()
        {
            _log.Info($"Getting user assignments");

            var user = await MicrosoftGraphSDK.User.GetUserInfoAsync(client);

            var assignments = await MicrosoftGraphSDK.User.GetUserAssignmentsAsync(client, user.Id);

            assignments.Should().NotBeNull("because current teacher has assignments.");
            assignments.Value.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task CreateAssignment()
        {
            var classId = config._classId;

            _log.Info($"Create assignment setting addToCalendarAction property");

            var assignment = await MicrosoftGraphSDK.Assignment.CreateAsync(
                client,
                classId);

            config._assignmentId = assignment.Id;

            assignment.Should().NotBeNull("because it was just created");
        }

        [Test]
        public async Task GetAssignmentWithHeader()
        {
            var classId = config._classId;
            var assignmentId = config._assignmentId;

            _log.Info($"Getting assignment({assignmentId}) from class({classId}) using request header");

            var assignment = await MicrosoftGraphSDK.Assignment.GetAssignmentWithHeadersAsync(
                client,
                classId,
                assignmentId,
                "Prefer",
                "include-unknown-enum-members");

            assignment.Should().NotBeNull("because we are sending valid AssignmentId");
        }

        [Test]
        public async Task PublishAssignment()
        {
            var classId = config._classId;
            var assignmentId = config._assignmentId;

            _log.Info($"Publish assignment({assignmentId}) from class({classId})");

            var assignment = await MicrosoftGraphSDK.Assignment.PublishAsync(
                client,
                classId,
                assignmentId);

            assignment.Should().NotBeNull("because the assignment already exists");
        }

        //[Test]
        //public async Task MeAssignmentsFromActiveClasses()
        //{
        //    var classId = config._classId;
        //    var assignmentId = config._assignmentId;

        //    _log.Info($"Publish assignment({assignmentId}) from class({classId})");

        //    var assignment = await MicrosoftEduGraphSamples

        //    assignment.Should().NotBeNull("because the assignment already exists");
        //}
    }
}
