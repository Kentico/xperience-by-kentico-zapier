<#
.Synopsis
    Prepares an Xperience by Kentico instance and runs the E2E suites against it:
    the Playwright admin tests (tests/Playwright) and the Zapier contract tests (src/XbKcli, npm run test:e2e).

.Description
    One entry point for developers and GitHub Actions, so both run the same process.

    -Target current  Uses the database of the configured connection string as it is. No restore, no upgrade.
                     Fast inner loop: Zapier contract tests only (Playwright expects the seeded database;
                     force it with -SkipPlaywright:$false). Creates a new Zapier API key there; an existing key is
                     only replaced with -ReplaceApiKey. The contract tests also leave form submissions (marked
                     "e2e-") and event log entries in that database.
    -Target minimal  Restores database/*.bak into a throwaway database (<catalog>_E2E), upgrades it to
                     LastAppliedHotfix (--kxp-update + --kxp-ci-restore), builds at LastAppliedHotfix.
    -Target latest   Like minimal, then rebuilds with -p:XbyKVersion=* (newest Xperience on nuget.org),
                     runs --kxp-update again and the .NET unit tests against that release.
                     This is what the weekly GitHub workflow runs.

    Resources: minimal/latest create one database (dropped at the end unless -KeepDatabase), a log file in
    the temp folder, and start one DancingGoat process (always stopped). "latest" rewrites the packages.lock.json
    of the sample app and the test project during the XbyKVersion=* build; both are put back afterwards.

.Parameter ConnectionString
    Connection string of the database to use (current) or to create (minimal/latest). Defaults to the one the
    other scripts resolve (user secrets, appsettings for the environment, appsettings.Development.json),
    with "_E2E" appended to the catalog name for minimal/latest.

.Parameter BackupPath
    Path of the .bak file as seen by the SQL Server process. Defaults to the first entry of database/backups.txt,
    extracted from its .zip into the database folder when missing. In CI pass the path inside the container.

.Parameter Force
    minimal/latest drop and recreate the target catalog. With an explicit -ConnectionString the catalog must end
    with "_E2E" and differ from the configured one, unless -Force is given.

.Parameter ReplaceApiKey
    -Target current only: replace the Zapier API key of the configured database. Zapier connections using the
    replaced key stop working. minimal/latest always create a key in their throwaway database.

.Parameter LicenseKey
    License key written to the restored database. Defaults to XPERIENCE_BY_KENTICO_LICENSE; when empty, the key
    is copied from the database of the configured connection string if that database is reachable.

.Example
    cd scripts
    ./Invoke-E2E.ps1                              # current database, fastest
    ./Invoke-E2E.ps1 -Target minimal
    ./Invoke-E2E.ps1 -Target latest -KeepDatabase
#>
[CmdletBinding()]
param (
    [ValidateSet("current", "minimal", "latest")]
    [string]$Target = "current",
    [string]$ConnectionString,
    [string]$BackupPath,
    [string]$LicenseKey = $env:XPERIENCE_BY_KENTICO_LICENSE,
    [string]$Url = "https://localhost:14070",
    [switch]$SkipPlaywright,
    [switch]$SkipZapier,
    [switch]$KeepDatabase,
    [switch]$Force,
    [switch]$ReplaceApiKey,
    [int]$StartupTimeoutSeconds = 180
)

$ErrorActionPreference = "Stop"
$PSNativeCommandUseErrorActionPreference = $false

Push-Location $PSScriptRoot

Import-Module (Resolve-Path Settings) -Function Get-AppSettings -Force

# The child scripts re-import Utilities with -Force and their own -Function subset, which removes the functions
# imported here. Call child scripts only through Invoke-ChildScript, which re-imports them afterwards.
function Import-Utilities {
    Import-Module (Join-Path $PSScriptRoot "Utilities") `
        -Function Invoke-ExpressionWithException, Invoke-SqlStatement, Invoke-SqlScalar, Resolve-ConnectionString, Write-Status, Write-Notification `
        -Force
}
Import-Utilities

function Invoke-ChildScript([scriptblock]$script) {
    try { & $script }
    finally { Import-Utilities }
}

# DbConnectionStringBuilder is a dictionary to PowerShell, so use the keyword indexer, not the typed property.
function Get-Catalog([string]$connection) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connection)
    return [string]$builder["Initial Catalog"]
}

function Set-Catalog([string]$connection, [string]$catalog) {
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connection)
    $builder["Initial Catalog"] = $catalog
    return $builder.ConnectionString
}

function ConvertTo-SqlLiteral([string]$value) {
    return $value -replace "'", "''"
}

# Bracket-quoted identifier, same escaping as T-SQL QUOTENAME.
function ConvertTo-SqlIdentifier([string]$value) {
    return "[" + ($value -replace "]", "]]") + "]"
}

function Get-DefaultBackupPath([string]$databaseFolder) {
    $listPath = Join-Path $databaseFolder "backups.txt"
    $name = (Get-Content $listPath | Where-Object { $_.Trim() } | Select-Object -First 1).Trim()
    if (-not $name) { throw "database/backups.txt is empty" }
    $bak = Join-Path $databaseFolder $name
    if (-not (Test-Path $bak)) {
        $zip = "$bak.zip"
        if (-not (Test-Path $zip)) { throw "Neither $bak nor $zip exists" }
        Write-Status "Extracting $zip"
        Expand-Archive -Path $zip -DestinationPath $databaseFolder -Force
    }
    return (Resolve-Path $bak).Path
}

function Restore-ThrowawayDatabase([string]$master, [string]$name, [string]$backup) {
    $dataPath = Invoke-SqlScalar $master "SELECT CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(512))"
    $logPath = Invoke-SqlScalar $master "SELECT CAST(SERVERPROPERTY('InstanceDefaultLogPath') AS nvarchar(512))"

    $moves = @()
    $hasPrimaryData = $false
    $connection = New-Object System.Data.SqlClient.SqlConnection($master)
    try {
        $connection.Open()
        $command = $connection.CreateCommand()
        $command.CommandText = "RESTORE FILELISTONLY FROM DISK = N'$(ConvertTo-SqlLiteral $backup)'"
        $reader = $command.ExecuteReader()
        while ($reader.Read()) {
            $logical = $reader["LogicalName"]
            $type = [string]$reader["Type"]
            # D = data, L = log. FILESTREAM (S) and full-text (F) containers need folder targets; not supported here.
            if ($type -ne "D" -and $type -ne "L") {
                throw "Backup file '$logical' has unsupported type '$type'; only data (D) and log (L) files can be restored by this script."
            }
            $isLog = $type -eq "L"
            $folder = if ($isLog) { $logPath } else { $dataPath }
            $extension = if ($isLog) { "ldf" } elseif ($hasPrimaryData) { "ndf" } else { "mdf" }
            if ($type -eq "D") { $hasPrimaryData = $true }
            $target = "$($folder)$($name)_$($logical).$extension"
            $moves += "MOVE N'$(ConvertTo-SqlLiteral $logical)' TO N'$(ConvertTo-SqlLiteral $target)'"
        }
        $reader.Close()
    }
    finally {
        $connection.Close()
    }

    Invoke-DropDatabase $master $name
    Invoke-SqlStatement $master "RESTORE DATABASE $(ConvertTo-SqlIdentifier $name) FROM DISK = N'$(ConvertTo-SqlLiteral $backup)' WITH $($moves -join ', '), REPLACE" | Out-Null
    Write-Status "Database $name restored from $backup"
}

function Invoke-DropDatabase([string]$master, [string]$name) {
    $identifier = ConvertTo-SqlIdentifier $name
    Invoke-SqlStatement $master @"
IF DB_ID(N'$(ConvertTo-SqlLiteral $name)') IS NOT NULL
BEGIN
    ALTER DATABASE $identifier SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE $identifier;
END
"@ | Out-Null
}

function Remove-ThrowawayDatabase([string]$master, [string]$name) {
    Invoke-DropDatabase $master $name
    Write-Status "Database $name dropped"
}

# A process already listening on the URL (typically a developer's running DancingGoat) would answer the readiness
# probe before the started application fails to bind, and the suites would run against that instance and its database.
function Assert-PortFree([string]$url) {
    $uri = [Uri]$url
    $client = [System.Net.Sockets.TcpClient]::new()
    try {
        $connected = $false
        try { $connected = $client.ConnectAsync($uri.Host, $uri.Port).Wait(5000) } catch { }
        if ($connected) { throw "$url is already in use. Stop the application listening there or pass a different -Url." }
    }
    finally { $client.Dispose() }
}

function Start-App([string]$dll, [string]$log) {
    Assert-PortFree $Url
    Write-Status "Starting $dll (log: $log)"
    $process = Start-Process -FilePath "dotnet" -ArgumentList "`"$dll`"" -WorkingDirectory $projectPath `
        -RedirectStandardOutput $log -RedirectStandardError "$log.err" -NoNewWindow -PassThru
    $script:appLogs += $log
    $script:appProcess = $process
    Wait-ForUrl "$Url/status" $StartupTimeoutSeconds $process
    Write-Status "Application is ready"
}

function Stop-App {
    if ($script:appProcess -and -not $script:appProcess.HasExited) {
        Stop-Process -Id $script:appProcess.Id -Force -ErrorAction SilentlyContinue
        $script:appProcess.WaitForExit(30000) | Out-Null
        Write-Status "Application stopped"
    }
}

# The application caches the API key hash for one hour and a direct database insert does not invalidate that
# cache, so the key is always created while the application is not running.
function New-ApiKey {
    return Invoke-ChildScript { ./New-ZapierApiKey.ps1 -ConnectionString $targetConnection -Force -NoExport 6> $null }
}

function Wait-ForUrl([string]$statusUrl, [int]$timeoutSeconds, [System.Diagnostics.Process]$process) {
    $deadline = (Get-Date).AddSeconds($timeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) { throw "Application exited with code $($process.ExitCode) before it became ready" }
        try {
            $response = Invoke-WebRequest -Uri $statusUrl -Method Get -SkipCertificateCheck -TimeoutSec 5
            if ($response.StatusCode -eq 200) { return }
        }
        catch { }
        Start-Sleep -Seconds 2
    }
    throw "Application did not respond with 200 on $statusUrl within $timeoutSeconds s (missing license key, port in use, or startup error; see the application log)"
}

$appSettings = Get-AppSettings
$repoRoot = (Resolve-Path $appSettings.WorkspaceFolder).Path
$solution = $appSettings.SolutionPath
$projectPath = $appSettings.XbKProjectPath
$configuration = $appSettings.Configuration
$databaseFolder = Join-Path $repoRoot "database"

# The configured connection is only required when -ConnectionString is not given; with it, it only feeds the drop guard below.
$configuredConnection = $null
try {
    $configuredConnection = Resolve-ConnectionString $appSettings
}
catch {
    if (-not $ConnectionString) { throw }
}
$baseConnection = if ($ConnectionString) { $ConnectionString } else { $configuredConnection }
$baseCatalog = Get-Catalog $baseConnection
if (-not $baseCatalog) { throw "The connection string has no Initial Catalog / Database." }

$targetConnection = if ($Target -eq "current" -or $ConnectionString) { $baseConnection } else { Set-Catalog $baseConnection "$($baseCatalog)_E2E" }
$databaseName = Get-Catalog $targetConnection
$masterConnection = Set-Catalog $targetConnection "master"

if ($Target -ne "current" -and -not $Force) {
    # minimal/latest DROP and recreate the target catalog; refuse anything that looks like a real database.
    $looksThrowaway = $databaseName.EndsWith("_E2E", [StringComparison]::OrdinalIgnoreCase)
    $isConfigured = $configuredConnection -and ($databaseName -eq (Get-Catalog $configuredConnection))
    if (-not $looksThrowaway -or $isConfigured) {
        throw "Refusing to drop and recreate '$databaseName' for -Target $Target. Use a catalog ending with _E2E, or pass -Force."
    }
}
$Url = $Url.TrimEnd('/')
# Fail before the database and build work, not minutes later when the application starts.
Assert-PortFree $Url

if ($Target -eq "current" -and -not $SkipZapier -and -not $ReplaceApiKey) {
    # Checked before building so the run fails in seconds, not after the build.
    $existingKeys = [int](Invoke-SqlScalar $targetConnection "SELECT COUNT(*) FROM KenticoZapier_ApiKey")
    if ($existingKeys -gt 0) {
        throw "Database $databaseName already has a Zapier API key; the contract tests need a new one, which replaces it (Zapier connections using it stop working). Re-run with -ReplaceApiKey, or use -Target minimal."
    }
}

if ($Target -eq "current" -and -not $PSBoundParameters.ContainsKey("SkipPlaywright")) {
    # The Playwright suite asserts the seeded state of a freshly restored database (3 triggers, no API key).
    Write-Notification "Target current: Playwright skipped (pass -SkipPlaywright:`$false to force it)"
    $SkipPlaywright = $true
}

Write-Status "E2E target=$Target database=$databaseName url=$Url configuration=$configuration"

$exitCodes = [ordered]@{}
$appProcess = $null
$appLogs = @()
$createdDatabase = $false
# Lock files of the projects that float with XbyKVersion.
$lockFiles = @(
    (Join-Path $projectPath "packages.lock.json"),
    (Join-Path $repoRoot "tests/Kentico.Xperience.Zapier.Tests/packages.lock.json")
)
$lockFileBackups = @{}
$logPath = Join-Path ([IO.Path]::GetTempPath()) "xbyk-e2e-$databaseName.log"
$completed = $false
$savedEnvironment = @{}
foreach ($name in "ConnectionStrings__CMSConnectionString", "ASPNETCORE_ENVIRONMENT", "ASPNETCORE_URLS", "XBYK_URL", "ZAPIER_API_KEY") {
    $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}

try {

    if ($Target -ne "current") {
        if (-not $BackupPath) { $BackupPath = Get-DefaultBackupPath $databaseFolder }
        Restore-ThrowawayDatabase $masterConnection $databaseName $BackupPath
        $createdDatabase = $true

        if (-not $LicenseKey -and $baseConnection -ne $targetConnection) {
            try {
                $LicenseKey = Invoke-SqlScalar $baseConnection "SELECT KeyValue FROM CMS_SettingsKey WHERE KeyName = 'CMSLicenseKey'"
                if ($LicenseKey) { Write-Notification "License key copied from database $(Get-Catalog $baseConnection)" }
            }
            catch {
                $LicenseKey = $null
            }
        }
        if ($LicenseKey) {
            Invoke-SqlStatement $targetConnection "UPDATE CMS_SettingsKey SET KeyValue = '$(ConvertTo-SqlLiteral $LicenseKey)' WHERE KeyName = 'CMSLicenseKey'" | Out-Null
        }
        else {
            Write-Warning "No license key available; the application will not serve requests. Set XPERIENCE_BY_KENTICO_LICENSE or -LicenseKey."
        }

        Invoke-ChildScript { ./Seed-Database.ps1 -ConnectionString $targetConnection }
    }

    # Child processes (dotnet run --kxp-*, the application) must use the target database regardless of appsettings.
    $env:ConnectionStrings__CMSConnectionString = $targetConnection

    # Pipelines build with warnings as errors; local runs stay lenient. Applies to the pinned version only:
    # the XbyKVersion=* build is there to catch runtime breaks on the newest release, and new deprecation
    # warnings from a newer hotfix are expected, not a failure of this repository.
    $buildOptions = if ($env:GITHUB_ACTIONS) { "-warnaserror -p:WarningsNotAsErrors=NU1900%3BNU1901%3BNU1902%3BNU1903%3BNU1904" } else { "" }
    Invoke-ExpressionWithException "dotnet build `"$solution`" -c $configuration $buildOptions"

    if ($Target -ne "current") {
        Invoke-ChildScript { ./Reset-DatabaseConsistency.ps1 -ConnectionString $targetConnection }
    }
    if ($Target -eq "latest") {
        # Back up the lock files as resolved at LastAppliedHotfix (also when dirty or untracked, e.g. during an upgrade)
        # so the XbyKVersion=* resolution is never left behind to be committed.
        foreach ($file in $lockFiles | Where-Object { Test-Path $_ }) {
            $backup = [IO.Path]::GetTempFileName()
            Copy-Item $file $backup -Force
            $lockFileBackups[$file] = $backup
        }
        Invoke-ExpressionWithException "dotnet build `"$solution`" -c $configuration -p:XbyKVersion=*"
        # ci.yml runs the unit tests at LastAppliedHotfix; here they run against the newest release.
        dotnet test "$solution" -c $configuration --no-build -p:XbyKVersion=*
        $exitCodes[".NET unit (latest)"] = $LASTEXITCODE
        Invoke-ChildScript { ./Reset-DatabaseConsistency.ps1 -ConnectionString $targetConnection -ExcludeCIRestore }
    }

    # Ask MSBuild for the output path instead of hardcoding the target framework.
    $dll = (dotnet msbuild "$projectPath" -getProperty:TargetPath -p:Configuration=$configuration | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or -not $dll -or -not (Test-Path $dll)) { throw "Build output not found: '$dll'" }
    $env:ASPNETCORE_URLS = $Url
    if (-not $env:ASPNETCORE_ENVIRONMENT) { $env:ASPNETCORE_ENVIRONMENT = "Development" }

    # Playwright expects no API key (its key test generates and deletes keys), so with Playwright the key is
    # created after it, while the application is stopped, and the application is started again.
    $apiKey = $null
    if (-not $SkipZapier -and $SkipPlaywright) { $apiKey = New-ApiKey }
    Start-App $dll $logPath

    if (-not $SkipPlaywright) {
        Push-Location (Join-Path $repoRoot "tests/Playwright")
        try {
            if (-not (Test-Path "node_modules")) { Invoke-ExpressionWithException "npm ci" }
            npx playwright test
            $exitCodes["Playwright"] = $LASTEXITCODE
        }
        finally { Pop-Location }
    }

    if (-not $SkipZapier) {
        if (-not $apiKey) {
            Stop-App
            $apiKey = New-ApiKey
            Start-App $dll "$logPath.zapier"
        }

        Push-Location (Join-Path $repoRoot "src/XbKcli")
        try {
            if (-not (Test-Path "node_modules")) { Invoke-ExpressionWithException "npm ci" }
            $env:XBYK_URL = $Url
            $env:ZAPIER_API_KEY = $apiKey
            npm run test:e2e
            $exitCodes["Zapier contract"] = $LASTEXITCODE
        }
        finally { Pop-Location }
    }
    $completed = $true
}
finally {
    Import-Utilities
    Stop-App
    $anyFailure = -not $completed -or ($exitCodes.Values | Where-Object { $_ -ne 0 })
    if ($appLogs -and ($anyFailure -or $env:GITHUB_ACTIONS)) {
        foreach ($file in $appLogs | ForEach-Object { $_; "$_.err" }) {
            if (Test-Path $file) {
                Write-Status "----- $file (last 200 lines) -----"
                Get-Content $file -Tail 200
            }
        }
    }
    if ($createdDatabase -and -not $KeepDatabase) {
        try { Remove-ThrowawayDatabase $masterConnection $databaseName } catch { Write-Warning "Could not drop ${databaseName}: $_" }
    }
    elseif ($createdDatabase) {
        Write-Notification "Database $databaseName kept (-KeepDatabase)"
    }
    try {
        foreach ($entry in $lockFileBackups.GetEnumerator()) {
            try { Move-Item $entry.Value $entry.Key -Force }
            catch { Write-Warning "Could not restore $($entry.Key) from $($entry.Value): $_" }
        }
        # Locally, bin still holds the XbyKVersion=* build; scripts that run it with --no-build (Update-DB, Restore-CI)
        # would irreversibly upgrade the developer's database past LastAppliedHotfix. CI runners are discarded.
        if ($lockFileBackups.Count -gt 0 -and -not $env:GITHUB_ACTIONS) {
            Write-Status "Rebuilding at LastAppliedHotfix"
            dotnet build "$solution" -c $configuration | Out-Null
            if ($LASTEXITCODE -ne 0) { Write-Warning "Rebuild at LastAppliedHotfix failed; run 'dotnet build' before using the other scripts." }
        }
    }
    finally {
        foreach ($entry in $savedEnvironment.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
        }
        Pop-Location
    }
}

Write-Host ""
foreach ($entry in $exitCodes.GetEnumerator()) {
    $state = if ($entry.Value -eq 0) { "passed" } else { "FAILED (exit $($entry.Value))" }
    Write-Status "$($entry.Key): $state"
}
$worst = 0
foreach ($code in $exitCodes.Values) { if ($code -ne 0 -and $worst -eq 0) { $worst = $code } }
exit $worst
