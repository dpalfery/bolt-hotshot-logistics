# Design Document

This document: [`6-Docs/specs/dbsetup/design.md`](6-Docs/specs/dbsetup/design.md:1)

## Overview
The DB Setup CLI is a Multi-platform (Windows, macOS, CI/Linux containers) .NET 8+ cross-platform console application that automates provisioning of the Hotshot Logistics SQL Server database and an application account, and invokes FluentMigrator to apply schema migrations. The tool is idempotent and supports both interactive developer workflows and non-interactive CI runs.

Key implementation artifacts described here will be implemented in a new project (example name: `HotshotLogistics.DbSetup`) and will interact with existing migration code under [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1) and the migration runner at [`4-Persistence/MigrationRunner/Program.cs`](4-Persistence/MigrationRunner/Program.cs:1).

## CLI application structure (proposed)
- `Program.cs` — entry point; bootstraps components and dispatches commands. (create in new project)
- `ArgumentParser.cs` — parse CLI flags and environment fallbacks.
- `EnvironmentManager.cs` — read/write environment variables safely using `System.Environment`. Responsible for detecting existing `HOTSHOT_DB_*` vars and offering to set system env var when appropriate.
- `PasswordManager.cs` — secure password generation and validation using `System.Security.Cryptography.RandomNumberGenerator`.
- `SqlServerProvisioner.cs` — ADO.NET-based component that performs parameterized T-SQL operations for checking/creating database, logins, and users.
- `PermissionManager.cs` — calculates and applies the minimal permission set for the application user and optionally grants migration-time elevated permissions to a migration account.
- `MigrationInvoker.cs` — runs the existing FluentMigrator runner, e.g., launching [`4-Persistence/MigrationRunner`](4-Persistence/MigrationRunner/Program.cs:1) as a subprocess or invoking library method if available.
- `Logger.cs` — structured logger with secret-masking capabilities.
- `PreflightChecker.cs` — probes privileges, SQL Server reachability, and required external tools.
- `InteractivePrompter.cs` — used only in interactive mode to confirm destructive actions.

All database verification and changes must use native ADO.NET (`Microsoft.Data.SqlClient`) with parameterized commands. No ORM usage is allowed.

## Components and responsibilities (brief)
- ArgumentParser
  - Flags: `--server`, `--sa-cs`, `--db-name`, `--app-user`, `--password`, `--non-interactive`, `--force`.
  - Fallback priority: CLI flag → environment variable → interactive prompt → generated default.

- EnvironmentManager
  - Reads `HOTSHOT_DB_SERVER`, `HOTSHOT_DB_NAME`, `HOTSHOT_DB_APP_USER`, `HOTSHOT_DB_PASSWORD` using cross-platform .NET APIs (`System.Environment`).
  - Supports an explicit flag `--persist-env` (or `--persist-environment`) that, when provided in interactive mode, attempts to persist the chosen `HOTSHOT_DB_PASSWORD` in a platform-appropriate way (see Cross-platform considerations). The CLI MUST NOT persist secrets without explicit consent.
  - In non-interactive/CI usage the CLI will prefer pipeline secrets or Key Vault references; it will read environment variables but will not attempt to create persistent OS-level variables in CI.
  - When persistence cannot be performed (permission denied or unsupported environment), the CLI should fall back to keeping the secret in the current process environment and print concise, secure instructions for the user to set the secret manually in their environment or pipeline.

- PasswordManager
  - Generates a cryptographically random password when none provided.
  - Policy: default 24 characters, includes upper, lower, digits, and symbols. Enforce at least 16 chars minimum.
  - Does NOT keep plaintext longer than necessary; mask when persisted.

