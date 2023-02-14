using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle.Models;
using System.Text.Json;

namespace MicrosoftEduImportFromGoogle
{
    internal class Courses
    {
        private readonly IConfiguration _config;
        public Courses(IConfiguration configuration)
        {
            this._config = configuration;
        }

        // client configuration

        public async Task Get()
        {
            string accessToken = await Utilities.AuthorizeAppAndGetToken(_config["googleClientId"], _config["googleClientSecret"], _config["googleAuthEndpoint"] );
            string content = await Utilities.MakeHttpGetRequest(accessToken, "https://classroom.googleapis.com/v1/courses");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Dictionary<string, Course[]> courseDictionary = JsonSerializer.Deserialize<Dictionary<string, Course[]>>(content,options);
            foreach(var course in courseDictionary["courses"])
            {
                Console.WriteLine($"Course Id - {course.Id}, Course Name: {course.Name}, Description: {course.Description}");
            }
            
        }


    }
}
