[Memory Bank: Active]

# kilocode.status.dbsetup.create-app-user

Date: 2025-10-04

Summary:
- Reviewed dbsetup CLI documentation and memory bank files.
- Recommended CLI invocation and, where necessary, T-SQL fallback to create the application user for the `hotshot_logistic` database with least-privilege access.

Checklist:
- [x] Read memory bank files: [`​.kilocode/rules/memory-bank/brief.md`](.kilocode/rules/memory-bank/brief.md:1), [`​.kilocode/rules/memory-bank/context.md`](.kilocode/rules/memory-bank/context.md:1), [`​.kilocode/rules/memory-bank/tech.md`](.kilocode/rules/memory-bank/tech.md:1)
- [x] Reviewed dbsetup docs: [`6-Docs/specs/dbsetup/cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md:1), [`6-Docs/specs/dbsetup/design.md`](6-Docs/specs/dbsetup/design.md:1), [`6-Docs/specs/dbsetup/security.md`](6-Docs/spec/dbsetup/security.md:1), [`6-Docs/specs/dbsetup/requirements.md`](6-Docs/spec/dbsetup/requirements.md:1), [`6-Docs/specs/dbsetup/tasks.md`](6-Docs/spec/dbsetup/tasks.md:1), [`6-Docs/specs/dbsetup/testing.md`](6-Docs/spec/dbsetup/testing.md:1)
- [x] Drafted recommended CLI invocation for least-privilege app user
- [x] Provided T-SQL fallback statements with placeholders
- [x] Documented security rationale & operational recommendations
- [x] Status file created

Section A — Recommended CLI invocation (documented flags)
- Documented flags from CLI docs: `--server`/`-s`, `--sa-connection-string`/`--sa-cs`, `--db-name`, `--app-user`, `--password`, `--non-interactive`, `--force`, `--persist-env`.
- Recommended non-interactive invocation (use pipeline/Key Vault to supply secrets):

dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj --non-interactive --server "localhost\\SQLEXPRESS" --sa-connection-string "Server=localhost\\SQLEXPRESS;Database=master;User Id=sa;Password=<secure-sa-password-from-vault>;" --db-name "hotshot_logistic" --app-user "hotshot_app" --password "<app-user-password-from-vault>"

Notes:
- Prefer not to use `--persist-env` in shared CI runners. Pass the `--password` value from an injected pipeline secret or Key Vault reference.
- If the CLI supports a `--migrate` or `--run-migrations` flag (see docs), invoke migrations with a dedicated migration identity or via a temporary elevated option; otherwise run migrations with a separate migrator account.

Section B — T-SQL fallback (run by a DBA if CLI cannot grant granular permissions)
-- Create server login (run on master)
CREATE LOGIN [hotshot_app] WITH PASSWORD = '<secure-password-from-vault>';

-- Create database user (run on hotshot_logistic)
USE [hotshot_logistic];
CREATE USER [hotshot_app] FOR LOGIN [hotshot_app];

-- Grant minimal runtime permissions
GRANT CONNECT TO [hotshot_app];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[dbo] TO [hotshot_app];

-- Migration account (create, grant elevated rights, then revoke)
CREATE LOGIN [hotshot_migrator] WITH PASSWORD = '<secure-migration-password-from-vault>';
USE [hotshot_logistic];
ALTER ROLE db_owner ADD MEMBER [hotshot_migrator];

-- After migrations: revoke and remove migrator
USE [hotshot_logistic];
ALTER ROLE db_owner DROP MEMBER [hotshot_migrator];
DROP USER [hotshot_migrator];
USE master;
DROP LOGIN [hotshot_migrator];

Section C — Security rationale & operational recommendations
- Grant only DATA-level permissions to the app user (CONNECT + DML on the application schema) to prevent schema changes from application runtime.
- Use a separate migration account with temporary db_owner/db_ddladmin rights; revoke and remove it after migrations complete.
- Never hardcode passwords in source; retrieve from environment variables or Azure Key Vault (recommend variables HOTSHOT_DB_SA_CREDENTIAL and HOTSHOT_APP_PASSWORD).
- Prefer contained database users if supported to avoid server-level logins exposure; otherwise use least-privilege server login and scoped DB user.
- Rotate credentials regularly and monitor SQL audit logs for provisioning/migration activity.

Next steps:
- Operator to run the recommended CLI invocation in a secure environment (CI pipeline or admin workstation) with secrets injected from Key Vault.
- If the CLI lacks migration orchestration, use the migrator account approach shown in Section B and revoke immediately after use.

Constraints:
- No secrets are included here; all passwords are placeholders.
- This file is the authoritative status record for this task.