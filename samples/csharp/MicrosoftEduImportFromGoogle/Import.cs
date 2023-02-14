using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle.Models;
using System.Text.Json;

namespace MicrosoftEduImportFromGoogle
{
    internal class Import
    {
        private readonly IConfiguration _config;
        public string accessToken;
        public Import(IConfiguration configuration)
        {
            this._config = configuration;
        }
        public async Task AuthorizeApp()
        {
            this.accessToken = await Utilities.AuthorizeAppAndGetToken(_config["googleClientId"], _config["googleClientSecret"], _config["googleAuthEndpoint"]);
        }

        // client configuration

        public async Task<List<string>> GetCourses()
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
            return courseIds;
        }

        public async Task GetCourseWork(string courseId)
        {
            Console.WriteLine("Course Work");
            Console.WriteLine("________________________________________");
            string url = $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWork";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWork[]> courseDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWork[]>>(content, options);
            foreach (var work in courseDictionary["courseWork"])
            {
                Console.WriteLine($"Id: {work.Id},Title: {work.Title}, Description: {work.Description}");
            }
        }
        public async Task GetCourseWorkMaterials(string courseId)
        {
            Console.WriteLine("Course Materials");
            Console.WriteLine("________________________________________");
            string url = $"https://classroom.googleapis.com/v1/courses/{courseId}/courseWorkMaterials";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWorkMaterials[]> courseDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWorkMaterials[]>>(content, options);
            foreach(var material in courseDictionary["courseWorkMaterial"])
            {
                Console.WriteLine($"Id: {material.Id},Title: {material.Title}, Link: {material.AlternateLink}");
            }
        }


    }
}
