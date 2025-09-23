# secrets.md

don't check secrets of anykind into source control. don't store secrets in plane text anywere

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

1. Always use Azure AD authentication with proper role-based access control
2. Validate all user inputs on both client and server sides
3. Implement proper CORS configuration in production environments
4. Enforce HTTPS and HTTP Strict Transport Security (HSTS) for all communications
5. Follow the principle of least privilege for all service accounts
6. Regularly update dependencies to patch security vulnerabilities
7. Implement proper JWT token validation for protected endpoints
8. Store all secrets in environment variables or Azure Key Vault
9. Implement comprehensive authentication (authN) and authorization (authZ) mechanisms
10. Protect against Cross-Site Request Forgery (CSRF) attacks where relevant
11. Persist and regularly rotate Data Protection API keys for data encryption
12. Use rate limiting as a security measure to prevent abuse and attacks