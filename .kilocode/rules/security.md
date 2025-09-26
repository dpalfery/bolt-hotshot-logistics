# Security Rule

Enforces security practices for Hotshot Logistics, including secrets management, resilience patterns, and optional security features.

## When to Apply

Apply when implementing security measures, external dependencies, or advanced features in ASP.NET Core applications.

## Secrets Management

- Never check secrets into source control or store them in plain text.
- Secure values include connection strings, passwords, tokens, API keys, and client secrets.
- Use Azure Key Vault or environment variables for secret storage.
- Implement Azure AD authentication with role-based access control.
- Validate user inputs on client and server sides.
- Configure CORS properly in production.
- Enforce HTTPS and HSTS for communications.
- Apply least privilege to service accounts.
- Update dependencies regularly to patch vulnerabilities.
- Validate JWT tokens for protected endpoints.
- Implement comprehensive authentication and authorization.
- Protect against CSRF attacks where applicable.
- Persist and rotate Data Protection API keys for encryption.
- Use rate limiting to prevent abuse.

## Resilience Patterns

- Use Polly for resilience patterns (retry, circuit breaker, timeout).
- Configure policies for database connections and external API calls.
- Define appropriate retry counts and backoff strategies.
- Implement circuit breakers to prevent cascading failures.
- Set reasonable timeouts for all external operations.
- Use `CancellationToken` for cooperative cancellation.
- Configure timeouts at the HttpClient level using `IHttpClientFactory`.
- Implement exponential backoff for transient failures.
- Distinguish between retryable and non-retryable errors.
- Limit retry attempts to prevent resource exhaustion.
- Use circuit breakers to fail fast during prolonged outages.
- Configure appropriate thresholds for opening/closing circuits.
- Implement fallback behaviors when circuits are open.
- Register Polly policies in the DI container.
- Apply policies to HttpClient instances via `IHttpClientFactory`.
- Use typed clients for better testability and configuration.
- Log resilience events for monitoring and debugging.

## Optional Security Features

- Use `Microsoft.AspNetCore.RateLimiting` for rate limiting.
- Implement per IP, user, or key-based limiting.
- Configure limits to prevent abuse.
- Use as security measure.
- Implement output caching for cacheable GET endpoints.
- Use `Microsoft.AspNetCore.OutputCaching`.
- Configure Redis for scale when needed.
- Cache safe responses to improve performance.
- Add health checks for gRPC services.
- Use `Grpc.AspNetCore.HealthChecks` package.
- Implement checks for gRPC endpoints.
- Ensure integration with orchestrators.

## Authentication Implementation Patterns

### Azure AD Integration
- Use `Microsoft.Identity.Web` for Azure AD authentication in ASP.NET Core.
- Configure authentication schemes in `Program.cs` with `AddMicrosoftIdentityWebApi()`.
- Implement token validation middleware for API endpoints.
- Support both user and service principal authentication flows.
- Configure multi-tenant applications when needed.

### JWT Token Management
- Use `Microsoft.AspNetCore.Authentication.JwtBearer` for JWT validation.
- Implement custom JWT handlers for complex scenarios.
- Configure token validation parameters (issuer, audience, lifetime).
- Implement refresh token patterns for long-lived sessions.
- Store tokens securely using HttpOnly cookies or secure storage.

### OAuth 2.0 / OpenID Connect
- Implement authorization code flow for web applications.
- Use PKCE (Proof Key for Code Exchange) for mobile apps.
- Configure identity providers (Azure AD, Google, etc.).
- Handle token refresh and revocation.
- Implement logout functionality with proper session cleanup.

### Multi-Factor Authentication (MFA)
- Enable MFA for high-privilege accounts.
- Implement TOTP (Time-based One-Time Password) validation.
- Support hardware security keys (FIDO2/WebAuthn).
- Configure MFA bypass policies for emergency scenarios.
- Log MFA events for audit trails.

### Custom Authentication Providers
- Implement `IAuthenticationHandler` for custom authentication schemes.
- Create custom identity stores using `IUserStore<TUser>`.
- Support external identity providers through federation.
- Implement password policies and complexity requirements.
- Handle account lockout and password reset flows.

## Authorization Mechanisms and RBAC Setup

### Role-Based Access Control (RBAC)
- Define roles in Azure AD or custom role stores.
- Implement role-based authorization using `[Authorize(Roles = "Admin")]` attributes.
- Create hierarchical role structures (Admin > Manager > User).
- Support dynamic role assignment through user management.
- Implement role claims in JWT tokens.

### Policy-Based Authorization
- Use `IAuthorizationRequirement` and `AuthorizationHandler<T>` for custom policies.
- Implement resource-based authorization for fine-grained access control.
- Create policies for business rules (e.g., "CanEditOwnJobs", "CanViewCustomerData").
- Support claim-based authorization with custom claim types.
- Implement requirement handlers for complex authorization logic.

### Claims-Based Authorization
- Extend user identities with custom claims.
- Implement claim transformation during authentication.
- Use claims for attribute-based access control (ABAC).
- Support claim inheritance and delegation.
- Validate claims in authorization policies.

### API Authorization Patterns
- Implement scope-based authorization for APIs.
- Use OAuth 2.0 scopes to control API access.
- Support delegated permissions for service-to-service calls.
- Implement API key authentication for external integrations.
- Configure CORS policies per API endpoint.

