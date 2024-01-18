using ConsoleTools;
using Microsoft.Extensions.Configuration;
using MicrosoftEduImportFromGoogle;
using MicrosoftEduImportFromGoogle.Models;
using System.Transactions;

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

// Google class specified in config?
var googleClassId = config.GetSection("googleSourceClass:id").Value;
Course selectedCourse = null;

bool showSelectedCourse = true;
if (string.IsNullOrEmpty(googleClassId))
{
    // Choose a course
    Course[] courses = await export.GetCourses();
    if (courses == null)
    {
        Console.WriteLine("!! No courses found in Google Classroom !!");
        goto lastStep;
    }

    void DisplaySelectedCourses()
    {
        //Thread keeps running until showSelectedCourse is set to false, which happens  
        //when the user moves to the next step in the console menu
        while (showSelectedCourse)
        {
            Task.WaitAll(Task.Delay(500));

            if (selectedCourse != null)
            {
                int i = 0;
                foreach (var course in courses)
                {
                    if (course.Id == selectedCourse.Id)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            Console.SetCursorPosition(4, i + 1);
                            Console.Write("X");
                            Console.SetCursorPosition(0, 0);
                            break;
                        }
                    }
                    i++;
                }
            }
            Task.WaitAll(Task.Delay(500));
        }
    }
    ThreadStart work = DisplaySelectedCourses;
    Thread thread = new Thread(work);
    thread.Start();
    Console.Write("Choose a course: ");
    
    ConsoleMenu courseMenu = new ConsoleMenu()
    .AddRange(courses.Select(x => new Tuple<string, Action>(x.Name, () => { selectedCourse = x; })))
    .Add("DONE CHOOSING", ConsoleMenu.Close)
    .Configure(config =>
    {
        config.WriteHeaderAction = () => Console.WriteLine("** Choose a Google Classroom course to export coursework from:");        
        config.WriteItemAction = item => Console.Write("[ ][{0}] {1}", item.Index, item.Name);
    });
    courseMenu.Show();
    courseMenu.CloseMenu();
    showSelectedCourse = false;

    if (selectedCourse == null)
    {
        Console.WriteLine("!! No courses selected !!");
        goto lastStep;
    }
}
else {
    selectedCourse = new Course() { Id = googleClassId };
}

// Google coursework materials list in config?
List<CourseWorkMaterials> selectedCourseWorkMaterialsList = new List<CourseWorkMaterials>();
int materialsFoundCount = config.GetSection("googleSourceClass:courseWorkMaterials").GetChildren().Count();
CourseWorkMaterials[] courseWorkMaterials = await export.GetCourseWorkMaterials(selectedCourse.Id);
bool importAll = Convert.ToBoolean(config["importAll"]);

bool showSelectedCourseWorkMaterials = true;
void DisplaySelectedCourseWorkMaterials()
{
    //Thread keeps running until showSelectedCourseWorkMaterials is set to false, which happens  
    //when the user moves to the next step in the console menu
    while (showSelectedCourseWorkMaterials)
    {
        Task.WaitAll(Task.Delay(500));
        if (selectedCourseWorkMaterialsList != null)
        {
            int i = 0;
            foreach (var courseWorkMaterial in courseWorkMaterials)
            {
                foreach (var selectedMaterial in selectedCourseWorkMaterialsList)
                {
                    if (selectedMaterial.Id == courseWorkMaterial.Id)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            Console.SetCursorPosition(4, i + 2);
                            Console.Write("X");
                            Console.SetCursorPosition(0, 0);
                        }
                        break;
                    }
                }
                i++;
            }
        }
        Task.WaitAll(Task.Delay(500));
    }
}


