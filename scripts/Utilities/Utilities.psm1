# Utilities

<#
    .DESCRIPTION
        Gets the database connection string from the user secrets or appsettings.json file
#>
function Get-ConnectionString {
    param (
        [PSCustomObject]$appSettings
    )

    $projectPath = $appSettings.XbKProjectPath

    # Try to get the connection string from user secrets first
    Write-Host "Checking for a connection string user secrets for project: $projectPath"

    $connectionString = dotnet user-secrets list --project $projectPath `
    | Select-String -Pattern "ConnectionStrings:" `
    | ForEach-Object { $_.Line -replace '^ConnectionStrings:CMSConnectionString \= ', '' }

    if (-not [string]::IsNullOrEmpty($connectionString)) {
        Write-Host 'Using ConnectionString from user-secrets'

        return $connectionString
    }

    $appSettingFileName = $appSettings.AppSettingsFileName
    
    $jsonFilePath = Join-Path $projectPath $appSettingFileName

    Write-Host "Using settings from $jsonFilePath"
    
    if (!(Test-Path $jsonFilePath)) {
        throw "Could not find file $jsonFilePath"
    }

    $appSettingsJson = Get-Content $jsonFilePath | Out-String | ConvertFrom-Json
    $connectionString = $appSettingsJson.ConnectionStrings.CMSConnectionString;
    
    if (!$connectionString) {
        throw "Connection string not found in $jsonFilePath"
    }

    return $connectionString;
}

<#
    .DESCRIPTION
        Executes SQL query given by string with provided connection string
#>
function Invoke-SqlQuery {
    param (
        [string]$connectionString,
        [string]$query
    )

    # Create and open a SQL connection
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)

    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $rowsAffected = $command.ExecuteNonQuery()  # Return the number of affected rows
        
        if ($rowsAffected -ne $null) {
            Write-Notification "Rows affected: $rowsAffected"
        }
        return $rowsAffected
    }
    catch {
        Write-Error "An error occurred: $_"
        return $null
    }
    finally {
        $connection.Close()
    }
}

<#
    .DESCRIPTION
        Ensures the expression successfully exits and throws an exception
        with the failed expression if it does not.
#>
function Invoke-ExpressionWithException {
    param(
        [string]$expression
    )

    Write-Host "$expression"

    Invoke-Expression -Command $expression

    if ($LASTEXITCODE -ne 0) {
        $errorMessage = "[ $expression ] failed`n`n"

        throw $errorMessage
    }
}
function Write-Status {
    param(
        [string]$message
    )

    Write-Host $message -ForegroundColor Blue
}

function Write-Notification {
    param(
        [string]$message
    )

    Write-Host $message -ForegroundColor Magenta
}

function Write-Error {
    param(
        [string]$message
    )

    Write-Host $message -ForegroundColor Red
}