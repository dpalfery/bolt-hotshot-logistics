# Requirements Document

This document: [`6-Docs/specs/dbsetup/requirements.md`](6-Docs/specs/dbsetup/requirements.md)

## Introduction
The DB setup CLI automates creation and configuration of the Hotshot Logistics SQL Server database and an application account required by the persistence layer. It prepares the environment so that tests and services relying on a database named `hotshot_logistics.db` or a SQL Server database can run reliably.

## Goals
- Provision a SQL Server database with required schema by invoking FluentMigrator migrations.
- Create an application SQL login and database user with least privileges needed.
- Store or ensure the application password is available via a secure system environment variable.
- Provide an idempotent, Multi-platform (Windows, macOS, CI/Linux containers) CLI usable interactively by a developer and non-interactively by CI.

## Actors
1. Human operator (developer / QA) — runs the CLI locally with elevated privileges when required.
2. CI agent — non-interactive execution in pipelines using secrets (pipeline variables / vault).
3. Deployment agent — automation that provisions production or staging database resources.

## Success criteria
1. WHEN the CLI runs with valid SA or equivalent credentials THEN it SHALL create the database if missing, apply FluentMigrator migrations, create the application login and user, and set the system environment variable for the application password (if requested).
2. WHEN the CLI runs against an environment where the DB and user already exist THEN it SHALL make no destructive changes and SHALL exit successfully (idempotence).
3. WHEN run in CI with supplied credentials THEN it SHALL complete without interactive prompts, and provide exit codes indicating success or failure.
4. WHEN invoked without sufficient privileges THEN the CLI SHALL fail early with a clear error and remediation instructions.

## Non-functional requirements
- Idempotence: Re-running the CLI must be safe and repeatable.
- Multi-platform (Windows, macOS, CI/Linux containers): The tool MUST run on Windows, macOS, and CI/Linux containers. Target runtime: .NET 8+ cross-platform console application.
- Explicit cross-platform constraints:
  - Target .NET 8+ cross-platform console application as the runtime and toolset.
  - Core functionality MUST use .NET APIs — do NOT rely on OS-specific shell commands for core behavior. Prefer `System.Environment`, `System.Runtime.InteropServices.RuntimeInformation`, and `Microsoft.Data.SqlClient` for environment access, OS detection, and SQL connectivity.
  - OS branching is allowed only when unavoidable (for example, persisting system-level environment variables). When branching by OS, use explicit runtime detection and document the branch rationale and behavior.
  - Avoid shelling out to platform shells for core functionality. If a shell call is unavoidable, encapsulate it, document it clearly, and provide a platform-agnostic fallback.
- Secrets & persistence policy:
  - Secure defaults: Generate cryptographically random passwords if not provided and prefer storing secrets in environment variables, pipeline secret stores, or a Key Vault.
  - The CLI MAY persist environment variables only when the operator explicitly requests it (for example via `--persist-env`) and confirms the action interactively. Default behavior MUST NOT persist secrets to the OS.
  - When `--persist-env` is used, the CLI must clearly explain the scope and privileges required for the persistence action and refuse to persist without explicit confirmation.
- Minimal permissions principle: The created application DB user must have the minimum set of permissions required to run the application and migrations.
- Auditability: Operations must be logged (with secrets masked) and sufficient metadata written to a non-sensitive operation log.
- No plaintext secrets in repository or logs.
- Failure and rollback: Fail early on preflight checks (connectivity, credentials, privileges). On partial failures attempt compensating rollbacks for artifacts created by the run.

## Environment assumptions
- The persistence layer lives at `4-Persistence/HotshotLogistics.Data/` and migrations exist in `4-Persistence/HotshotLogistics.Data/Migrations`.
- A SQL Server instance is reachable from the machine running the CLI. On macOS and CI/Linux this is typically provided in one of two ways:
  A) External SQL Server reachable over TCP (recommended for CI and some developer setups) — supply host/port and privileged credentials.
  B) Local Docker container running the official SQL Server image for Linux (e.g., `mcr.microsoft.com/mssql/server`) with SA credentials provided via secure pipeline secrets or local environment (suitable for macOS and Linux development).
- For local macOS developer use, the preferred patterns are:
  - Docker-based SQL Server (local container) or
  - Azure SQL (remote) — use secure connection strings or Key Vault references.
  - Optionally a remote dev SQL Server instance can be used if available.
