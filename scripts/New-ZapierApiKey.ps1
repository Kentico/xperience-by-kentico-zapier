<#
.Synopsis
    Creates a Zapier API key directly in the database and returns the plaintext key.

.Description
    Equivalent of "Configuration > Zapier > API key > Generate" in the admin UI, for automation
    (CI, E2E tests). Any existing key is replaced, because the integration only honours the first row.

    Run it BEFORE the application starts: the key hash is cached by the running app for one hour
    and a direct database insert does not invalidate that cache.

    When GITHUB_ENV is set, the key is also exported as ZAPIER_API_KEY for later workflow steps.

.Parameter NoExport
    Do not export ZAPIER_API_KEY to GITHUB_ENV even when running in GitHub Actions (the caller keeps the returned key).
    The key is still masked in the GitHub Actions log.

.Parameter Force
    Replace an existing key without prompting. Implied when running in CI (CI or GITHUB_ACTIONS environment variable set).

.Parameter ConnectionString
    Optional. Defaults to the connection string resolved like the other scripts (user secrets,
    then the environment's appsettings file), with a fallback to appsettings.Development.json.

.Example
    cd scripts
    $key = ./New-ZapierApiKey.ps1
#>
param (
    [string]$ConnectionString,
    [switch]$Force,
    [switch]$NoExport
)

Import-Module (Resolve-Path Settings) `
    -Function `
    Get-AppSettings `
    -Force

Import-Module (Resolve-Path Utilities) `
    -Function `
    Invoke-SqlStatement, `
    Invoke-SqlScalar, `
    Resolve-ConnectionString, `
    Write-Status `
    -Force

$appSettings = Get-AppSettings
$connection = if ($ConnectionString) { $ConnectionString } else { Resolve-ConnectionString $appSettings }

# Mirrors Kentico.Xperience.Zapier.Admin.ApiKeyHelper: 32 random bytes as Base64,
# stored as Base64(SHA-256(UTF-8 key)).
$keyBytes = [byte[]]::new(32)
[System.Security.Cryptography.RandomNumberGenerator]::Fill($keyBytes)
$key = [Convert]::ToBase64String($keyBytes)
$hash = [Convert]::ToBase64String([System.Security.Cryptography.SHA256]::HashData([System.Text.Encoding]::UTF8.GetBytes($key)))

$query = @"
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DELETE FROM KenticoZapier_ApiKey;
INSERT INTO KenticoZapier_ApiKey (ApiKeyToken, ApiKeyCreated, ApiKeyCreatedBy)
VALUES ('$hash', GETDATE(), (SELECT TOP 1 UserID FROM CMS_User WHERE UserName = 'administrator'));
COMMIT TRANSACTION;
"@

if (-not $Force -and -not $env:CI -and -not $env:GITHUB_ACTIONS) {
    $existing = [int](Invoke-SqlScalar -connectionString $connection -query "SELECT COUNT(*) FROM KenticoZapier_ApiKey")
    if ($existing -gt 0) {
        throw "A Zapier API key already exists and would be replaced (Zapier connections using it stop working). Re-run with -Force to replace it."
    }
}

$rowsAffected = Invoke-SqlStatement -connectionString $connection -query $query

if ($rowsAffected -lt 1) {
    throw "Inserting the Zapier API key failed (no rows affected)."
}

if ($env:GITHUB_ACTIONS) {
    # Masked even with -NoExport: the caller still passes the key to processes whose output reaches the run log.
    # Written straight to stdout so callers that silence the information stream cannot swallow the mask.
    [Console]::Out.WriteLine("::add-mask::$key")
}

if ($env:GITHUB_ENV -and -not $NoExport) {
    "ZAPIER_API_KEY=$key" >> $env:GITHUB_ENV
}

Write-Host "`n"
Write-Status "Zapier API key created"
Write-Host "`n"

return $key
