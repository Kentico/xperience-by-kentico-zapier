# Constant settings for environment
$EnvAppSettings = @{
    CI        = @{
        LaunchProfile         = "Zapier.WebCI"
        Configuration         = "Release"
        AppSettingsFileName   = "appsettings.CI.json"
    }
    default   = @{
        LaunchProfile         = "DancingGoat"
        Configuration         = "Debug"
        AppSettingsFileName   = "appsettings.json"
    }
}

function Get-AppSettings {   
    # --- Constant settings --- #
    $workspaceFolder = ".."
    $solutionFileName = "Kentico.Xperience.Zapier.sln"
    $assemblyName = "DancingGoat"
    $dancingGoatXbKPath = "examples/DancingGoat"
    $repositoryPath = "App_Data/CIRepository"
    $sqlExpressPath = "C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA"

    # --- Derived settings --- #
    # Select the settings based on the environment variable with fallback to default
    $envKey = if ($Env:ASPNETCORE_ENVIRONMENT) { $Env:ASPNETCORE_ENVIRONMENT } else { "default" }
    $appSettings = $EnvAppSettings[$envKey]
    $solutionPath = Resolve-Path(Join-Path $($workspaceFolder) $($solutionFileName))
    $XbKProjectPath = Resolve-Path(Join-Path $($workspaceFolder) $($dancingGoatXbKPath))
    $repositoryPath = Join-Path $XbKProjectPath $repositoryPath
    
    # Return as a custom object
    return @{
        WorkspaceFolder        = $workspaceFolder
        SolutionFileName       = $solutionFileName
        AssemblyName           = $assemblyName
        SolutionPath           = $solutionPath
        XbKProjectPath         = $XbKProjectPath
        RepositoryPath         = $repositoryPath
        SqlExpressPath         = $sqlExpressPath
        LaunchProfile          = $appSettings.launchProfile
        Configuration          = $appSettings.Configuration
        AppSettingsFileName    = $appSettings.AppSettingsFileName
    }
}