
<#
.Synopsis
    Restore DB from Backup.
#>

param (
    [Parameter(Mandatory=$true)]
    [string]$backupName
)

Import-Module (Resolve-Path Settings) `
    -Function `
    Get-AppSettings `
    -Force

Import-Module (Resolve-Path Utilities) `
    -Function `
    Invoke-ExpressionWithException, `
    Get-ConnectionString, `
    Write-Status `
    -Force


$appSettings = Get-AppSettings
$connectionString = Get-ConnectionString $appSettings
$connectionStringMaster = $connectionString -replace 'Initial Catalog=[^;]+', 'Initial Catalog=master'

$backupDatabaseFolderPath = Resolve-Path(Join-Path $($appSettings.WorkspaceFolder) "database")
$backupDatabaseFilePath = Resolve-Path(Join-Path $($backupDatabaseFolderPath) $($backupName))

$fileListQuery = "RESTORE FILELISTONLY FROM DISK = N'$backupDatabaseFilePath'"
$fileList = Invoke-Sqlcmd -Query $fileListQuery -ConnectionString $connectionStringMaster

$logicalDataFile = ($fileList | Where-Object { $_.Type -eq 'D' }).LogicalName
$logicalLogFile = ($fileList | Where-Object { $_.Type -eq 'L' }).LogicalName

$dataFilePath = Join-Path -Path $appSettings.SqlExpressPath -ChildPath "$logicalDataFile.mdf"
$logFilePath = Join-Path -Path $appSettings.SqlExpressPath -ChildPath "$logicalLogFile.ldf"

$restoreQuery = @"
    RESTORE DATABASE [$logicalDataFile]
    FROM DISK = N'$backupDatabaseFilePath'
    WITH FILE = 1,
    MOVE N'$logicalDataFile' TO N'$dataFilePath',
    MOVE N'$logicalLogFile' TO N'$logFilePath',
    REPLACE;
"@

$command = "Invoke-Sqlcmd " + `
    "-Query ""$restoreQuery"" " + `
    "-ConnectionString ""$connectionStringMaster"" "

Invoke-ExpressionWithException $command

Write-Host "`n"
Write-Status "Database $logicalDataFile restored from backup $backupName"
Write-Host "`n"
