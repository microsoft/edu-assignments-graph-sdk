using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

Courses courses = new Courses(config);
await courses.Get();