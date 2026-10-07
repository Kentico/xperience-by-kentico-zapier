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
        Resolves the connection string like Get-ConnectionString, with a fallback to appsettings.Development.json
        (written by Init-Project.ps1 for local development).
#>
function Resolve-ConnectionString {
    param (
        [PSCustomObject]$appSettings
    )

    $connectionString = $null
    try {
        $connectionString = Get-ConnectionString $appSettings
    }
    catch {
        $connectionString = $null
    }

    if (-not $connectionString) {
        $devSettingsPath = Join-Path $appSettings.XbKProjectPath "appsettings.Development.json"
        if (Test-Path $devSettingsPath) {
            Write-Host "Using settings from $devSettingsPath"
            $connectionString = (Get-Content $devSettingsPath | Out-String | ConvertFrom-Json).ConnectionStrings.CMSConnectionString
        }
    }

    if (-not $connectionString) {
        throw "No connection string found. Pass -ConnectionString or configure the DancingGoat project first (scripts/Init-Project.ps1)."
    }

    return $connectionString
}

<#
    .DESCRIPTION
        Executes a SQL statement and returns the number of affected rows. Throws on failure.
#>
function Invoke-SqlStatement {
    param (
        [string]$connectionString,
        [string]$query,
        [int]$timeoutSeconds = 600
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $command.CommandTimeout = $timeoutSeconds
        return $command.ExecuteNonQuery()
    }
    finally {
        $connection.Close()
    }
}

<#
    .DESCRIPTION
        Executes a SQL query and returns the first column of the first row. Throws on failure.
#>
function Invoke-SqlScalar {
    param (
        [string]$connectionString,
        [string]$query
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = $query
        $result = $command.ExecuteScalar()
        if ($result -is [DBNull]) { return $null }
        return $result
    }
    finally {
        $connection.Close()
    }
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

    # Reset so a non-zero exit code left behind by an earlier native command cannot fail this call.
    $global:LASTEXITCODE = 0
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