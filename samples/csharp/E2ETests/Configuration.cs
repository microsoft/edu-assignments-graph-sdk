using Microsoft.Extensions.Configuration;

namespace E2ETests
{
    public class Configuration
    {
        public string _tenantId { get; set; }
        public string _appId { get; set; }
        public string _secret { get; set; }
        public string _teacherAccount { get; set; }
        public string _studentAccount { get; set; }
        public string _password { get; set; }
        public string _classId { get; set; }
        public string _assignmentId { get; set; }
        public string _submissionId { get; set; }

        public Configuration()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            // Read settings to use during client creation
            if (config["tenantId"] != null)
                _tenantId = config["tenantId"].ToString();
            if (config["appId"] != null)
                _appId = config["appId"].ToString();
            if (config["secret"] != null)
                _secret = config["secret"].ToString();
            if (config["teacherAccount"] != null)
                _teacherAccount = config["teacherAccount"].ToString();
            if (config["studentAccount"] != null)
                _studentAccount = config["studentAccount"].ToString();
            if (config["password"] != null)
                _password = config["password"].ToString();
            if (config["classId"] != null)
                _classId = config["classId"].ToString();
            if (config["assignmentId"] != null)
                _assignmentId = config["assignmentId"].ToString();
            if (config["submissionId"] != null)
                _submissionId = config["submissionId"].ToString();
        }
    }
}