- SqlServerProvisioner
  - Uses `SqlConnection` and `SqlCommand` with `@param` placeholders.
  - Core operations (all parameterized):
    - Check database existence:
      - Example query: SELECT database_id FROM sys.databases WHERE name = @dbName
    - Create database:
      - Example query: CREATE DATABASE [dbName] (note: dbName passed as parameter for use in dynamic T-SQL via QUOTENAME when needed)
    - Create login (server-level):
      - Example query pattern (parameterized): CREATE LOGIN [loginName] WITH PASSWORD = @password; (implementation must use safe escaping with QUOTENAME for identifiers, and parameter for password)
    - Create user in database:
      - Example: CREATE USER [userName] FOR LOGIN [loginName];
    - Grant permissions as per PermissionManager.
  - When multiple steps are required across server and database scope, the provisioner should coordinate transactions where possible. Note: CREATE LOGIN and CREATE DATABASE cannot be wrapped in a single T-SQL user transaction that spans server-level and database-level actions; therefore implement compensating rollback logic in code to remove created artifacts on failure.

- PermissionManager
  - Recommended minimal permission set (see Permissions section).
  - Option: create a dedicated `migration` user with elevated permissions (db_owner on target database) to run migrations; after migration, remove elevated rights and leave only minimal privileges to the application user.

- MigrationInvoker
  - Invokes the migration runner with a secure connection string that uses the migration account (or the SA-provided credentials).
  - Example invocation: run the project [`4-Persistence/MigrationRunner/Program.cs`](4-Persistence/MigrationRunner/Program.cs:1) as a subprocess passing `--connection-string` (ensure password masked in process listing or use secure channel).
  - Prefer in-process invocation if the migration runner is available as a library to avoid command-line exposure of secrets.

- Logger
  - Structured logging (e.g., use `Serilog` or built-in `ILogger`) with secret masking rules:
    - Any message containing `password`, `pwd`, `connection string`, or exact value from `HOTSHOT_DB_PASSWORD` must be redacted.
    - Log levels: Info for high-level steps, Warning for recoverable issues, Error for fatal steps.
    - Do NOT log full connection strings in Info logs; log only host, instance, and database name with password masked.

## Data flow scenarios

### Scenario A — Fresh machine (no DB and no env var)
1. Operator runs CLI interactively:
   - `ArgumentParser` collects flags; `EnvironmentManager` reads missing env vars.
   - `PasswordManager` generates a secure password (or prompts for one).
   - `PreflightChecker` verifies SA or privileged connection via `--sa-cs` or prompts for privileged credentials.
   - `PreflightChecker` verifies the current user has privileges (or SA credentials will be used).
   - `SqlServerProvisioner`:
     - Creates database if missing.
     - Creates login at server level with generated password.
     - Creates user in the new database and assigns minimal permissions.
   - `MigrationInvoker` runs FluentMigrator to apply migrations from [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1).
   - `EnvironmentManager` offers to persist the password to the system environment variable `HOTSHOT_DB_PASSWORD` (with instructions, or performs the set if consent provided).
   - `Logger` emits summary log with masked secrets.
2. CLI exits with success (exit code 0).

### Scenario B — Existing DB or existing login (idempotent path)
1. Operator runs CLI (interactive or CI) with same parameters:
   - `SqlServerProvisioner` checks for database and login existence.
   - If DB exists, verify that migration history indicates latest migrations applied; if not, run `MigrationInvoker`.
   - If login exists and password not supplied, do not change password unless `--force` provided.
   - If `--password` provided, optionally update login password only when `--force` is specified (explicit).
   - All checks are read-only when no change requested; tool exits 0 when no changes required.

## Security design

- Secrets handling
  - The canonical storage location for the application DB password in this project is the system environment variable `HOTSHOT_DB_PASSWORD`.
  - For production/CI, prefer pipeline secret variables or a Key Vault (e.g., Azure Key Vault). The CLI must accept references to vault secrets but must not implement vault storage directly; instead, document recommended patterns.
  - The CLI MAY set a system environment variable only with explicit interactive consent, or it MAY output instructions for administrators to set the var manually:
    - Windows setx (with consequences): document how to set system-level variables using PowerShell or `setx` and warn that a logoff/login may be required.
    - Prefer guidance: "Store the generated password in your CI secret store or Key Vault; do not persist to disk."

