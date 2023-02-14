using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

Import import = new Import(config);
await import.AuthorizeApp();
List<string> courseIds = await import.GetCourses();
Console.WriteLine("Select Course to get Started...");
int index = Convert.ToInt32(Console.ReadLine());
await import.GetCourseWork(courseIds[index-1]);
await import.GetCourseWorkMaterials(courseIds[index - 1]);
