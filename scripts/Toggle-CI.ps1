<#
.Synopsis
    Toggle CMSEnableCI Key valu in DB with flag.
#>

param (
    [switch]$CIEnabled
)

Import-Module (Resolve-Path Settings) `
    -Function `
    Get-AppSettings `
    -Force

Import-Module (Resolve-Path Utilities) `
    -Function `
    Invoke-ExpressionWithException, `
    Invoke-SqlQuery, `
    Get-ConnectionString, `
    Write-Status `
    -Force

# Use a conditional expression to set KeyValue based on the boolean $Argument
$keyValue = if ($CIEnabled) { 'True' } else { 'False' }

$appSettings = Get-AppSettings
$connection = Get-ConnectionString $appSettings

$command = "Invoke-SqlQuery " + `
    "-connectionString ""$connection"" " + `
    "-query ""UPDATE CMS_SettingsKey SET KeyValue='$keyValue' WHERE KeyName='CMSEnableCI'"" "

Invoke-ExpressionWithException $command

Write-Host "`n"
Write-Status "CI restore for Enabled=$keyValue"
Write-Host "`n"