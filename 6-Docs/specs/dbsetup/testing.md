# Testing Plan

This document: [`6-Docs/specs/dbsetup/testing.md`](6-Docs/specs/dbsetup/testing.md:1)

Purpose
- Define automated and manual tests required to verify the DB Setup CLI provisions the Hotshot Logistics database, creates the application account, applies migrations, and leaves the system in a reproducible, secure state.

References
- Migrations: [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1)
- Migration runner: [`4-Persistence/MigrationRunner/Program.cs`](4-Persistence/MigrationRunner/Program.cs:1)
- Requirements: [`6-Docs/specs/dbsetup/requirements.md`](6-Docs/specs/dbsetup/requirements.md:1)
- Design: [`6-Docs/specs/dbsetup/design.md`](6-Docs/specs/dbsetup/design.md:1)
- CLI usage: [`6-Docs/specs/dbsetup/cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1)
- Security: [`6-Docs/specs/dbsetup/security.md`](6-Docs/specs/dbsetup/security.md:1)

----------------------------------------------------------------
1) Automated tests the CLI should enable
----------------------------------------------------------------

Unit tests (fast, isolated)
- EnvironmentManager unit tests
  - Verify reading of `HOTSHOT_DB_*` environment variables using `Environment.GetEnvironmentVariable`.
  - Validate behavior when attempting to set a system env variable without privileges (simulate/abstract).
  - Expected outputs: correct fallback logic and error handling.

- PasswordManager unit tests
  - Validate generated passwords meet policy (length, character classes).
  - Validate that a supplied password is accepted or rejected per policy.

- Logger masking tests
  - Ensure logger redacts any value that matches the configured secret(s) or contains `password`, `pwd`, or `connection string`.
  - Assert that log lines never contain the literal value of `HOTSHOT_DB_PASSWORD`.

- ArgumentParser tests
  - Validate precedence: CLI flags > env vars > interactive defaults.
  - Validate `--non-interactive` enforces required flags.

Integration tests (requires a disposable SQL Server instance)
- Test: Fresh provisioning end-to-end
  - Setup: start a disposable SQL Server instance (Docker `mcr.microsoft.com/mssql/server:2019-latest` or LocalDB/SQL Express).
  - Run CLI non-interactively with SA/privileged connection string provided via CI secrets (or test secret injection).
  - Assertions:
    - Database named `hotshot_logistics` (or `--db-name`) exists.
    - Server login `hotshot_app` (or `--app-user`) exists.
    - Database user exists and has expected permissions.
    - All FluentMigrator migrations from [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1) applied (e.g., expected tables exist).
  - Clean up: drop DB and login (see Cleanup section).

- Test: Idempotence
  - Run CLI twice in succession with same parameters.
  - Assertions:
    - Second run reports no destructive changes and exits with code 0.
    - Migration runner indicates schema is up-to-date.

- Test: Migration-only path
  - Prepare DB in older migration state, run CLI to invoke migrations, assert migration history updated and schema updated.

- Test: Privilege failure handling
  - Run CLI with non-privileged account; assert preflight fails with exit code 2 and helpful message; no partial DB/login created.

- Test: Password persistence behavior
  - Run CLI generating a password and selecting NOT to persist to machine env; assert output instructs manual secret persistence and no system env var is set.
  - Run CLI and allow persistence; assert `HOTSHOT_DB_PASSWORD` available to processes started after the setting (note: setting machine env may require new session; test in-process reading where applicable).

Test harness recommendations
- Use a test fixture that can start/stop a SQL Server container or spin up LocalDB for isolation.
- Use `Dapper` or direct ADO.NET for verification queries in tests (remember: production code must use native ADO.NET; tests may use Dapper for conciseness).
- Ensure tests clean up created objects to avoid stateful failures across test runs.

----------------------------------------------------------------
2) Manual test plan (human operator)
----------------------------------------------------------------

Preconditions:
- Have a Windows machine with .NET SDK and access to a SQL Server instance.
- If testing privileged flows, have SA credentials or an account with `CREATE DATABASE` and `CREATE LOGIN` rights.

Manual steps:
1. Interactive fresh run
   - Ensure `HOTSHOT_DB_PASSWORD` not set in machine environment.
   - Run `HotshotLogistics.DbSetup` without flags.
   - Follow prompts: enter server (e.g., `localhost\SQLEXPRESS`), provide privileged connection string when requested.
   - When asked to generate a password accept; when asked to persist to environment accept.
   - Expected results:
     - CLI reports DB and login creation, migrations applied.
     - Verify system env `HOTSHOT_DB_PASSWORD` exists: run PowerShell `[Environment]::GetEnvironmentVariable("HOTSHOT_DB_PASSWORD","Machine")` and confirm not empty.
     - Verify tables created by migrations exist (see validation queries).

2. Non-interactive CI-like run
   - In PowerShell, set env var `SA_CONNECTION_STRING` in session (do NOT commit).
   - Run:
     - `HotshotLogistics.DbSetup --non-interactive --server "127.0.0.1,1433" --sa-cs "%SA_CONNECTION_STRING%" --db-name "hotshot_test_ci" --app-user "hotshot_app"`
   - Expected results: completes without prompts, exit 0, DB and users exist.

3. Idempotence check
   - Re-run the previous command and confirm no changes and success.

4. Insufficient privilege simulation
   - Attempt run with a low-privilege SQL user; expect clear error and no partial objects left behind.

----------------------------------------------------------------
3) Validation queries (parameterized)
----------------------------------------------------------------

All verification queries below MUST be executed using parameterized ADO.NET calls (SqlCommand with `@param`).

A) Verify database existence
- Query:
  - SQL: `SELECT database_id FROM sys.databases WHERE name = @dbName;`
- Parameter:
  - `@dbName` = chosen DB name (e.g., `hotshot_logistics`)
- Expected:
  - Non-empty result (single row) when DB exists.

B) Verify server login existence
- Query:
  - SQL: `SELECT principal_id FROM sys.server_principals WHERE name = @loginName;`
- Parameter:
  - `@loginName` = server login name (e.g., `hotshot_app`)
- Expected:
  - Non-empty result when login exists.

C) Verify database user existence
- Query (connect to target DB)
  - SQL: `SELECT principal_id FROM sys.database_principals WHERE name = @userName;`
- Parameter:
  - `@userName` = database user name (usually same as `@loginName`)
- Expected:
  - Non-empty result.

D) Verify migration-applied (example: table exists)
- Query (connect to target DB)
  - SQL: `SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName;`
- Parameter:
  - `@tableName` = expected table (e.g., `Jobs`, `Customers`)
- Expected:
  - Presence of at least one row for each expected table.

E) Verify permissions for application user
- Query (connect to target DB)
  - SQL: `SELECT HAS_PERMS_BY_NAME(null, null, 'CONNECT');` (returns 1 if CONNECT allowed — this is user-context check)
  - Better: connect using the application user's credentials and run a simple SELECT on a known table to ensure allowed operations.
- Parameter:
  - N/A for the context-based check; for SELECT tests use parameterized SELECT.
- Expected:
  - Application user can SELECT from an application table; cannot perform schema-changing DDL.

F) Verify role membership for migration user (if used)
- Query (connect to target DB)
  - SQL: `SELECT dp.name AS memberName, ro.name AS roleName FROM sys.database_role_members drm JOIN sys.database_principals dp ON drm.member_principal_id = dp.principal_id JOIN sys.database_principals ro ON drm.role_principal_id = ro.principal_id WHERE dp.name = @userName;`
- Parameter:
  - `@userName` = migration user name
- Expected:
  - Migration user is member of `db_owner` during migration if this pattern is used.

----------------------------------------------------------------
4) Clean-up steps (for test and CI teardown)
----------------------------------------------------------------

The CLI should provide a `--cleanup` flag or a separate `cleanup` subcommand for test automation to remove created resources. If not implemented, tests must run explicit cleanup SQL.

Cleanup SQL (execute via privileged connection; all commands must treat identifiers safely with `QUOTENAME`):

- Drop database (if created by test)
  - SQL: `ALTER DATABASE + QUOTENAME(@dbName) + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE'; DROP DATABASE ' + QUOTENAME(@dbName) + ';'`
  - Implementation note: use correct sequence: set SINGLE_USER then DROP.

- Drop database user (connect to target DB)
  - SQL: `DROP USER IF EXISTS ' + QUOTENAME(@userName)`

- Drop server login
  - SQL: `DROP LOGIN IF EXISTS ' + QUOTENAME(@loginName)`

- Revoke elevated rights if applied (example)
  - SQL: `ALTER ROLE db_owner DROP MEMBER ' + QUOTENAME(@migrationUser)`

Important:
- `IF EXISTS` is T-SQL 2016+ syntax; implementer should handle older versions accordingly or perform existence checks before dropping.
- All cleanup must be parameterized and carefully handle quoting via `QUOTENAME` for identifiers.

CI cleanup recommendations
- Prefer ephemeral SQL Server instances for CI (Docker) so teardown is simply stopping/removing the container.
- If using a shared SQL instance, use unique DB names per CI job (e.g., `hotshot_ci_{BUILD_ID}`) and ensure cleanup runs in `finally` blocks.

----------------------------------------------------------------
5) Test data and fixtures
----------------------------------------------------------------