- Password generation
  - Use `System.Security.Cryptography.RandomNumberGenerator` to create base64 or charset-limited strings.
  - Default: 24-character password with at least one uppercase, one lowercase, one digit, and one symbol.

- Logging policy
  - Mask secrets in all logs. Example mask pattern: replace password value with `****` or `REDACTED`.
  - Never write full connection strings to logs. Only log connection metadata: server, instance, database name, and user (not password).
  - Error logs that might contain SQL errors should be sanitized to remove user-supplied sensitive content before persistence.

- Auditing
  - Log actions (create-db, create-login, run-migrations) with timestamps and operator identity (Windows username or CI actor) but never include secrets.
  - Recommended to pipe audit logs to a centralized logging solution that enforces retention and access controls.

## SQL design (high-level examples)
- All queries must use parameterized `SqlCommand` objects.
- Avoid string concatenation for user inputs. Use `QUOTENAME` for identifiers when building DDL that must embed object names.

Examples (pseudocode snippets — not full implementations):

- Check DB existence (parameterized)
  - SQL: `SELECT database_id FROM sys.databases WHERE name = @dbName;`
  - Use a parameter `@dbName`.

- Create login (parameterized for password)
  - SQL (identifier handling required):
    - Use `EXEC('CREATE LOGIN ' + QUOTENAME(@loginName) + ' WITH PASSWORD = @pwd')`
    - Note: When dynamic SQL required for identifiers, pass identifiers separately and `QUOTENAME` them in the SQL executed on server side. Password must be a parameter, not interpolated in text.

- Create user in DB
  - Connect to target DB and run:
    - `CREATE USER [userName] FOR LOGIN [loginName];` (userName/loginName must be identifier-escaped via `QUOTENAME`)

- Grant permissions
  - Example: Grant connect and specific DML rights on schema objects. To run migrations, you may grant `db_owner` to the migration account temporarily then revoke.

Transaction/rollback approach:
- Use database transactions for DB-scoped changes where possible (e.g., creating objects inside the DB).
- For server-scoped actions (CREATE LOGIN), implement compensating actions: if later steps fail remove the created login and drop DB if it was created by this run and left in inconsistent state.
- Maintain an in-memory operation log of created artifacts to allow best-effort cleanup on failure.

## Permissions model (recommendation)
- Principle: grant the least privilege necessary.
- Two-account model recommended:
  1. `hotshot_migrator` (optional): Used to run migrations. Granted `db_owner` on the target database while migrations run. Immediately after migration, revoke `db_owner` if possible.
  2. `hotshot_app` (application user): Minimal runtime permissions:
     - CONNECT to database
     - SELECT, INSERT, UPDATE, DELETE on application schemas
     - EXECUTE on stored procedures if used
     - If schema modification is required at runtime (not recommended), document that additional permissions are needed.
- Justification: Migrations often need elevated rights; the app runtime should not run with db_owner to reduce attack surface.

Minimal permission SQL examples (apply within target database):
- Grant CONNECT:
  - `GRANT CONNECT TO [hotshot_app];`
- Grant DML on schema:
  - `GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[dbo] TO [hotshot_app];`
- Grant EXECUTE on procedure collection if used:
  - `GRANT EXECUTE ON SCHEMA::[dbo] TO [hotshot_app];`

Trade-offs:
- Using `db_owner` simplifies migration but increases risk if the account is compromised. The two-account pattern balances safety with flexibility.

## Error handling and rollback
- Fail early on preflight checks: connectivity, credentials, and privileges.
- When an operation fails mid-run:
  - Log the failure (masking secrets).
  - Attempt compensating actions in reverse order for created artifacts (drop user, drop login, drop DB) only for artifacts created by this run.
  - If compensating rollback fails, present clear remediation steps for an operator.
