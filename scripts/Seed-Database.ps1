<#
.Synopsis
    Seeds Zapier triggers used by the E2E tests.
#>
param (
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

$appSettings = Get-AppSettings
$connection = if ($ConnectionString) { $ConnectionString } else { Get-ConnectionString $appSettings }

# Trigger URLs use a reserved, never-resolving domain (RFC 6761 .invalid) so CI runs never post real data to Zapier.
# Executed directly, not through Invoke-Expression, so the connection string is neither re-parsed nor echoed.
Invoke-SqlStatement -connectionString $connection -query @"
INSERT INTO KenticoZapier_ZapierTrigger VALUES
    ('e569a1c7', 'BizForm.DancingGoatContactUs', 'Form', 'Create', 'https://hooks.zapier.example.invalid/hooks/standard/18364481/7669f3b896c3454da513d8f361464208/'),
    ('e569a1c8', 'DancingGoat.Cafe', 'Reusable', 'Update', 'https://hooks.zapier.example.invalid/hooks/standard/18364481/7669f3b896c3454da513d8f361464209/'),
    ('e569a1c9', 'CMS.EventLog', '', 'Create', 'https://hooks.zapier.example.invalid/hooks/standard/18364481/7669f3b896c3454da513d8f361464207/');
"@ | Out-Null

Write-Host "`n"
Write-Status "Database seeded"
Write-Host "`n"