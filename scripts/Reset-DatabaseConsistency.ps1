<#
.Synopsis
    Updates the local database to last hotfix version and then update it with all the objects in the CI repository

.Parameter ConnectionString
    Optional. Database to update instead of the configured one. Applies to every step, including the
    "dotnet run --kxp-*" commands, which read it from ConnectionStrings__CMSConnectionString.
#>

param (
    [switch]$ExcludeCIRestore,
    [string]$ConnectionString
)

$previousConnectionString = $env:ConnectionStrings__CMSConnectionString
if ($ConnectionString) {
    # Environment variables override appsettings and user secrets, so dotnet run targets the same database.
    $env:ConnectionStrings__CMSConnectionString = $ConnectionString
}

try {
    ./Toggle-CI.ps1 -ConnectionString $ConnectionString;
    ./Update-DB.ps1;
    ./Toggle-CI.ps1 -CIEnabled -ConnectionString $ConnectionString;
    if (-not $ExcludeCIRestore) {
        ./Restore-CI.ps1;
    }
}
finally {
    $env:ConnectionStrings__CMSConnectionString = $previousConnectionString
}