- Exit codes: define (see [`cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1)).

## Privilege escalation and checks

- Database-privileges (OS-neutral)
  - Detect the current DB identity and test for ability to:
    - CREATE DATABASE
    - CREATE LOGIN
    - ALTER ANY LOGIN (if needed)
  - Creating server-level logins or database-level principals requires a SQL Server account with sufficient privileges (e.g., `sa` or an account granted ALTER ANY LOGIN / CREATE DATABASE). The CLI MUST accept privileged DB credentials via `--sa-cs` / `--sa-connection-string` or environment variables for non-interactive runs.
  - Mandatory verification before any state change:
    - The CLI MUST validate privileged DB credentials by performing a harmless verification query and MUST log a masked summary of the result before making changes, for example:
      - `SELECT SERVERPROPERTY('ProductVersion');`
      - Or another lightweight call that confirms connectivity and authentication.
    - If verification fails, the CLI MUST abort and emit actionable remediation (required privileges, how to supply `--sa-cs`, or how to inject secrets via CI).
  - Do not rely on OS-level "run as Administrator" semantics for SQL Server privileges. On macOS and Linux there is no equivalent; privileged actions must be performed using privileged DB credentials supplied to the CLI.

- OS-level privilege escalation for environment persistence
  - Windows
    - Machine-level persistent example (requires elevation):
      - `setx HOTSHOT_DB_PASSWORD "<password>" /M`
    - PowerShell machine-level example (requires elevation):
      - `[Environment]::SetEnvironmentVariable("HOTSHOT_DB_PASSWORD","<password>","Machine")`
    - Notes:
      - `setx /M` and machine-level PowerShell writes require Administrator elevation and may require a new logon/session before processes see the change.
      - The CLI MUST request explicit user confirmation and be invoked with `--persist-env` prior to attempting any persistent write; log intended action with the password masked.
  - macOS / Linux
    - Session-only (temporary):
      - `export HOTSHOT_DB_PASSWORD='<password>'`
    - Persistent for interactive shells:
      - Append `export HOTSHOT_DB_PASSWORD='<password>'` to `~/.zshrc` or `~/.bashrc`.
    - macOS GUI session caveat:
      - `launchctl setenv HOTSHOT_DB_PASSWORD '<password>'` sets a GUI-session value that may not survive reboots/session restarts; document this limitation.
    - Guidance: Prefer shell-profile edits for terminal workflows or pipeline/Key Vault for CI.
  - CI / production
    - DO NOT attempt to persist environment variables into runner OS from code. Use pipeline secret injection (GitHub Actions, Azure Pipelines, GitLab CI) or Key Vault.
    - On Linux CI runners, `System.Environment` writes persist only for the process lifetime; use pipeline-level secrets to share across steps.
  - Persistence policy
    - The CLI MUST only attempt persistent writes when `--persist-env` is provided and the operator confirms. Default behavior MUST NOT persist secrets. If persistence fails/denied, fall back to process-scoped env and print secure manual instructions.

- Failure behavior
  - If privileged DB verification fails, the CLI MUST fail fast with a clear message describing required DB privileges and how to supply them (flags, env vars, or pipeline secrets).
  - If an attempt to persist an environment variable is denied due to insufficient OS privileges, the CLI MUST fall back to process-scoped environment and print secure, platform-specific instructions for manual persistence or pipeline secret usage.
## Cross-platform and OS-specific considerations

- OS detection
  - The CLI SHALL detect runtime OS using .NET APIs such as:
    - `System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.XXX)` for branching behavior when necessary.
    - `System.Runtime.InteropServices.RuntimeInformation.OSDescription` for human-readable OS details in diagnostics and logs.
    - Use `System.Environment` for reliable environment access across platforms.
  - Branch behavior only when strictly necessary; prefer platform-agnostic approaches.

- Environment variable persistence (only when `--persist-env` requested)
  - Windows (examples and guidance)
    - Machine-level persistent example (requires elevation):
      - `setx HOTSHOT_DB_PASSWORD "<password>" /M`
      - Note: `/M` writes machine-level variables and requires Administrator elevation. Immediate processes may not see the change until a new logon/session.
    - Per-user example:
      - `setx HOTSHOT_DB_PASSWORD "<password>"`
    - PowerShell (requires elevation for machine-level):
      - `[Environment]::SetEnvironmentVariable("HOTSHOT_DB_PASSWORD","<password>","Machine")`
    - Guidance:
      - Always warn the operator that persistent writes require OS privileges and may affect other applications. Require explicit confirmation before performing persistent writes.
  - macOS / Linux (examples and guidance)
    - Session-only (temporary):
      - `export HOTSHOT_DB_PASSWORD='<password>'`
    - Persistent for interactive shells:
      - Add `export HOTSHOT_DB_PASSWORD='<password>'` to the user's shell profile (e.g., `~/.zshrc`, `~/.bashrc`).
    - macOS GUI session caveat:
      - `launchctl setenv HOTSHOT_DB_PASSWORD '<password>'` sets the value for the current user GUI session but is session-limited and may not survive reboots — document these caveats.
    - Guidance:
      - Programmatic persistence may be fragile across macOS GUI sessions; prefer shell-profile edits for terminal usage or pipeline/vault for CI.
  - CI / Linux containers
    - DO NOT attempt to persist environment variables into the runner OS from code. Pipeline-level secret injection is the correct mechanism (e.g., GitHub Actions secrets, Azure Pipelines variables, GitLab CI variables).
    - Process environment variables on Linux CI persist only for the process lifespan; use pipeline secret facilities to share secrets across steps.
  - CLI persistence policy
    - The CLI MUST only attempt to persist environment variables when `--persist-env` is explicitly provided and the operator confirms the action interactively.
    - Default behavior: DO NOT persist secrets; keep secrets in-process for the current run and print secure platform-specific instructions for manual persistence or pipeline secret configuration.
    - If the CLI cannot perform persistence due to permission errors, it must fall back to process-only env and emit clear manual instructions.

- Privilege model (OS-neutral)
  - Creating SQL Server logins/users requires privileged DB credentials (SA or equivalent); these are independent of the OS. The CLI must accept privileged DB credentials and should not rely on OS user privileges for DB operations.
  - The CLI MUST verify privileged credentials with a lightweight verification query (e.g., `SELECT SERVERPROPERTY('ProductVersion')`) before making changes and fail fast with actionable guidance if verification fails.

- Docker guidance (macOS / Linux / CI)
  - Concrete Docker run example:
    - docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong!Passw0rd' -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest
  - CI-friendly example using pipeline secret:
    - docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=${{ secrets.CI_SQL_SA_PASSWORD }}' -p 1433:1433 --name ci-hotshot-sql -d mcr.microsoft.com/mssql/server:2019-latest
  - Health-check recommendation:
    - Wait for TCP port 1433 to accept connections, then run a verification query in a retry loop until it succeeds before proceeding. Example health loop (pseudo-bash):
      - while ! nc -z localhost 1433; do sleep 1; done
      - Then attempt an authenticated `SELECT 1` or `SELECT SERVERPROPERTY('ProductVersion')`.
  - Mapping and networking:
    - Map host port 1433 to container port 1433 or use container networking. Ensure the CLI uses the mapped host/port in the connection string.

- Connectivity diagnostics
  - The CLI should:
    1. Attempt a TCP connect to host:port.
    2. If TCP succeeds, attempt an authenticated verification query.
    3. If either step fails, print a masked diagnostic (host, port, error) with clear next steps and a Docker `docker run` example for local troubleshooting.

- Cross-platform implementation guidance
  - Use .NET APIs for OS detection and environment operations (`System.Environment`, `RuntimeInformation`). Avoid shelling out to platform-specific commands for core functionality.
  - If a platform shell command is unavoidable (e.g., to set machine-level variables on Windows), encapsulate the call, mark it optional, and require explicit user consent. Provide robust fallbacks (process-only secrets + printed manual instructions) when persistence is not possible.
  - Always sanitize and mask secrets before printing or logging any diagnostic.

- Logging / UX
  - Use plain stdout/stderr and structured logs (JSON) for CI (`--log-format json`) where appropriate.
  - Avoid Windows-only console APIs. Detect non-TTY environments and avoid interactive prompts in those contexts.
  - Always mask secrets in logs and never print full connection strings; log only server/instance/database name and user (password redacted).


## CI considerations
- CI pipelines MUST inject privileged credentials as pipeline secrets or Key Vault references; never hardcode credentials in YAML or commit them into the repo.
- Recommended pattern: start a SQL Server container as part of the CI job and pass the SA password via the pipeline secret store:
  - Example (Linux runners):
    - docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=${CI_SQL_SA_PASSWORD}" -p 1433:1433 --name ci-hotshot-sql -d mcr.microsoft.com/mssql/server:2019-latest
  - Wait for readiness before executing the CLI (use a retry loop that attempts a lightweight SQL connection).
- The CLI MUST support fully non-interactive mode:
  - Flags: `--sa-cs` (or `--sa-connection-string`), `--non-interactive`, `--db-name`, `--app-user`, and `--password` (or read `HOTSHOT_DB_PASSWORD` from env).
  - Support reading privileged DB credentials from environment variables such as `CI_SQL_SA_PASSWORD` or a complete `CI_SA_CONNECTION_STRING`.
- For Linux CI runners, remember `System.Environment` changes persist only for the process lifetime — use pipeline secrets rather than trying to persist OS-level environment variables.
- Provide example pipeline snippets in [`cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1) showing how to:
  - Start the container.
  - Wait for readiness.
  - Run the CLI in non-interactive mode with secrets injected from variables.
  - Tear down the container after tests.