- Preferred workflow (macOS & CI): For development on macOS prefer a Docker-hosted SQL Server or Azure SQL for remote testing. In CI prefer starting a SQL Server container and injecting SA credentials from pipeline secrets (or use Key Vault). This combination (Docker/Azure SQL + pipeline secrets) reduces platform-specific persistence needs and simplifies non-interactive runs.
- The machine or CI agent has a FluentMigrator runner available (either the project's MigrationRunner in `4-Persistence/MigrationRunner` or a dotnet tool).
- SA or an account with ALTER ANY LOGIN and CREATE DATABASE privileges will be supplied when needed (passed via `--sa-cs`, environment variables, or pipeline secrets).
- Tests under `5-Test` expect a database named `hotshot_logistics.db` or a matching configured name; CI test jobs should ensure the selected database name does not conflict with parallel runs (parameterize DB name via `--db-name`).

## Inputs / Outputs

### Inputs (CLI parameters and environment)
- CLI flags:
  - `--server` (or `-s`): SQL Server instance (e.g. `localhost\\SQLEXPRESS`).
  - `--sa-connection-string` (or `--sa-cs`): SA or privileged connection string (non-interactive).
  - `--db-name` (default `hotshot_logistics`): target database name.
  - `--app-user` (default `hotshot_app`): application DB user/login name.
  - `--password` (optional): explicitly provide application user password (not recommended for CI; prefer env var).
  - `--non-interactive` (flag): run without prompts for CI.
  - `--force` (flag): allow destructive operations (must be combined with confirmation).
- Environment variables:
  - `HOTSHOT_DB_PASSWORD` (system env var): application DB user password.
  - `HOTSHOT_DB_SERVER` (optional): server default.
  - `HOTSHOT_DB_NAME` (optional): name default.
  - `HOTSHOT_DB_APP_USER` (optional): app user default.
- CI secrets: SA credentials or Key Vault references injected into pipeline.

### Outputs / Artifacts
- SQL Server database created (if missing): name set to chosen `--db-name`.
- SQL Server login and user created: `--app-user`.
- System environment variable `HOTSHOT_DB_PASSWORD` set (optionally) or instruction printed to use pipeline secrets.
- FluentMigrator migrations applied against the target database.
- Exit code and operation log indicating success/failure with masked secrets.

## Acceptance criteria (EARS-style)
1. WHEN the CLI is executed with valid privileged credentials THEN it SHALL create the database and run migrations and SHALL create the application login and user and SHALL ensure `HOTSHOT_DB_PASSWORD` is available for the application (either as a documented persistent system environment variable when explicitly requested, or via CI/pipeline secret instructions for non-interactive runs).
2. WHEN the database and login already exist THEN the CLI SHALL verify schema is up-to-date and SHALL NOT overwrite the application password unless explicitly requested via `--password` or `--force`.
3. WHEN the CLI is run non-interactively in CI THEN it SHALL read privileged credentials from environment variables or pipeline secrets and SHALL exit with a non-zero code on failure.
4. WHEN a partial failure occurs during provisioning THEN the CLI SHALL roll back the steps it can and provide an actionable error summary.
5. WHEN implementing cross-platform support THEN the CLI SHALL meet the following testable criteria:
   - Windows developer flow: interactive and non-interactive runs succeed against a local or hosted SQL Server instance (including named instance scenarios). If `--persist-env` is used, the tool documents required elevation steps and demonstrates the environment variable persisted as expected.
   - macOS developer flow: interactive and non-interactive runs succeed against a Docker-hosted SQL Server or Azure SQL instance. The tool documents how to set the `HOTSHOT_DB_PASSWORD` manually (shell profile, `launchctl`) and falls back to process-only environment when persistence is not possible.
   - CI/Linux flow: full setup, migrations, and verification run successfully inside CI (SQL Server provided as a container) using SA credentials injected from pipeline secrets. The CLI supports non-interactive flags and reads credentials from env vars (e.g., `CI_SQL_SA_PASSWORD` or a complete `CI_SA_CONNECTION_STRING`).
   - Automated integration test: an end-to-end integration test runs migrations against a Docker-hosted SQL Server on Linux and passes.
   - Verification check: the CLI performs a lightweight privileged-credential verification (e.g., `SELECT SERVERPROPERTY('ProductVersion')`) and fails fast with a clear message when verification fails.