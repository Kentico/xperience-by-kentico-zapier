<#
.Synopsis
    Updates the local database with all the objects in the CI repository
#>

Import-Module (Resolve-Path Settings) `
    -Function `
    Get-AppSettings `
    -Force

Import-Module (Resolve-Path Utilities) `
    -Function `
    Invoke-ExpressionWithException, `
    Write-Status `
    -Force

$appSettings = Get-AppSettings

$command = "dotnet run " + `
    "--launch-profile $($appSettings.LaunchProfile) " + `
    "-c $($appSettings.Configuration) " + `
    "--no-build " + `
    "--no-restore " + `
    "--project $($appSettings.XbKProjectPath) " + `
    "--kxp-ci-restore"

Invoke-ExpressionWithException $command

Write-Host "`n"
Write-Status "CI files processed"
Write-Host "`n"