## Acceptance criteria for cross-platform
- The implementer MUST verify the CLI on the three target environments:
  1. Windows (developer machine): Run interactive and non-interactive flows against a local or hosted SQL Server instance (including named instance scenarios). Confirm environment persistence via `setx` if `--persist-env` used and document required elevation steps.
  2. macOS (developer machine): Run CLI against Docker-hosted SQL Server or Azure SQL. Confirm non-persistent process-level environment variable behavior and validate documented manual persistence steps (shell profile or `launchctl`) are sufficient.
  3. CI/Linux container runner: Run full setup using a SQL Server container launched within the pipeline. CLI must run non-interactively with SA credentials passed via pipeline secrets and complete migrations successfully.
- Tests:
  - Automated integration test that runs migrations end-to-end against a Docker-hosted SQL Server on Linux.
  - Unit tests covering OS-branch logic for environment persistence (mocks) and connectivity failure messaging.
  - A smoke test that performs the privileged-credential verification query (e.g., SELECT SERVERPROPERTY('ProductVersion')) and fails fast if verification fails.
- Observability:
  - The CLI must emit structured logs suitable for CI consumption (optionally JSON) and mask secrets in all outputs.
- Security:
  - No plaintext secrets committed to repo; CLI must support Key Vault/pipeline secret references and document them.
