# Powershell script

This project has a script to list classes with assignments in a tenant

## Prerequisites

### Run the following commands in powershell as administrator
* Install-Module Microsoft.Graph
* Install-Module Microsoft.Graph -AllowClobber -Force

## Steps to run the script
1. Open the script and give the below details of the client, username, password and save the file.
-	    $tenantId = "YOUR_TENANT_ID"
-	    $clientId = "YOUR_CLIENT_ID"
-	    $username = user@example.com  # User's username (email or username)
-	    $password = "UserPassword123"  # User's password (plaintext, should ideally be securely stored).
2. Open PowerShell in administrator mode.	
2. Navigate to location of drive where the above file is saved.
3. Run the command – PowerShell -file <filename>.ps1

A text file with title “ClassAssignmentsList” is created in the drive where the script is executed from with details of class names along with the number of assignments available. 
