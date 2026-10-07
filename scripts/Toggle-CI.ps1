<#
.Synopsis
    Toggle CMSEnableCI Key valu in DB with flag.
#>

param (
    [switch]$CIEnabled,
    [string]$ConnectionString
)

Import-Module (Resolve-Path Settings) `
    -Function `
    Get-AppSettings `
    -Force

Import-Module (Resolve-Path Utilities) `
    -Function `
    Invoke-SqlStatement, `
    Get-ConnectionString, `
    Write-Status `
    -Force

# Use a conditional expression to set KeyValue based on the boolean $Argument
$keyValue = if ($CIEnabled) { 'True' } else { 'False' }

$appSettings = Get-AppSettings
$connection = if ($ConnectionString) { $ConnectionString } else { Get-ConnectionString $appSettings }

# Executed directly, not through Invoke-Expression, so the connection string is neither re-parsed nor echoed.
Invoke-SqlStatement -connectionString $connection -query "UPDATE CMS_SettingsKey SET KeyValue='$keyValue' WHERE KeyName='CMSEnableCI'" | Out-Null

Write-Host "`n"
Write-Status "CI restore for Enabled=$keyValue"
Write-Host "`n"