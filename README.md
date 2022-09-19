# Assignments Code Samples using Microsoft Graph SDK

This repo is a set of code samples that guide you to easily and quickly integrate the [Microsoft Graph SDK](/graph/sdks/sdks-overview) into your applications. In order to start building third party solutions for Microsoft EDU assignments service.

These samples also demostrate assignments service functionality exposed through the Microsoft Graph SDK, such as work with classes, users, [assignments, submissions](/graph/assignments-submissions-states-transition), resources and work with any of the EDU APIs. Using Microsoft Graph SDK we can perform any operation supported by graph SDK, not just limited to assignments.

## Prerequisites

* An EDU tenant for Azure Active Directoy authentication. Follow [these instructions](/graph/msgraph-onboarding-overview) to get set up. You will need some info from the tenant created when filling the `appsettings.json` file in the samples.
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads) or [Visual Studio Code](https://code.visualstudio.com/download).

## Usage

1. Open __microsoft-graph-sdk.sln__ in Visual Studio.

1. Locate the file `appsettings.json` under __microsoft-graph-samples__ project in Solution Explorer. Replace the contents of that file supplying your values as appropriate:

    ```json
    {
        "tenantId": "YOUR_TENANT_ID",               // Look for it in Azure portal; https://learn.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-to-find-tenant
        "appId": "YOUR_APPLICATION_ID",             // Create a new app or take any existing in your Azure portal; https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
        "secret": "YOUR_SECRET",                    // Look for it in your app registration in Azure portal.
        "classId": "YOUR_CLASS_ID",                 // Create a new class team or take it from an existing one; https://support.microsoft.com/en-us/topic/get-started-in-your-class-team-6b5fd708-35b9-4caf-b66e-d8f2468e4fd5
        "teacherAccount": "YOUR_TEACHER_ACCOUNT",   // Any class owner account
        "studentAccount": "YOUR_STUDENT_ACCOUNT",   // Any member from the class.
        "password": "YOUR_PASSWORD"                 // Your account's password.
    }
    ```

> [!IMPORTANT]
> __Be sure not to commit any references that contain secrets into source control, as secrets should not be made public__.

1. Right-click on the solution in the Solution Explorer and choose __Restore Nuget Packages__.

1. Run __Debug > Start Debugging__ or just press __F5__.

## Project structure

### microsoft-graph-sdk

It is a set of C# class libraries, those classes contain the actual calls to the Microsoft Graph SDK (v1.0) and each class contains only methos related to an specific entity; for instance User, GraphClient, Assignment, Submission and so on.

All the methods added in those classes can be used into your application.

This project works with these nuget packages:

* Azure.Identity (1.6.1)
* Microsoft.Graph (latest 4.XX.X)

### microsoft-graph-sdk-beta

The same set of C# class libraries, but the calls are done to the Microsoft Graph SDK (beta). It is a separate project due to packages used are different and involve code changes.

This project works with these nuget packages:

* Azure.Identity (1.6.1)
* Microsoft.Graph.Beta (latest 5.XX.X-preview)
* Microsoft.Identity.Web.MicrosoftGraphBeta (latest 1.XX.X)

### microsoft-graph-samples

This project provides Microsoft EDU code samples on how to use the methods from the class library. All the samples are developed in a __Workflow__ structure to easily guide you and show you how to catch the responses and use the results.

Use the `Program.cs` file to test any of the current workflows.

1. Add a project reference according to the wanted version.

    * Right click on the __microsoft-graph-samples__ project.
    * Add / Project Reference ...
    * Check the version required and make sure the other is unchecked.
    ![Project references](../edu-assignments-graph-sdk/images/project-references.png)

1. Create an instance of the flow to test.

```csharp
    submission_reassign reassign = new submission_reassign(config);
```

1. Call its __workflow__ method.

```csharp
    reassign.workflow();
```

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
