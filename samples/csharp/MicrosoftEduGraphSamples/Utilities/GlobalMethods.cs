using Microsoft.Extensions.Configuration;

namespace MicrosoftEduGraphSamples.Utilities
{
    internal class GlobalMethods
    {
        private static IConfiguration _config;

        public static void ValidateConfiguration(IConfiguration configuration)
        {
            _config = configuration;

            // Verify and throw exception for input values if null or empty
            if (string.IsNullOrEmpty(_config["classId"]))
            {
                throw new Exception("Missing classId please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["tenantId"]))
            {
                throw new Exception("Missing tenantId please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["secret"]))
            {
                throw new Exception("Missing secret please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["appId"]))
            {
                throw new Exception("Missing appId please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["teacherAccount"]))
            {
                throw new Exception("Missing teacherAccount please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["studentAccount"]))
            {
                throw new Exception("Missing studentAccount please check appconfig.json file.");
            }
            else if (string.IsNullOrEmpty(_config["password"]))
            {
                throw new Exception("Missing password please check appconfig.json file.");
            }
        }

    }
}