### Permission Management
- Create permission enums or constants for granular control.
- Implement permission checking in application services.
- Support permission inheritance from roles.
- Cache permissions for performance optimization.
- Audit permission changes and access attempts.

## Security Testing Procedures

### Static Application Security Testing (SAST)
- Integrate security scanning tools (SonarQube, Checkmarx) in CI/CD pipeline.
- Scan for common vulnerabilities (OWASP Top 10).
- Review code for insecure patterns (SQL injection, XSS, CSRF).
- Implement security code review checklists.
- Fix critical and high-severity findings before deployment.

### Dynamic Application Security Testing (DAST)
- Perform automated security scanning on running applications.
- Use tools like OWASP ZAP or Burp Suite for vulnerability detection.
- Test API endpoints for injection attacks and authentication bypass.
- Scan for misconfigurations and exposed sensitive data.
- Conduct authenticated and unauthenticated scanning.

### Penetration Testing
- Engage certified security professionals for comprehensive testing.
- Test authentication mechanisms and session management.
- Attempt privilege escalation and data exfiltration.
- Validate incident response procedures.
- Document findings and remediation plans.

### Security Unit Testing
- Write unit tests for authentication and authorization logic.
- Test password policies and validation rules.
- Validate JWT token handling and claims.
- Test authorization policies and requirements.
- Mock security dependencies for isolated testing.

### Dependency Vulnerability Scanning
- Use tools like OWASP Dependency-Check or Snyk.
- Scan NuGet packages and npm dependencies regularly.
- Implement automated vulnerability alerts.
- Update dependencies to patch known vulnerabilities.
- Maintain a software bill of materials (SBOM).

### Configuration Security Testing
- Validate Azure Key Vault and App Configuration security.
- Test secret rotation procedures.
- Verify CORS and security headers configuration.
- Check for exposed sensitive data in logs and error messages.
- Validate HTTPS and certificate configurations.

## Threat Modeling Guidelines

### STRIDE Framework
- **Spoofing**: Identify authentication and authorization weaknesses.
- **Tampering**: Protect data integrity in transit and at rest.
- **Repudiation**: Implement audit logging for non-repudiation.
- **Information Disclosure**: Prevent unauthorized data access.
- **Denial of Service**: Implement rate limiting and resource protection.
- **Elevation of Privilege**: Validate authorization controls.

### Threat Modeling Process
1. **Define Scope**: Identify system boundaries and trust zones.
2. **Create Architecture Diagram**: Document components and data flows.
3. **Identify Assets**: List valuable data and resources to protect.
4. **Identify Threats**: Use STRIDE to enumerate potential threats.
5. **Identify Vulnerabilities**: Map threats to specific weaknesses.
6. **Determine Mitigations**: Design security controls and countermeasures.
7. **Validate Model**: Review and update threat model regularly.

### DREAD Risk Assessment
- **Damage Potential**: Impact if threat is realized.
- **Reproducibility**: How easy is it to exploit the vulnerability.
- **Exploitability**: Technical difficulty of exploitation.
- **Affected Users**: Number of users impacted.
- **Discoverability**: How easily can the vulnerability be found.

### Threat Modeling Tools
- Use Microsoft Threat Modeling Tool for structured analysis.
- Create data flow diagrams (DFDs) for system visualization.
- Document threat models in architecture documentation.
- Review threat models during design and code reviews.
- Update models when architecture changes.

### Common Threat Scenarios
- API authentication bypass through parameter manipulation.
- SQL injection through improperly parameterized queries.
- Cross-site scripting (XSS) in web interfaces.
- Insecure direct object references (IDOR).
- Server-side request forgery (SSRF) in external integrations.
- Race conditions in concurrent operations.

## Incident Response Procedures

### Incident Detection and Classification
- Implement monitoring and alerting for security events.
- Use Azure Monitor and Application Insights for anomaly detection.
- Classify incidents by severity (Critical, High, Medium, Low).
- Establish response time SLAs based on incident severity.
- Document incident classification criteria.

### Incident Response Team
- Define roles: Incident Response Coordinator, Technical Lead, Communications Lead.
- Maintain 24/7 contact information for response team members.
- Conduct regular incident response training and simulations.
- Establish escalation procedures for different incident types.
- Coordinate with external parties (law enforcement, regulators) when needed.

### Containment and Eradication
- Isolate affected systems to prevent spread.
- Preserve evidence for forensic analysis.
- Remove malicious code or configurations.
- Patch vulnerabilities exploited in the incident.
- Restore systems from clean backups when possible.

### Recovery and Lessons Learned
- Validate system integrity before returning to production.
- Monitor systems closely during recovery phase.
- Document incident timeline and response actions.
- Conduct post-mortem analysis to identify root causes.
- Update security controls and procedures based on lessons learned.

### Communication Procedures
- Notify affected customers and stakeholders promptly.
- Coordinate public communications through designated channels.
- Maintain incident status updates during response.
- Document communications for regulatory compliance.
- Preserve confidentiality of sensitive incident details.

### Legal and Regulatory Compliance
- Report incidents to relevant authorities as required.
- Preserve evidence for potential legal proceedings.
- Comply with data breach notification laws (GDPR, CCPA).
- Document incident response for audit purposes.
- Update incident response plan based on regulatory changes.

## References

See 6-Docs/security-examples.md for detailed implementation examples.