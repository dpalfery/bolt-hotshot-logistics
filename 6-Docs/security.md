---
id: security-guidelines
title: Security guidelines
doc-type: reference
status: draft
owner: unassigned
last-reviewed: 2026-08-14
---

# Security Guidelines

Never check secrets of any kind into source control. Never store secrets in plain text anywhere. The agent-facing rule is [security handling](rules/security-handling.md). GitHub's policy file is [SECURITY.md](../SECURITY.md).

## Secure Values

Values that must never be stored in plain text include:

- Connection strings
- Passwords
- PAT tokens
- Bearer tokens
- Tokens of any kind
- API keys
- Client secrets

## Security Best Practices

1. Use Azure AD authentication with role-based access control
2. Validate all user inputs on client and server
3. Configure CORS for production
4. Use HTTPS for API traffic
5. Least privilege for service accounts
6. Keep dependencies updated
7. Validate JWT tokens on protected endpoints
8. Store production secrets in Azure Key Vault; local API configuration uses .NET user secrets; dashboard public Azure AD ids come from the process environment. Do not write secrets into the repository tree, including `.env` files.

## Secret Management

- Azure Key Vault and App Configuration for Azure
- .NET user secrets for local API development
- Process environment for dashboard `NEXT_PUBLIC_*` values (generated `.env.local` must stay gitignored)
- Placeholders only in example configuration
- Rotate secrets when they leak
