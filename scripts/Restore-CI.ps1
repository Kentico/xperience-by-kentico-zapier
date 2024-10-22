Import-Module (Resolve-Path Utilities) `
    -Function `
    Get-WebProjectPath, `
    Invoke-ExpressionWithException, `
    Write-Status `
    -Force

$projectPath = Get-WebProjectPath
$repositoryPath = Join-Path $projectPath "App_Data/CIRepository"
$launchProfile = $Env:ASPNETCORE_ENVIRONMENT -eq "CI" ? "Zapier.WebCI" : "DancingGoat"
$configuration = $Env:ASPNETCORE_ENVIRONMENT -eq "CI" ? "Release" : "Debug"
$dbName = $Env:DATABASE_NAME
$dbUser = $Env:DATABASE_USER
$dbPassword = $Env:DATABASE_PASSWORD

$restoreCommand = "dotnet run " + `
    "--launch-profile $launchProfile " + `
    "-c $configuration " + `
    "--no-build " + `
    "--no-restore " + `
    "--project $projectPath " + `
    "--kxp-ci-restore"

$turnOffCI = "sqlcmd " + `
            "-S localhost " + `
            "-d $dbName " + `
            "-U $dbUser " + `
            "-P $dbPassword " + `
            "-Q `"UPDATE CMS_SettingsKey SET KeyValue = N'False' WHERE KeyName = N'CMSEnableCI'`""

$updateCommand = "dotnet run " + `
    "--launch-profile $launchProfile " + `
    "-c $configuration " + `
    "--no-build " + `
    "--project $projectPath " + `
    "--kxp-update " + `
    "--skip-confirmation"

Invoke-ExpressionWithException $restoreCommand
Invoke-ExpressionWithException $turnOffCI
Invoke-ExpressionWithException $updateCommand



Write-Host "`n"
Write-Status 'CI files processed'
Write-Host "`n"