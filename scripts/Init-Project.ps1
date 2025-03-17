<#
.Synopsis
    Restore DBs for project.
    Add license keys for project.
    Set DB to consistent state.
#>

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

$backupDatabaseFolderPath = Resolve-Path(Join-Path $($appSettings.WorkspaceFolder) "database")
$backupFileListPath = Join-Path -Path $backupDatabaseFolderPath -ChildPath "backups.txt"

# Read the list of .bak file names from backup.txt
$bakFileNames = Get-Content -Path $backupFileListPath


$extractedBakFiles = @()

# Loop through each .bak file name
foreach ($bakFileName in $bakFileNames) {
    # Define the expected path of the unzipped .bak file
    $bakFilePath = Join-Path -Path $backupDatabaseFolderPath -ChildPath $bakFileName

    # Check if the .bak file already exists
    if (!(Test-Path -Path $bakFilePath)) {
        # If the .bak file doesn't exist, look for the corresponding .zip file
        $zipFilePath = "$bakFilePath.zip"

        # Check if the .zip file exists
        if (Test-Path -Path $zipFilePath) {
            # Unzip the .zip file to the database folder
            Write-Output "Extracting $zipFilePath..."
            Expand-Archive -Path $zipFilePath -DestinationPath $backupDatabaseFolderPath -Force
        } else {
            Write-Output "Warning: Zip file $zipFilePath not found."
            continue
        }
    }

    # Add the extracted .bak file path to the array
    $extractedBakFiles += $bakFileName
}

foreach ($extractedBakFile in $extractedBakFiles) {
    ./Restore-Database.ps1 $extractedBakFile
}

Write-Host "Import license key to imported DBs, then press any key to continue..."
[System.Console]::ReadKey() | Out-Null

./Reset-DatabaseConsistency.ps1

