<#
.Synopsis
    Updates the local database to last hotfix version and then update it with all the objects in the CI repository
#>

param (
    [switch]$ExcludeCIRestore
)

./Toggle-CI.ps1;
./Update-DB.ps1;
./Toggle-CI.ps1 -CIEnabled;
if (-not $ExcludeCIRestore) {
    ./Restore-CI.ps1;
}