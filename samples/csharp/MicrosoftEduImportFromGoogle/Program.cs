using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle;
using MicrosoftEduImportFromGoogle.Models;

// See https://aka.ms/new-console-template for more information

// Build configuration
IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .Build();

//Export from Google

Export export = new Export(config);
await export.AuthorizeApp();
Course[] courses = await export.GetCourses();
Console.WriteLine("Select Course and hit Enter to start import process.");
int index = Convert.ToInt32(Console.ReadLine());

CourseWork[] courseWorkList = await export.GetCourseWork(courses[index - 1].Id);
Console.WriteLine("Select Coursework to copy. Enter courseworks to copy as a \"space\" separated list.\neg: 3 5 6\n OR. Enter 0(zero) to copy everything.");
List<string> courseWorkIds = new List<string>();
string courseWorkIndices = Console.ReadLine();
if (Int32.TryParse(Console.ReadLine(), out index) && index == 0)
{
    var courseWorkToCopy = courseWorkList;
}
try
{
    IEnumerable<int> indices = Console.ReadLine().Split(' ').Select(x => Convert.ToInt32(x));
    var courseWorkToCopy = courseWorkList.Where((x, i) => indices.Contains(i));
}
catch (Exception ex)
{
    Console.WriteLine($"Wrong input, {ex.Message}");
}


// Import to Microsoft

Import import = new Import(config);
await import.AuthorizeApp();
var myClasses = import.GetMeClasses();
for(int i = 0; i< myClasses.Count; i++)
{
    Console.WriteLine($"{i+1}) Class Name:{myClasses[i].DisplayName}");
}
Console.WriteLine("Select the class you want to import to...");
int classIndex = Convert.ToInt32(Console.ReadLine());

await import.MapAndCreateAssignments(courseWorkList ,myClasses[classIndex - 1].Id);

//await export.GetCourseWorkMaterials(courseIds[index - 1]);