- Documentation:
  - Update `cli-usage.md` with platform-specific run examples (Windows, macOS Docker, CI container) and the `--persist-env` behavior and implications.

## Implementation notes for developer
- Place the new project under a logical folder that respects the repository's numbered architecture (e.g., `7-Deployment/tools/HotshotLogistics.DbSetup` or `0-Base/tools` depending on organization policy). Consult `c:\git\bolt-hotshot-logistics\.kilocode\rules\architecture-general.md` for placement rules.
- Use `Microsoft.Data.SqlClient` for ADO.NET access; ensure all commands are awaited and disposed with `await using`.
- Add unit tests (mock ADO.NET via wrappers or use test SQL instances) and integration tests that run the CLI end-to-end with an ephemeral SQL Server.

## References
- Migrations location: [`4-Persistence/HotshotLogistics.Data/Migrations`](4-Persistence/HotshotLogistics.Data/Migrations:1)
- Migration runner: [`4-Persistence/MigrationRunner/Program.cs`](4-Persistence/MigrationRunner/Program.cs:1)
- Environment variable names referenced in design: `HOTSHOT_DB_PASSWORD`, `HOTSHOT_DB_SERVER`, `HOTSHOT_DB_NAME`, `HOTSHOT_DB_APP_USER` (documented in [`cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1)).