- Provide a test fixture that:
  - Accepts parameters: server, sa-connection-string, db-name, app-user, app-password.
  - Starts an ephemeral SQL Server (optional) or connects to provided instance.
  - Ensures migrations run and returns verification handles for tests.

- Seed data
  - Use the existing migration `20250101001000_SeedInitialData.cs` located in [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1) as canonical seed data. Tests may depend on seed rows (use with caution).

----------------------------------------------------------------
6) Failure modes and expected behaviors
----------------------------------------------------------------

- If CLI fails before making changes:
  - No changes should exist and exit code should be non-zero.
- If CLI fails after creating some artifacts:
  - CLI should attempt compensating cleanup for artifacts it created; tests should assert cleanup success or report remaining artifacts.
- If cleanup fails on CI:
  - Mark the job as failed and notify an operator for manual cleanup; include parameterized diagnostic queries to locate artifacts.

----------------------------------------------------------------
7) Example automated test steps (summary)
----------------------------------------------------------------

- Unit: `dotnet test ./tests/HotshotLogistics.DbSetup.Tests.Unit`
- Integration (local): start SQL container, then:
  - `HotshotLogistics.DbSetup --non-interactive --server "localhost,1433" --sa-cs "%SA_CONNECTION_STRING%" --db-name "hotshot_integration" --app-user "hotshot_app"`
  - Run validation queries from test harness and assert results.
  - Cleanup via CLI `--cleanup` or direct privileged SQL commands.

----------------------------------------------------------------
7) Additional notes for implementer
----------------------------------------------------------------
- Use parameterized ADO.NET for all verification queries and DDL. Avoid building SQL via string concatenation.
- Tests that require elevated privileges should explicitly note they require SA-like credentials and be separated from standard unit test runs. Gate such tests behind an environment variable (e.g., `RUN_DB_INTEGRATION_TESTS=true`) to avoid accidental runs.
- Document exact environment variable names and sample commands in [`6-Docs/specs/dbsetup/cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1) for CI integrators.
