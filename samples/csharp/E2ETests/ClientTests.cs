using NUnit.Framework;
using FluentAssertions;
using log4net;

namespace E2ETests
{
    public class ClientTests
    {
        Configuration config = new Configuration();
        private readonly ILog _log;

        public ClientTests()
        {
            _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        [Test]
        public void GetClient_DelegatedPermissions()
        {
            _log.Info($"Getting delegated client for account({config._teacherAccount})");

            var client = MicrosoftGraphSDK.GraphClient.GetDelegateClient(config._tenantId, config._appId, config._teacherAccount, config._password);

            client.Should().NotBeNull("because tenant, app and user credentials are valid.");
        }

        [Test]
        public void GetClient_AppPermissions()
        {
            _log.Info($"Getting application client for app({config._appId})");

            var client = MicrosoftGraphSDK.GraphClient.GetApplicationClient(config._tenantId, config._appId, config._secret);

            client.Should().NotBeNull("because tenant and app credentials are valid.");
        }
    }
}
