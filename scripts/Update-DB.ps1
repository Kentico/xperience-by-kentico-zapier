<#
.Synopsis
    Updates the local with hotfix according to version of packages of live site.
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
    "--kxp-update " + `
    "--skip-confirmation"

Invoke-ExpressionWithException $command

Write-Host "`n"
Write-Status "Updated DB to latest hotfix"
Write-Host "`n"
