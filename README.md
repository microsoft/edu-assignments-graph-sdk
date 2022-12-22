# Assignments Code Samples using Microsoft Graph SDK

This repo is a set of code samples that will guide you to easily and quickly integrate the [Microsoft Graph SDK](https://learn.microsoft.com/graph/sdks/sdks-overview) into your applications in order to start building third party solutions for Microsoft EDU assignments service.

These samples also demostrate assignments service functionality exposed through the Microsoft Graph SDK, such as work with classes, users, [assignments, submissions](https://learn.microsoft.com/graph/assignments-submissions-states-transition), resources and work with any of the EDU APIs. Using Microsoft Graph SDK we can perform any operation supported by graph SDK, not just limited to assignments.

## Prerequisites

* An EDU tenant for Azure Active Directoy authentication. Follow [these instructions](https://learn.microsoft.com/graph/msgraph-onboarding-overview) to get set up. You will need some info from the tenant created when filling the `appsettings.json` file in the samples.
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads) or [Visual Studio Code](https://code.visualstudio.com/download).

## Usage

1. Open __microsoft-graph-sdk.sln__ in Visual Studio.

1. Locate the file `appsettings.json` under __MicrosoftEduGraphSamples__ project in Solution Explorer. Replace the contents of that file supplying your values as appropriate:

    ```json
    {
        "tenantId": "YOUR_TENANT_ID",
        "appId": "YOUR_APPLICATION_ID",
        "secret": "YOUR_SECRET",
        "classId": "YOUR_CLASS_ID",
        "teacherAccount": "YOUR_TEACHER_ACCOUNT",
        "studentAccount": "YOUR_STUDENT_ACCOUNT",
        "password": "YOUR_PASSWORD"
    }
    ```

    * __tenantId__: Look for it in [Azure portal](https://learn.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant).
    * __appId__: [Create a new app or take any existing](https://learn.microsoft.com/azure/active-directory/develop/quickstart-register-app) in your Azure portal.
    * __secret__: Look for it in your app registration in Azure portal.
    * __classId__: Create a new class team or [take it from an existing one](https://support.microsoft.com/topic/get-started-in-your-class-team-6b5fd708-35b9-4caf-b66e-d8f2468e4fd5).
    * __teacherAccount__: Any class owner account
    * __studentAccount__: Any member from the class.
    * __password__: Your account's password.

> [!IMPORTANT]
> __Do not commit any references that contain secrets into source control, as secrets should not be made public__.

1. Right-click on the solution in the Solution Explorer and choose __Restore Nuget Packages__.

1. Run __Debug > Start Debugging__ or just press __F5__.

## Project structure

### MicrosoftGraphSDK

It is a set of C# class libraries, those classes contain the actual calls to the Microsoft Graph SDK and each class contains only methods related to an specific entity; for instance User, GraphClient, Assignment, Submission and so on.

All the methods added in those classes can be used into your application.

This project works with these nuget packages:

* [Azure.Identity (1.6.1)](https://www.nuget.org/packages/Azure.Identity).
* [Microsoft.Graph.Beta (latest 4.XX.X) for Beta](https://www.nuget.org/packages/Microsoft.Graph.Beta/4.67.0-preview).
* [Microsoft.Graph (latest 4.XX.X) for V1.0](https://www.nuget.org/packages/Microsoft.Graph/4.48.0).

> __NOTE__: make sure you install only the package needed according to the desired version, the project is using `Microsoft.Graph` package for `v1.0` by default. Both packages (`Microsoft.Graph` and `Microsoft.Graph.Beta`) __cannot__ be installed at the same time.

### Scripts to switch between versions in Visual Studio 2022

1. Switch to V1.0

```
    Uninstall-Package Microsoft.Graph.Beta -Project MicrosoftEduGraphSamples
    Uninstall-Package Microsoft.Graph.Beta -Project MicrosoftGraphSDK
    Install-Package Microsoft.Graph -Version 4.48.0 -Project MicrosoftEduGraphSamples
    Install-Package Microsoft.Graph -Version 4.48.0 -Project MicrosoftGraphSDK
```

2. Switch to Beta

```
    Uninstall-Package Microsoft.Graph -Project MicrosoftEduGraphSamples
    Uninstall-Package Microsoft.Graph -Project MicrosoftGraphSDK
    Install-Package Microsoft.Graph.Beta -Version 4.67.0-preview -Project MicrosoftEduGraphSamples
    Install-Package Microsoft.Graph.Beta -Version 4.67.0-preview -Project MicrosoftGraphSDK
 ```   

### MicrosoftEduGraphSamples

This project provides Microsoft EDU code samples on how to use the methods from the class library. All the samples are developed in a __Workflow__ structure to easily guide you and show you how to catch the responses and use the results.

Use the `Program.cs` file to test any of the current workflows.

1. Add a project reference.

    * Right click on the __MicrosoftEduGraphSamples__ project.
    * Add / Project Reference ...
    * Check the MicrosoftGraphSDK project.
    ![Project references](/images/project-references.png)

2. Install/uninstall nuget packages as needed.

    * Microsoft.Graph.Beta (latest 4.XX.X) for Beta.
    * Microsoft.Graph (latest 4.XX.X) for V1.0. *Installed by default*.

3. Create an instance of the class you want to test.

```csharp
    Submission submission = new Submission(config);
```

4. Call the desired workflow method.

```csharp
    await submission.ReassignWorkflow();
```

## [Code samples](/samples/csharp/MicrosoftEduGraphSamples/workflows)
|    | Sample Name        | Description                                                                      | C#    |
|:--:|:-------------------|:----------------------------------------------------------------------------------------------|:--------|
|1| Reassign submission   | Sample showing how the teacher creates an assignment and then publish it; the student submit his work and then teacher reassign it with feedback.                      |[View](https://github.com/microsoft/edu-assignments-graph-sdk/blob/b895615c3a5cfcbf7f1030a148dbbe4d68446913/samples/csharp/MicrosoftEduGraphSamples/workflows/Submission.cs#L24)|
|2| Filter archived classes from assignments   | Get me assignments from non-archived classes endpoint.                      |[View](https://github.com/microsoft/edu-assignments-graph-sdk/blob/4b92f784855c63c30ec6dc9fb400eb1bb791019e/samples/csharp/MicrosoftEduGraphSamples/Workflows/AssignmentWorkflow.cs#L62)|

## Need help?

* For reference documentation visit the [Microsoft Graph SDK reference](https://learn.microsoft.com/graph/sdks/sdks-overview).
* For other documentation, go to [Working with education APIs in Microsoft Graph](https://learn.microsoft.com/graph/api/resources/education-overview).
* File an issue via [Github Issues](https://github.com/microsoft/edu-assignments-graph-sdk/issues/new).

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
