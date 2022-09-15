# Microsoft Graph SDK code samples

This repo is a set of C# class libraries that guide you to easily and quickly integrate the [Microsoft Graph SDK](/graph/sdks/sdks-overview) into your applications.

These samples also demostrates more of the functionality provided by the Microsoft Graph SDK, such as work with classes, users, assignments, submissions, resources and work with any of the EDU APIs.

## Prerequisites

* An EDU tenant for Azure Active Directoy authentication. Follow [these instructions](/graph/msgraph-onboarding-overview) to get set up. You will need some info from the tenant created when filling the `appsettings.json` file in the samples.
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads)

## Usage

1. Open __microsoft-graph-sdk.sln__ in Visual Studio.

1. Locate the file `appsettings.json` under __microsoft-graph-samples__ project in Solution Explorer. Replace the contents of that file supplying your values as appropriate:

    ```json
    {
        "tenantId": "YOUR_TENANT_ID",
        "appId": "YOUR_APPLICATION_ID",
        "secret": "YOUR_SECRET",
        "teacherAccount": "YOUR_TEACHER_ACCOUNT",
        "studentAccount": "YOUR_STUDENT_ACCOUNT",
        "password": "YOUR_PASSWORD",
        "classId": "YOUR_CLASS_ID",
        "assignmentId": "YOUR_ASSIGNMENT_ID",
        "submissionId": "YOUR_SUBMISSION_ID"
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

### microsoft-graph-samples

This project provides some samples on how to use the methods from the class library. All the samples are developed in a __Workflow__ structure to easily guide you and show you how to catch the responses and use the results.

Use the `Program.cs` file to test any of the current workflows.

1. Add one of the using statements according to the version you want to work with.

```csharp
    using microsoft_graph_samples.workflows; // For v1.0
    using microsoft_graph_samples_beta.workflows; // For beta
```

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
