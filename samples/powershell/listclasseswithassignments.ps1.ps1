# Prerequisite permissions

# Install-Module Microsoft.Graph

#Install-Module Microsoft.Graph -AllowClobber -Force

# PowerShell Script to list Teams (Classes) and their Assignments in Microsoft Graph API

# Function to get the access token (ensure you have a valid OAuth2 flow to get the token)

function Get-AccessToken {
    # Replace these variables with your app's details
    $tenantId = "YOUR_TENANT_ID"
    $clientId = "YOUR_CLIENT_ID"
    $username = "USERNAME"  # User's username (email or username)
    $password = "PASSWORD"  # User's password (plaintext, should ideally be securely stored)
    $resource = "https://graph.microsoft.com"

    # OAuth 2.0 token endpoint for user credentials flow
    $tokenUrl = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"

    # Prepare the body for the request using ROPC (Resource Owner Password Credentials) flow
    $body = @{
        client_id     = $clientId
        scope         = "https://graph.microsoft.com/.default"
        grant_type    = "password"
        username      = $username
        password      = $password
    }

    # Make the request to get the access token
    try {
        $response = Invoke-RestMethod -Uri $tokenUrl -Method Post -ContentType "application/x-www-form-urlencoded" -Body $body
        return $response.access_token
    } catch {
        Write-Error "Failed to obtain access token: $_"
    }
}
# Get the access token
$token = Get-AccessToken

# Define the Graph API endpoint for listing classes
$uri = "https://graph.microsoft.com/beta/education/me/classes"

# Initialize the output file
$outputFile = "ClassAssignmentsList.txt"
"Class Name" | Out-File -FilePath $outputFile

# Loop through each page of the results (handle pagination)
do {
	try {
    # Get list of classes
    $response = Invoke-RestMethod -Uri $uri -Headers @{Authorization = "Bearer $token"} -Method Get
	} catch {
        Write-Error "Failed to retrieve data from Graph API: $_"
        break
    }

    # Loop through each class
    foreach ($class in $response.value) {
        $className = $class.displayName
        $classId = $class.id

        # Get assignments for the current class (with count=true)
        $assignmentsUri = "https://graph.microsoft.com/beta/education/classes/$classId/assignments?$count=true"
                try {
            $assignmentsResponse = Invoke-RestMethod -Uri $assignmentsUri -Headers @{Authorization = "Bearer $token"} -Method Get -ErrorAction Stop
        }
        catch {
            Write-Host "Error retrieving assignments for class $className. Skipping this request."
            continue  # Skip this class and move to the next class
        }
		
		# Check if there are any assignments
        if ($assignmentsResponse.value.Count -gt 0) {
            # Write class and assignment details to the output file
            """$className"" with $($assignmentsResponse.value.Count) assignments." | Out-File -FilePath $outputFile -Append
            Write-Host "Processed class: $className with $($assignmentsResponse.value.Count) assignments."
        } 

    }

    # If there are more pages, update the URI
    $uri = if ($response."@odata.nextLink") { $response."@odata.nextLink" } else { $null }

} while ($uri)

Write-Host "Class assignments list has been saved to $outputFile."