# Implementation Tasks

This document: [`6-Docs/specs/dbsetup/tasks.md`](6-Docs/specs/dbsetup/tasks.md:1)

Overview:
- Tasks below convert the design into implementable work for a C# developer. Each task is actionable and test-driven.

- Owner tags: [dev] developer, [ci] CI/automation engineer, [qa] QA/human

Checklist (ordered)

- [ ] 1. Project skeleton and arg parsing
  - Create a new .NET console project `HotshotLogistics.DbSetup` in an appropriate location (adhere to numbered folder rules).
  - Files to add: `Program.cs`, `ArgumentParser.cs`
  - Owner: [dev]
  - Validation: `dotnet build` succeeds.
  - _Requirements:_ referenced in [`requirements.md`](6-Docs/specs/dbsetup/requirements.md:1)

- [ ] 2. EnvironmentManager implementation
  - Implement `EnvironmentManager` that reads `HOTSHOT_DB_*` environment variables and can set system-level env var with admin consent. Use `Environment.GetEnvironmentVariable` for reads and documented Windows approach for setting system env vars if needed.
  - Owner: [dev]
  - Validation: Unit tests that mock `Environment` calls confirm reads/writes and permission errors are handled.
  - _Requirements:_ secure storage, env var names

- [ ] 3. PasswordManager and policy
  - Implement secure password generator using `RandomNumberGenerator`.
  - Enforce policy: minimum 16 chars, at least 3 character classes, configurable length.
  - Owner: [dev]
  - Validation: Unit tests for policy enforcement.

- [ ] 4. SqlServerProvisioner (core)
  - Implement ADO.NET-based provisioner performing parameterized operations:
    - Check DB existence
    - Create DB if missing
    - Create server login and DB user if missing
    - Verify permissions
  - Owner: [dev]
  - Validation: Integration test against a disposable SQL instance creating resources.
  - _Requirements:_ ADO.NET and parameterized SQL

- [ ] 5. PermissionManager
  - Implement minimal-permissions grants and role membership logic. Provide option to perform migrations using a privileged migration account rather than the runtime app user.
  - Owner: [dev]
  - Validation: Tests that assert user can CONNECT and perform DML.

- [ ] 6. MigrationInvoker integration
  - Integrate invocation of the Migration Runner located at [`4-Persistence/MigrationRunner/Program.cs`](4-Persistence/MigrationRunner/Program.cs:1). Allow running as a subprocess or direct invocation passing the connection string to the target DB.
  - Owner: [dev]
  - Validation: Run migrations end-to-end in an integration test and assert expected tables exist.

- [ ] 7. Logging and secret masking
  - Implement structured logging and secret redaction (mask passwords and connection string password segments).
  - Owner: [dev]
  - Validation: Unit tests ensure logs never contain password values (masking enforced).

- [ ] 8. Interactive prompts and non-interactive flags
  - Implement `--non-interactive` and `--force`; prompt for confirmation in interactive mode for destructive actions.
  - Owner: [dev]
  - Validation: Manual test and unit tests for prompt wrappers.

- [ ] 9. Privilege checks and preflight
  - Implement privilege probing and clear error messages when lacking rights. Probe for `CREATE DATABASE` and `CREATE LOGIN` capability.
  - Owner: [dev]
  - Validation: Simulate insufficient privileges and confirm graceful failure.

- [ ] 10. CI pipeline integration docs and sample pipeline
  - Add documentation and an example pipeline step showing env var usage and running CLI in CI. Use secure pipeline secrets or Key Vault.
  - Owner: [ci]
  - Validation: Pipeline run in test org or local runner.

- [ ] 11. Integration tests and cleanup utilities
  - Implement integration tests that:
    - Start disposable SQL Server (container or LocalDB)
    - Run CLI non-interactively
    - Verify DB and user
    - Tear down resources after test
  - Owner: [dev]/[ci]
  - Validation: Tests pass on PR pipeline.

- [ ] 12. Documentation and README
  - Write user-facing README and add references to `6-Docs/specs/dbsetup`.
  - Owner: [dev]
  - Validation: Review-ready docs.

Estimated complexity and ordering:
- Low: tasks 1,2,3,7,8
- Medium: tasks 4,5,6,9
- High: tasks 11,10

Notes:
- Keep implementation within Clean Architecture placement rules (do not place production infra code in wrong numbered folder).
- Avoid committing secrets; use pipeline variables and Key Vault in CI.

Quick checklist (for update_todo_list usage)
- [ ] Project skeleton and arg parsing
- [ ] EnvironmentManager
- [ ] PasswordManager
- [ ] SqlServerProvisioner
- [ ] PermissionManager
- [ ] MigrationInvoker
- [ ] Logging and masking
- [ ] Interactive & non-interactive UX
- [ ] Privilege preflight
- [ ] CI integration docs
- [ ] Integration tests and teardown
- [ ] README and docs