if (importAll)
{
    // Pass all courseWorkMaterials to the selected list
    selectedCourseWorkMaterialsList = courseWorkMaterials.ToList();
}
else if (materialsFoundCount == 0)
{
    // Choose coursework materilas from the selected course
    if (courseWorkMaterials == null)
    {
        Console.WriteLine("!! No coursework materials found in Google Classroom for course [{0}] !!", selectedCourse.Name);
        goto lastStep;
    }
    ThreadStart work2 = DisplaySelectedCourseWorkMaterials;
    Thread thread2 = new Thread(work2);
    thread2.Start();

    ConsoleMenu courseMaterialsWorkMenu = new ConsoleMenu()
    .Add("ADD ALL COURSEWORK MATERIALS", () => { selectedCourseWorkMaterialsList = courseWorkMaterials.ToList(); })
    .AddRange(courseWorkMaterials.Select(x => new Tuple<string, Action>(x.Title, () => { selectedCourseWorkMaterialsList.Add(x); })))
    .Add("DONE CHOOSING", ConsoleMenu.Close)
    .Configure(config =>
    {
        config.WriteHeaderAction = () => Console.WriteLine("** Choose one or more coursework materials to export from Google Classroom course [{0}]:", selectedCourse.Name);
        config.WriteItemAction = item => Console.Write("[ ][{0}] {1}", item.Index, item.Name);
    });
    courseMaterialsWorkMenu.Show();
    courseMaterialsWorkMenu.CloseMenu();
    showSelectedCourseWorkMaterials = false;
    selectedCourseWorkMaterialsList = selectedCourseWorkMaterialsList.DistinctBy(x => x.Id).ToList();
    if (!selectedCourseWorkMaterialsList.Any())
    {
        Console.WriteLine("!! No coursework materials selected from course [{0}] !!", selectedCourse.Name);
        //goto lastStep;
    }
}
else {
    // Select only courseWorkMaterials specified in the config
    for (int m = 0; m < materialsFoundCount; m++) {
        string id = config.GetSection("googleSourceClass:courseWorkMaterials").GetSection($"{m}:id").Value;
        CourseWorkMaterials courseworkMaterials = courseWorkMaterials.SingleOrDefault(x => x.Id == id);
        selectedCourseWorkMaterialsList.Add(courseworkMaterials);
    }
}

// Google coursework list in config?
List<CourseWork> selectedCourseWorkList = new List<CourseWork>();
int courseworkFoundCount = config.GetSection("googleSourceClass:courseWork").GetChildren().Count();
CourseWork[] courseWorkList = await export.GetCourseWork(selectedCourse);

if (importAll) {
    // Pass all courseWork to the selected list
    selectedCourseWorkList = courseWorkList.ToList();
}
else if (courseworkFoundCount == 0)
{
    // Choose coursework from the selected course
    if (courseWorkList == null)
    {
        Console.WriteLine("!! No coursework found in Google Classroom for course [{0}] !!", selectedCourse.Name);
        goto lastStep;
    }
    bool showSelectedCourseworks = true;
    void DisplaySelectedCourseworks()
    {
        //Thread keeps running until showSelectedCourseworks is set to false, which happens
        //when the user moves to the next step in the console menu
        while (showSelectedCourseworks)
        {
            Task.WaitAll(Task.Delay(500));
            if (selectedCourseWorkMaterialsList != null)
            {
                int i = 0;
                foreach (var courseWork in courseWorkList)
                {
                    foreach (var selectedCoursework in selectedCourseWorkList)
                    {
                        if (selectedCoursework.Id == courseWork.Id)
                        {
                            using (TransactionScope scope = new TransactionScope())
                            {
                                Console.SetCursorPosition(4, i + 2);
                                Console.Write("X");
                                Console.SetCursorPosition(0, 0);
                            }
                            break;
                        }
                    }
                    i++;
                }
            }
            Task.WaitAll(Task.Delay(500));
        }

    }
    ThreadStart work3 = DisplaySelectedCourseworks;
    Thread thread3 = new Thread(work3);
    thread3.Start();
    ConsoleMenu courseWorkMenu = new ConsoleMenu()
    .Add("ADD ALL COURSEWORK", () => { selectedCourseWorkList = courseWorkList.ToList(); })
    .AddRange(courseWorkList.Select(x => new Tuple<string, Action>(x.Title, () => { selectedCourseWorkList.Add(x); })))
    .Add("DONE CHOOSING", ConsoleMenu.Close)
    .Configure(config =>
    {
        config.WriteHeaderAction = () => Console.WriteLine("** Choose one or more coursework to export from Google Classroom course [{0}]:", selectedCourse.Name);
        config.WriteItemAction = item => Console.Write("[ ][{0}] {1}", item.Index, item.Name);
    });
    courseWorkMenu.Show();
    courseWorkMenu.CloseMenu();
    showSelectedCourseworks = false;
    selectedCourseWorkList = selectedCourseWorkList.DistinctBy(x => x.Id).ToList();
    if (!selectedCourseWorkList.Any())
    {
        Console.WriteLine("!! No coursework selected from course [{0}] !!", selectedCourse.Name);
        //goto lastStep;
    }
}
else {
    // Select only courseWork specified in the config
    for (int m = 0; m < courseworkFoundCount; m++)
    {
        string id = config.GetSection("googleSourceClass:courseWork").GetSection($"{m}:id").Value;
        CourseWork coursework = courseWorkList.SingleOrDefault(x => x.Id == id);
        selectedCourseWorkList.Add(coursework);
    }
}

