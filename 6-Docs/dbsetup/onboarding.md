---
id: dbsetup/onboarding
title: DbSetup onboarding
doc-type: onboarding
status: draft
component: DbSetup
source-root: 7-Deployment/DbSetup
owner: unassigned
last-reviewed: 2026-08-14
---

# DbSetup onboarding

## Dependencies

- .NET 8 SDK
- SQL Server with permission to create databases and logins

## Setup

From the repository root:

```bash
dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj -- --server "<sql-server-instance>" --db-name "hotshot_logistics"
```

Interactive mode prompts for missing values. For CI, pass `--non-interactive` and `--sa-connection-string` from the **process environment or a secret store**, never from a file in the repository.

Do not persist generated passwords into tracked files. `--persist-env` writes to the process/user environment only.

## Debug path

- Logs go to the console via `SecureLogger` (secret masking).
- Failures roll back artifacts created in that run (`SqlServerProvisioner`).
- Migrations run in-process from `HotshotLogistics.Data` when that assembly is loadable; otherwise DbSetup builds and invokes `4-Persistence/MigrationRunner`.

## Command line options

| Option | Purpose |
| --- | --- |
| `--server`, `-s` | SQL Server instance |
| `--sa-connection-string`, `--sa-cs` | Privileged connection string from a secret store or process environment |
| `--db-name` | Target database (default `hotshot_logistics`) |
| `--app-user` | Application login name |
| `--password` | Application password (omit to generate) |
| `--non-interactive` | No prompts |
| `--force` | Allow destructive operations |
| `--persist-env` | Persist generated password to the process/user environment, never to the repo |

See the source-root [DbSetup README](../../7-Deployment/DbSetup/README.md) for the overview only.

## Non-standard operating procedures

- Entity Framework migrations are forbidden. This tool applies FluentMigrator migrations only.
- Do not copy connection strings with real passwords into docs, scripts, or READMEs. Use placeholders such as `<sa-connection-string>`.
