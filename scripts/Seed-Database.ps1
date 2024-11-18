<#
.Synopsis
    Toggle CMSEnableCI Key valu in DB with flag.
#>

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

$appSettings = Get-AppSettings
$connection = Get-ConnectionString $appSettings

$command = "Invoke-SqlQuery " + `
    "-connectionString ""$connection"" " + `
    "-query ""INSERT INTO KenticoZapier_ZapierTrigger VALUES
    ('e569a1c7', 'BizForm.DancingGoatContactUs', 'Form', 'Create', 'https://hooks.zapier.com/hooks/standard/18364481/7669f3b896c3454da513d8f361464208/'),
    ('e569a1c8', 'DancingGoat.Cafe', 'Reusable', 'Update', 'https://hooks.zapier.com/hooks/standard/18364481/7669f3b896c3454da513d8f361464209/'),
    ('e569a1c9', 'CMS.EventLog', '', 'Create', 'https://hooks.zapier.com/hooks/standard/18364481/7669f3b896c3454da513d8f361464207/');
    "" "

Invoke-ExpressionWithException $command

Write-Host "`n"
Write-Status "Database seeded"
Write-Host "`n"