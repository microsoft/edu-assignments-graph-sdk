// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using MicrosoftEduGraphSamples.Workflows;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

// Create an instance of the class you want to test and call the desired workflow method.
// Each flow represents a common entire process that can be tested using Microsoft Graph SDK, all the sample flows are located in the "workflows" folder.

AssignmentWorkflow workflow = new AssignmentWorkflow(config);
await workflow.AssignmentResource(appOnly: true); 
