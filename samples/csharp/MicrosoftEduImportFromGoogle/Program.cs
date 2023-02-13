using MicrosoftEduGraphSamples.Workflows;
using Microsoft.Extensions.Configuration;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();