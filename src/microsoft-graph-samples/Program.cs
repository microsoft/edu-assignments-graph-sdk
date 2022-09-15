using Microsoft.Extensions.Configuration;
using microsoft_graph_samples.workflows;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

// Create an instance of the flow to test and call its "workflow" method.
// Each flow represents a common entire process that can be tested using Microsoft Graph SDK, all the sample flows are located into "workflows" folder.
submission_reassign reassign = new submission_reassign(config);
reassign.workflow();
