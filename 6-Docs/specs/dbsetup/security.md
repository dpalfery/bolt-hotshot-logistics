# Security Specification

This document: [`6-Docs/specs/dbsetup/security.md`](6-Docs/specs/dbsetup/security.md:1)

Purpose
- Document secure handling of credentials, logging, secrets storage, and operational guidance for the DB setup CLI.

References
- Repository security rules (must follow): [`c:\git\bolt-hotshot-logistics\.kilocode\rules\security-general-rule.md`](.kilocode/rules/security-general-rule.md:1)
- Memory bank security guidance: [`c:\git\bolt-hotshot-logistics\.kilocode\rules\memory-bank\tech.md`](.kilocode/rules/memory-bank/tech.md:1)

1. Secret generation and storage
- Canonical secret names (exact; implementer MUST use these):
  - `HOTSHOT_DB_PASSWORD` — application DB user's password (system-level environment variable preferred)
  - `HOTSHOT_DB_SERVER` — optional server fallback
  - `HOTSHOT_DB_NAME` — optional DB name
  - `HOTSHOT_DB_APP_USER` — optional app user name
- Generation policy:
  - Use `System.Security.Cryptography.RandomNumberGenerator` for strong entropy.
  - Default generation: 24 characters containing at least one uppercase letter, one lowercase letter, one digit, and one symbol.
  - Minimum allowed length: 16 characters.
  - Avoid characters that cause shell/SQL parsing issues (e.g., newlines). Encode or escape responsibly when passing in shell contexts.
- Storage policy:
  - CLI MAY set `HOTSHOT_DB_PASSWORD` as a system environment variable only with explicit interactive operator consent.
  - For CI and production, require storing secrets in pipeline secure variables or a secure vault (e.g., Azure Key Vault). The CLI must accept vault references or read environment variables but must not implement persistent vault storage in repository code.
  - The spec disallows storing secrets in files, config checked into repo, or logs.
- How to set system env var on Windows safely (documented as guidance, not performed silently by the tool without consent):
  - PowerShell (requires admin): `[Environment]::SetEnvironmentVariable("HOTSHOT_DB_PASSWORD","<value>","Machine")`
  - setx (note: setx truncates values > 1024): `setx HOTSHOT_DB_PASSWORD "<value>" /M`
  - Document that a new process or logoff/login may be required for processes to see machine-scoped environment variables.

2. Handling SA / privileged credentials
- SA/privileged credentials used by the CLI for provisioning:
  - Must be provided via secure channels for CI (pipeline secrets or Key Vault).
  - If provided interactively, the CLI must NOT store them in disk or repo.
  - Privileged connection strings may be accepted via `--sa-cs` flag or environment variable (implementer should prefer non-env flag for interactive).
- Logging and process exposure:
  - Avoid printing full privileged connection strings in logs or stdout.
  - When launching subprocesses, avoid passing privileged secrets in process arguments (use secured stdin or files with restricted permissions where necessary).

3. Logging policy (masking and retention)
- Masking:
  - Any occurrence of the value of `HOTSHOT_DB_PASSWORD` or any token containing the substrings `password`, `pwd`, or `pass` must be replaced with `REDACTED` before writing to logs.
  - When logging a connection string, show only server, instance, and database name. Replace the password portion with `REDACTED`.
  - Examples:
    - Logged: `Server=localhost\SQLEXPRESS;Database=hotshot_logistics;User Id=hotshot_app;Password=REDACTED;`
- Log retention and location:
  - Default log directory: `%LOCALAPPDATA%\HotshotLogistics\DbSetup\logs\` with file permissions restricted to the current user or service account.
  - Logs should be rotated daily and kept for a configurable retention period (default: 30 days).
  - For CI, logs captured by the pipeline must not contain secrets; the CLI must ensure masking before writing to stdout/stderr captured by the pipeline.
- Audit trail:
  - Log high-level actions: operator identity (Windows user or CI actor), timestamp, actions taken (create-db, create-login, run-migrations), and results. Do not log secrets.

4. Least-privilege and role separation
- Two-account model recommended:
  - `hotshot_migrator` (migration account) — temporarily granted `db_owner` on the target DB to run migrations (if necessary).
  - `hotshot_app` (application account) — runtime account with minimal permissions:
    - CONNECT
    - SELECT, INSERT, UPDATE, DELETE on the application schema(s)
    - EXECUTE as needed
- The CLI should:
  - Create both accounts if requested, or use existing ones without altering passwords unless `--force` used.
  - Grant elevated rights to `hotshot_migrator` only for migration execution and revoke if requested.
  - Document trade-offs for permanent `db_owner` assignment.

5. Password rotation and lifecycle
- Rotation guidance:
  - Document process: generate new password, update login on SQL Server, update `HOTSHOT_DB_PASSWORD` in Key Vault/pipeline, restart services if necessary.
  - CLI may support a `--rotate-password` subcommand in future; spec requires documenting rotation as an operational step.
- Emergency rotation:
  - If compromise suspected, immediately disable the login (ALTER LOGIN ... DISABLE) and follow rotation steps.

6. CI / automation security
- CI must inject privileged credentials and `HOTSHOT_DB_PASSWORD` via pipeline secret variables or Key Vault. Example:
  - GitHub Actions: `secrets.SA_CONNECTION_STRING` and `secrets.HOTSHOT_DB_PASSWORD`
- The CLI must support reading secrets from environment variables only; do not allow secrets via plain text files in repo.
- For ephemeral CI databases, recommend running a SQL Server container bound to the job and using generated passwords stored in job-scoped secrets.

7. Network security and communication
- Use encrypted connections (encrypt=true; TrustServerCertificate=false) where possible.
- Document how to configure connection strings to require encryption and avoid plaintext transmission.
- Recommend restricting SQL Server network access using firewall rules and private networks for production.

8. SQL safety and injection protection
- All SQL executed by the CLI must be parameterized. When dynamic SQL must be used for identifiers, use server-side `QUOTENAME` to escape object names and pass values as parameters for data-containing values (passwords).
- Do not construct SQL by concatenating untrusted input.

9. Secrets exposure risk analysis
- Risk areas and mitigation:
  - Command-line arguments visible in process lists: prefer passing secrets via environment variables or stdin. Document risks for Windows process listings.
  - Logs: mask as defined.
  - Temporary files: do not write secrets to temp files. If temporary storage is necessary, ensure file ACL restricts access and files are deleted securely.

10. Compliance and operational advice
- Recommend storing production credentials in Azure Key Vault or equivalent with RBAC and access policies.
- Implement audit logging and integrate with SIEM for production deployments.
- Follow repository rule: "Never commit connection strings, API keys, tokens, or credentials" — see [`c:\git\bolt-hotshot-logistics\.kilocode\rules\security-general-rule.md`](.kilocode/rules/security-general-rule.md:1).

Appendix: Example masked log entry
- Allowed:
  - `[INFO] 2025-10-02T12:00:00Z - Created login 'hotshot_app' on server 'localhost\SQLEXPRESS' - password stored in HOTSHOT_DB_PASSWORD (REDACTED)`
- Disallowed:
  - `[DEBUG] 2025-10-02T12:00:01Z - Using connection string: Server=localhost;User Id=sa;Password=P@ssw0rd!;`