// -- Microsoft Teams side selections

// Authorize with Microsoft
Import import = new Import(config);
import.AuthorizeApp();

// Microsoft class specified in config?
var microsoftClassId = config.GetSection("microsoftTargetClass:id").Value;
var microsoftUserId = config.GetSection("microsoftTargetClass:userId").Value;

if (string.IsNullOrEmpty(microsoftClassId))
{
    // Choose a class
    var classes = new List<(string, string)>();

    if (!string.IsNullOrEmpty(microsoftUserId))
    {
        var userClasses = import.GetUserClasses(microsoftUserId);
        classes = userClasses.Select(c => (c.Id, c.DisplayName)).ToList();
    }
    else
    {
        bool delegated = config.GetSection("microsoftAuthMethod").Value == "delegated";
        var allClasses = import.GetClasses(delegated);
        classes = allClasses.Select(c => (c.Id, c.DisplayName)).ToList();
    }

    if (!classes.Any())
    {
        Console.WriteLine("!! No classes found in Microsoft Teams !!");
        goto lastStep;
    }
    bool showSelectedClassTeam = true;
    void DisplaySelectedClassTeam()
    {
        while (showSelectedClassTeam)
        {
            Task.WaitAll(Task.Delay(500));

            if (microsoftClassId != null)
            {
                int i = 0;
                foreach (var classTeam in classes)
                {
                    if (classTeam.Item1 == microsoftClassId)
                    {
                        using (TransactionScope scope = new TransactionScope())
                        {
                            Console.SetCursorPosition(4, i + 1);
                            Console.Write("X");
                            Console.SetCursorPosition(0, 0);
                            break;
                        }
                    }
                    i++;
                }
            }
            Task.WaitAll(Task.Delay(500));

        }

    }
    ThreadStart work4 = DisplaySelectedClassTeam;
    Thread thread4 = new Thread(work4);
    thread4.Start();
    ConsoleMenu classMenu = new ConsoleMenu()
    .AddRange(classes.Select(x => new Tuple<string, Action>(x.Item2, () => { microsoftClassId = x.Item1; })))
    .Add("DONE CHOOSING", ConsoleMenu.Close)
    .Configure(config =>
    {
        config.WriteHeaderAction = () => Console.WriteLine("** Choose a Microsoft Teams class team to import Google Classroom coursework to:");
        config.WriteItemAction = item => Console.Write("[ ][{0}] {1}", item.Index, item.Name);
    });
    classMenu.Show();
    classMenu.CloseMenu();
    showSelectedClassTeam = false;
    Console.Clear();
    if (microsoftClassId == null)
    {
        Console.WriteLine("!! No class selected !!");
        goto lastStep;
    }
}

// -- Do the actual migration
await import.MapCourseWorksToAssignments(selectedCourseWorkList.ToArray(), microsoftClassId, export);
await import.MapCourseWorkMaterialsToModules(selectedCourseWorkMaterialsList.ToArray(), microsoftClassId, export);

Console.WriteLine("--- Google Classroom migration to Microsoft Teams completed successfully! ---");

lastStep:
Console.WriteLine("* Type any key to exit...");
Console.ReadKey();
