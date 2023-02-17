using ConsoleTools;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using MicrosoftEduImportFromGoogle;
using MicrosoftEduImportFromGoogle.Models;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("--- Google Classroom Migrator (v0.1) ---");

// Build configuration
Console.WriteLine("* Reading configuration...");
IConfiguration config = new ConfigurationBuilder()
	.SetBasePath(System.IO.Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", true, true)
	.Build();

// -- Google Classroom side selections

// Authorize with Google
Console.WriteLine("* Signing in to Google Classroom...");
Thread.Sleep(2000);
Export export = new Export(config);
await export.AuthorizeApp();

// Choose a course
Course[]? courses = await export.GetCourses();
if (courses == null)
{
	Console.WriteLine("!! No courses found in Google Classroom !!");
	goto lastStep;
}

Course? selectedCourse = null;
ConsoleMenu courseMenu = new ConsoleMenu()
.AddRange(courses.Select(x => new Tuple<string, Action>(x.Name, () => { selectedCourse = x; })))
.Add("DONE CHOOSING", ConsoleMenu.Close)
.Configure(config =>
{
	config.WriteHeaderAction = () => Console.WriteLine("** Choose a Google Classroom course to export coursework from:");
});
courseMenu.Show();
courseMenu.CloseMenu();
if (selectedCourse == null)
{
	Console.WriteLine("!! No courses selected !!");
	goto lastStep;
}

// Choose coursework from the selected course
CourseWork[]? courseWorkList = await export.GetCourseWork(selectedCourse);
if (courseWorkList == null)
{
	Console.WriteLine("!! No coursework found in Google Classroom for course [{0}] !!", selectedCourse.Name);
	goto lastStep;
}

List<CourseWork> selectedCourseWorkList = new List<CourseWork>();
ConsoleMenu courseWorkMenu = new ConsoleMenu()
.Add("ADD ALL COURSEWORK", () => { selectedCourseWorkList = courseWorkList.ToList(); })
.AddRange(courseWorkList.Select(x => new Tuple<string, Action>(x.Title, () => { selectedCourseWorkList.Add(x); })))
.Add("DONE CHOOSING", ConsoleMenu.Close)
.Configure(config =>
{
	config.WriteHeaderAction = () => Console.WriteLine("** Choose one or more coursework to export from Google Classroom course [{0}]:", selectedCourse.Name);
});
courseWorkMenu.Show();
courseWorkMenu.CloseMenu();
selectedCourseWorkList = selectedCourseWorkList.DistinctBy(x => x.Id).ToList();
if (!selectedCourseWorkList.Any())
{
	Console.WriteLine("!! No coursework selected from course [{0}] !!", selectedCourse.Name);
	goto lastStep;
}

// -- Microsoft Teams side selections

// Authorize with Microsoft
Import import = new Import(config);
await import.AuthorizeApp();

// Choose a class
var classes = import.GetMeClasses();
if (!classes.Any())
{
	Console.WriteLine("!! No classes found in Microsoft Teams !!");
	goto lastStep;
}

EducationClass? selectedClass = null;
ConsoleMenu classMenu = new ConsoleMenu()
.AddRange(classes.Select(x => new Tuple<string, Action>(x.DisplayName, () => { selectedClass = x; })))
.Add("DONE CHOOSING", ConsoleMenu.Close)
.Configure(config =>
{
	config.WriteHeaderAction = () => Console.WriteLine("** Choose a Microsoft Teams class team to import Google Classroom coursework to:");
});
classMenu.Show();
classMenu.CloseMenu();
if (selectedClass == null)
{
	Console.WriteLine("!! No class selected !!");
	goto lastStep;
}

// -- Do the actual migration

await import.MapAndCreateAssignments(selectedCourseWorkList.ToArray(), selectedClass.Id, export);
Console.WriteLine("--- Google Classroom migration to Microsoft Teams completed successfully! ---");

lastStep:
Console.WriteLine("* Type any key to exit...");
Console.ReadKey();
