name: "Security-General-Rule"
description: "Enforces fundamental security practices that apply across all development activities in Hotshot Logistics."
when-to-apply: "always"
rule: |

## Secrets Management

- Never check secrets into source control or store them in plain text.
- Secure values include connection strings, passwords, tokens, API keys, and client secrets.
- Use secure providers (Azure Key Vault, environment variables) for secret storage.
- Implement proper access controls with RBAC and least privilege principles.
- Validate user inputs on client and server sides.
- Configure security properly in production environments.
- Enforce secure communications (HTTPS, HSTS).
- Update dependencies regularly to patch vulnerabilities.
- Implement comprehensive validation and authorization.
- Use rate limiting to prevent abuse.

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

### Common Threat Scenarios
- Authentication bypass through parameter manipulation.
- Injection attacks through improper input handling.
- Cross-site scripting (XSS) in web interfaces.
- Insecure direct object references (IDOR).
- Server-side request forgery (SSRF) in external integrations.
- Race conditions in concurrent operations.

## Incident Response Procedures

### Incident Detection and Classification
- Implement monitoring and alerting for security events.
- Classify incidents by severity (Critical, High, Medium, Low).
- Establish response time SLAs based on incident severity.
- Document incident classification criteria.

### Incident Response Team
- Define roles: Incident Response Coordinator, Technical Lead, Communications Lead.
- Maintain 24/7 contact information for response team members.
- Conduct regular incident response training and simulations.
- Establish escalation procedures for different incident types.

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

## References

- [Backend Security Rules](../backend/security-backend.md) - .NET-specific security implementation
- [Frontend Security Rules](../frontend/security-frontend.md) - Frontend-specific security practices
- [CI/CD Security Rules](../cicd/security-cicd.md) - Security scanning and validation in pipelines