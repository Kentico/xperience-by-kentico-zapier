# Contributing Setup

## Required Software

The requirements to setup, develop, and build this project are listed below.

### .NET Runtime

.NET SDK 8.0 or newer

- <https://dotnet.microsoft.com/en-us/download/dotnet/8.0>
- See `global.json` file for specific SDK requirements

### Node.js Runtime

- [Node.js](https://nodejs.org/en/download) 20.10.0 or newer
- [NVM for Windows](https://github.com/coreybutler/nvm-windows) to manage multiple installed versions of Node.js
- See `engines` in the solution `package.json` for specific version requirements

### C# Editor

- VS Code
- Visual Studio
- Rider

### Database

SQL Server 2019 or newer compatible database

- [SQL Server Linux](https://learn.microsoft.com/en-us/sql/linux/sql-server-linux-setup?view=sql-server-ver15)
- [Azure SQL Edge](https://learn.microsoft.com/en-us/azure/azure-sql-edge/disconnected-deployment)

### SQL Editor

- MS SQL Server Management Studio
- Azure Data Studio

## Sample Project

### Database Setup

Running the sample project requires creating a new Xperience by Kentico database using the included template.

Change directory in your console to `./examples/DancingGoat` and follow the instructions in the Xperience
documentation on [creating a new database](https://docs.xperience.io/xp26/developers-and-admins/installation#Installation-CreatetheprojectdatabaseCreateProjectDatabase).

### Admin Customization

To run the Sample app Admin customization in development mode, add the following to your [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-7.0&tabs=windows#secret-manager) for the application.

```json
"CMSAdminClientModuleSettings": {
  "kentico-xperience-integrations-zapier": {
    "Mode": "Proxy",
    "Port": 3009
  }
}
```

## Tests

| Layer | Command | Needs |
| --- | --- | --- |
| Zapier CLI unit tests (jest + nock, offline) | `cd src/XbKcli && npm test` | Node |
| .NET unit tests (NUnit, Kentico fakes, no database) | `dotnet test` | .NET SDK |
| E2E: Playwright admin tests + Zapier contract tests | `cd scripts && ./Invoke-E2E.ps1 -Target <current\|minimal\|latest>` | SQL Server (LocalDB is enough), Node, Playwright browsers (`npx playwright install` once) |

`Invoke-E2E.ps1` is the single entry point used locally and by the `E2E: Build and Test` GitHub workflow:

- `current` runs against the database of your configured connection string as it is. Fast; it creates a new Zapier API key there (refusing to replace an existing one unless you pass `-ReplaceApiKey`), and the contract tests leave form submissions marked `e2e-` and event log entries behind. Don't point it at a database with data you care about.
- `minimal` restores `database/*.bak` into `<catalog>_E2E`, upgrades it to `LastAppliedHotfix` (`--kxp-update`, `--kxp-ci-restore`) and tests at that version.
- `latest` does the same, then rebuilds with `-p:XbyKVersion=*` and upgrades again, so the newest Xperience release is tested. This is what CI runs weekly.

The throwaway database and the started application are removed when the script ends (`-KeepDatabase` keeps the database for inspection).
The Zapier contract tests alone, against any running instance, are `npm run test:e2e` with `XBYK_URL` and `ZAPIER_API_KEY` set; see `src/XbKcli/test/e2e/README.md`.

### Upgrading Xperience by Kentico

An upgrade is never only a package bump. After changing `LastAppliedHotfix` in `Directory.Packages.props`:

1. Run `./Invoke-E2E.ps1 -Target minimal -KeepDatabase` to upgrade a copy of the sample database with `--kxp-update`. Expect the `--kxp-ci-restore` step to fail at this point: the committed repository still describes the previous schema. The database is kept because of `-KeepDatabase`.
2. Point the sample project at that database, run `dotnet run --project examples/DancingGoat -- --kxp-ci-store` and commit the regenerated `App_Data/CIRepository`.
3. Optionally back up that database over `database/*.bak.zip`. Only do this on a SQL Server no newer than the CI container (SQL Server 2022, see `e2etest.yml`); a backup taken on a newer engine such as LocalDB 2025 cannot be restored there. Keeping the old backup is fine: the pipeline upgrades it on every run.
4. Run `./Invoke-E2E.ps1 -Target latest` before merging.

## Development Workflow

1. Create a new branch with one of the following prefixes

   - `feat/` - for new functionality
   - `refactor/` - for restructuring of existing features
   - `fix/` - for bugfixes

1. Run `dotnet format` against the `src/Kentico.Xperience.Zapier` project

   > use `dotnet: format` VS Code task.

1. Commit changes, with a commit message preferably following the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/#summary) convention.

1. Once ready, create a PR on GitHub. The PR will need to have all comments resolved and all tests passing before it will be merged.

   - The PR should have a helpful description of the scope of changes being contributed.
   - Include screenshots or video to reflect UX or UI updates
   - Indicate if new settings need to be applied when the changes are merged - locally or in other environments
