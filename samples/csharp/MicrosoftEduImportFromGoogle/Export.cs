using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle.Models;
using System.Text.Json;

namespace MicrosoftEduImportFromGoogle
{
    /// <summary>
    /// Google endpoints needed to export classroom courseWork and courseWorkMaterials
    /// </summary>
    internal class Export
    {
        private readonly IConfiguration _config;
        public string accessToken;
        public const string BASE_GOOGLEAPI_URL = "https://classroom.googleapis.com/v1";
        public const string BASE_GOOGLDRIVE_URL = "https://www.googleapis.com/drive";
        public Export(IConfiguration configuration)
        {
            this._config = configuration;
        }

        /// <summary>
        /// Authorizes the application and sets the Google access token
        /// </summary>
        /// <returns></returns>
        public async Task AuthorizeApp()
        {
            this.accessToken = await GoogleAuthenticator.AuthorizeAppAndGetTokenFromGoogle(_config["googleClientId"], _config["googleClientSecret"], _config["googleAuthEndpoint"]);
        }

        /// <summary>
        /// Returns an array of courses that the requesting user is permitted to view
        /// </summary>
        /// <returns>Course[]</returns>
        public async Task<Course[]> GetCourses()
        {
            List<string> courseIds= new List<string>();
            string content = await Utilities.MakeHttpGetRequest(accessToken, $"{BASE_GOOGLEAPI_URL}/courses");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, Course[]> courseDictionary = JsonSerializer.Deserialize<Dictionary<string, Course[]>>(content, options);
            return (courseDictionary == null) ? null : courseDictionary?["courses"];
        }

        /// <summary>
        /// Returns an array of draft and published course work that the requester is permitted to view
        /// </summary>
        /// <param name="course">Course</param>
        /// <returns>CourseWork[]</returns>
        public async Task<CourseWork[]> GetCourseWork(Course course)
        {
            Console.WriteLine("* Getting coursework for course [{0}] from Google Classroom...", course.Name);
            string url = $"{BASE_GOOGLEAPI_URL}/courses/{course.Id}/courseWork?courseWorkStates=DRAFT&courseWorkStates=PUBLISHED";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWork[]> courseWorkDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWork[]>>(content, options);
            return (courseWorkDictionary == null) ? null : courseWorkDictionary["courseWork"];
        }

        /// <summary>
        /// Returns an array of course work material that the requester is permitted to view.
        /// </summary>
        /// <param name="courseId">Course id</param>
        /// <returns>CourseWorkMaterials[]</returns>
        public async Task<CourseWorkMaterials[]> GetCourseWorkMaterials(string courseId)
        {
            Console.WriteLine("Course Materials");
            Console.WriteLine("________________________________________");
            string url = $"{BASE_GOOGLEAPI_URL}/courses/{courseId}/courseWorkMaterials";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWorkMaterials[]> courseWorkMaterialDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWorkMaterials[]>>(content, options);
            return courseWorkMaterialDictionary["courseWorkMaterial"];
        }

        /// <summary>
        /// Returns a file metadata from Google Drive
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <returns>Dictionary<string, string></returns>
        public async Task<Dictionary<string, string>> GetGoogleDriveFileMetadata(string fileId)
        {
            string url = $"{BASE_GOOGLDRIVE_URL}/v3/files/{fileId}";
            Dictionary<string, string> fileDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(await Utilities.MakeHttpGetRequest(accessToken, url));
            return fileDetails;
        }

        /// <summary>
        /// Exports Google Drive file to target mime type
        /// </summary>
        /// <param name="fileId">File id</param>
        /// <param name="targetMimeType">Target mime type</param>
        /// <param name="export">Flag to export file, default is false</param>
        /// <returns>Byte[]</returns>
        public async Task<Byte[]> GetGoogleDoc(string fileId, string targetMimeType, bool export = false)
        {
            string query = export ? $"/export?mimeType={targetMimeType}" : "?alt=media";
            string url = $"{BASE_GOOGLDRIVE_URL}/v3/files/{fileId}{query}";
            return await Utilities.MakeHttpGetByteArrayRequest(accessToken, url);
        }
    }
}
