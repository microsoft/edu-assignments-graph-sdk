﻿using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle.Models;
using System.Text.Json;

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

        public async Task<Course[]?> GetCourses()
        {
            Console.WriteLine("* Fetching courses from Google Classroom...");
            string content = await Utilities.MakeHttpGetRequest(accessToken, "https://classroom.googleapis.com/v1/courses");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, Course[]>? courseDictionary = JsonSerializer.Deserialize<Dictionary<string, Course[]>>(content, options);
            return (courseDictionary == null) ? null : courseDictionary?["courses"];
        }

        public async Task<CourseWork[]?> GetCourseWork(Course course)
        {
			Console.WriteLine("* Getting coursework for course [{0}] from Google Classroom...", course.Name);
			string url = $"https://classroom.googleapis.com/v1/courses/{course.Id}/courseWork";
            string content = await Utilities.MakeHttpGetRequest(accessToken, url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, CourseWork[]>? courseWorkDictionary = JsonSerializer.Deserialize<Dictionary<string, CourseWork[]>>(content, options);
            return (courseWorkDictionary == null) ? null : courseWorkDictionary["courseWork"];
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


    }
}
