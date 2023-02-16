using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftEduImportFromGoogle.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;

namespace MicrosoftEduImportFromGoogle
{
    internal class Export
    {
        private readonly IConfiguration _config;
        public string accessToken;
        public Export(IConfiguration configuration)
        {
            this._config = configuration;
        }
        public async Task AuthorizeApp()
        {
            this.accessToken = await GoogleAuthenticator.AuthorizeAppAndGetTokenFromGoogle(_config["googleClientId"], _config["googleClientSecret"], _config["googleAuthEndpoint"]);
        }

        // client configuration

        public async Task<Course[]> GetCourses()
        {
            List<string> courseIds= new List<string>();
            string content = await Utilities.MakeHttpGetRequest(accessToken, "https://classroom.googleapis.com/v1/courses");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, Course[]> courseDictionary = JsonSerializer.Deserialize<Dictionary<string, Course[]>>(content, options);
            if(courseDictionary != null )
            {
                int i = 0;
                foreach (var course in courseDictionary["courses"])
                {
                    i++;
                    Console.WriteLine($"{i}) Course Id - {course.Id}, Course Name: {course.Name}, Description: {course.Description}");
                    courseIds.Add(course.Id);
                }
            }
            return courseDictionary["courses"];
        }

        public async Task<CourseWork[]> GetCourseWork(string courseId)
        {
            Console.WriteLine("\nCourse Work");
            Console.WriteLine("________________________________________");
            string url = $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWork[]> courseWorkDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWork[]>>(content, options);
            foreach (var work in courseWorkDictionary["courseWork"])
            {
                Console.WriteLine($"Id: {work.Id},Title: {work.Title}, Description: {work.Description}");
            }
            return courseWorkDictionary["courseWork"];
        }

        public async Task<CourseWorkMaterials[]> GetCourseWorkMaterials(string courseId)
        {
            Console.WriteLine("Course Materials");
            Console.WriteLine("________________________________________");
            string url = $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWorkMaterials";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWorkMaterials[]> courseWorkMaterialDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWorkMaterials[]>>(content, options);
            foreach(var material in courseWorkMaterialDictionary["courseWorkMaterial"])
            {
                Console.WriteLine($"Id: {material.Id},Title: {material.Title}, Link: {material.AlternateLink}");
            }
            return courseWorkMaterialDictionary["courseWorkMaterial"];
        }

        public async Task<Dictionary<string, string>> GetGoogleDriveFileMetadata(string fileId)
        {
            string url = $"https://www.googleapis.com/drive/v3/files/{fileId}";
            Dictionary<string, string> fileDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(await Utilities.MakeHttpGetRequest(accessToken, url));
            return fileDetails;
        }

        public async Task<Byte[]> GetGoogleDoc(string fileId, string targetMimeType, bool export = false)
        {
            string query = export ? $"/export?mimeType={targetMimeType}" : "?alt=media";
            //string url;
            //if(export)
            //{
            //    url = $"https://www.googleapis.com/drive/v3/files/{fileId}/export?mimeType={targetMimeType}";
            //}
            //else
            //{
            //    url = $"https://www.googleapis.com/drive/v3/files/{fileId}?alt=media";
            //}
            string url = $"https://www.googleapis.com/drive/v3/files/{fileId}{query}";
            return await Utilities.MakeHttpGetByteArrayRequest(accessToken, url);
        }

    }
}
