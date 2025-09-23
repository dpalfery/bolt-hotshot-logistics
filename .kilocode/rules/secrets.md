# secrets.md

Don't check secrets into source control or store them in plain text.

## Guidelines

Secure values that must never be stored in plain text include:

- Connection strings
- Passwords
- PAT tokens
- Bearer tokens
- Tokens of any kind
- API keys
- Client secrets

## Security Best Practices

1. Use Azure AD authentication with role-based access control
2. Validate user inputs on client and server sides
3. Configure CORS properly in production
4. Enforce HTTPS and HSTS for communications
5. Apply least privilege to service accounts
6. Update dependencies regularly to patch vulnerabilities
7. Validate JWT tokens for protected endpoints
8. Store secrets in environment variables or Azure Key Vault
9. Implement comprehensive authentication and authorization
10. Protect against CSRF attacks where applicable
11. Persist and rotate Data Protection API keys for encryption
12. Use rate limiting to prevent abuse