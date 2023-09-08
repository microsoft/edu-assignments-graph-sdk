# Microsoft EDU import from Google Classroom

This project has a whole sample to guide partners to import assignments and classwork modules from Google Classroom including their resources.

## Prerequisites

### Google
* [Follow these steps](https://developers.google.com/classroom/quickstart/go) to create a project in the Google Cloud Console and enable the __Classroom API__ for your project.
* [Create an OAuth 2.0 client ID](https://developers.google.com/classroom/guides/auth) for your project and specify the scopes that the application needs. This will allow your application to authenticate with Google and request access to the Classroom data.

### Microsoft
* An EDU tenant for Azure Active Directoy authentication. Follow [these instructions](https://learn.microsoft.com/graph/msgraph-onboarding-overview) to get set up. You will need some info from the tenant created when filling the `appsettings.json` file in the samples.
* Create a new [app registration](https://learn.microsoft.com/azure/active-directory/develop/quickstart-register-app) or take any existing in your Azure portal.
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads) or [Visual Studio Code](https://code.visualstudio.com/download).

## Usage

1. Open __microsoft-graph-sdk.sln__ or the __MicrosoftEduImportFromGoogle.csproj__ in Visual Studio.

1. Locate the file `appsettings.json` under __MicrosoftEduImportFromGoogle__ project in Solution Explorer. Replace the contents of that file supplying your values as appropriate:

    ```json
    {
        "microsoftTenantId": "MICROSOFT_TENANT_ID",
        "microsoftClientId": "MICROSOFT_CLIENT_ID",
        "microsoftSecret": "MICROSOFT_CLIENT_SECRET",
        "googleClientId": "GOOGLE_CLIENT_ID",
        "googleClientSecret": "GOOGLE_CLIENT_SECRET",
        "googleAuthEndpoint": "https://accounts.google.com/o/oauth2/v2/auth",
        "importAll": false,
        "googleSourceClass": {
            "id": null,
            "courseWorkMaterials": [],
            "courseWork": []
        },
        "microsoftTargetClass": {
            "id": null,
            "userId": null
        }
    }
    ```

## Application settings

| Setting | Description | Sample value |
|---------|-------------|--------------|
| microsoftTenantId | Look for it in [Azure portal](https://learn.microsoft.com/azure/active-directory/fundamentals/active-directory-how-to-find-tenant) | |
| microsoftClientId |  [Create a new app or take any existing](https://learn.microsoft.com/azure/active-directory/develop/quickstart-register-app) in your Azure portal. | |
| microsoftSecret | Look for it in your app registration in Azure portal. | |
| googleClientId | Look for it in [Google cloud console](https://cloud.google.com). | |
| googleClientSecret | Look for it in **APIS & Services/Credentials** section in Google cloud console portal. | |
| googleAuthEndpoint | Base URL for Google authentication endpoint. Keep it as default. | Default value is `https://accounts.google.com/o/oauth2/v2/auth` |
| importAll | Indicates if it will import all courseWork and courseWorkMaterials from the selected class of the class specified at `googleSourceClass / id`. | Possible values are `true` and `false`. |
| googleSourceClass / id | Source class from Google classroom. The user will get a prompt asking for source class in case it's `null`. | 592285169927 |
| googleSourceClass / courseWorkMaterials | There are three possible scenarios for **courseWorkMaterials**: <br> 1. **importAll = true**, it doesn't matter what value is specified in courseWorkMaterials, everything will be imported. <br> 2. Only courseWorkMaterials specified here will be imported when **importAll = false**. <br> 3. User will have to choose when nothing be specified here. | <br><br>```[{"id": "618676647100"}]``` <br> ```[]``` |
| googleSourceClass / courseWork | There are three possible scenarios for **courseWork**: <br> 1. **importAll = true**, it doesn't matter what value is specified in courseWork, everything will be imported. <br> 2. Only courseWork specified here will be imported when **importAll = false**. <br> 3. User will have to choose when nothing be specified here. | <br><br>```[{"id": "619450535000"},{"id": "523664831971"}]``` <br> ```[]``` |
| microsoftTargetClass / id | Target class from Microsoft Teams. The user will get a prompt asking for source class in case it's `null`. | 8a9f02fd-1a6d-4f77-a500-5737e191fcc3 |
| microsoftTargetClass / userId | User will have to choose the target class from all classes accesible to this `userId`. | fd2f84b4-0f54-4d47-a7f8-e4fc259dee58 |

> [!IMPORTANT]
> __Do not commit any references that contain secrets into source control, as secrets should not be made public__.

1. Right-click on the solution in the Solution Explorer and choose __Restore Nuget Packages__.
1. Make sure __MicrosoftEduImportFromGoogle__ is set as start up project.
1. Run __Debug > Start Debugging__ or just press __F5__.

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
