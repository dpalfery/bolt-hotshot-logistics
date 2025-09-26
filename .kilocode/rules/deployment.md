# Deployment Rule

Enforces deployment standards and procedures for Hotshot Logistics, ensuring reliable, secure, and scalable Azure deployments.

## When to Apply

Apply when deploying to Azure environments, managing infrastructure as code, setting up CI/CD pipelines, or implementing rollback/disaster recovery procedures for the Hotshot Logistics platform.

## Environment Management

### Configuration Management Across Environments
- Use Azure App Configuration for centralized, environment-specific settings
- Implement the Options pattern with `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` for binding configurations
- Override settings via `appsettings.{Environment}.json` files and environment variables
- Never hardcode values; use configuration providers for all environment-specific data
- Validate configuration completeness during application startup

### Secret Management Procedures
- Store all secrets (connection strings, API keys, tokens, credentials) in Azure Key Vault
- Use managed identities for secure, passwordless access to Key Vault
- Implement proper RBAC on Key Vault resources
- Rotate secrets regularly and update applications to use new versions
- Never commit secrets to source control or expose them in logs

### Environment-Specific Validation
- Validate database connections, external API endpoints, and service dependencies before deployment
- Test authentication and authorization flows in each environment
- Ensure environment parity by using identical infrastructure configurations
- Implement environment-specific health checks and monitoring
- Document environment differences and access procedures

## Infrastructure as Code Standards

### Terraform/Bicep Implementation
- Use declarative infrastructure definitions with Terraform or Azure Bicep
- Version control all infrastructure code alongside application code
- Implement proper state management with remote state storage (Azure Storage Account)
- Use modules for reusable infrastructure components
- Follow naming conventions and tagging standards for Azure resources

### Azure Resource Standards
- Implement resource groups with clear separation of concerns
- Use consistent naming conventions (e.g., `{project}-{environment}-{resource-type}-{name}`)
- Apply appropriate RBAC and network security groups
- Implement resource locks for production environments
- Use Azure Policy for governance and compliance

### Containerization Standards
- Use Docker for application containerization
- Implement multi-stage builds for optimized images
- Store images in Azure Container Registry with proper tagging
- Use Kubernetes manifests or Helm charts for orchestration
- Implement health checks and resource limits in container configurations

## Deployment Pipeline Procedures

### CI/CD Pipeline Setup
- Implement automated pipelines using GitHub Actions or Azure DevOps
- Trigger builds on pushes to develop/main branches with proper branch protection
- Execute multi-stage pipelines: build → test → security scan → deploy
- Implement environment-specific deployments (dev → staging → prod) with manual approvals
- Generate deployment artifacts and store them securely

### Azure Deployment Procedures
- Use Azure Resource Manager (ARM) templates or Bicep for infrastructure deployment
- Implement blue-green deployments for zero-downtime releases where appropriate
- Use deployment slots for Azure Functions and Web Apps
- Monitor deployment progress and implement automatic rollback on failures
- Validate deployment success through automated smoke tests

### Release Management
- Use semantic versioning for releases
- Maintain release notes and changelogs
- Implement feature flags for gradual rollouts
- Coordinate releases across multiple services and environments
- Document rollback procedures for each release

## Rollback and Disaster Recovery

### Rollback Procedures
- Identify the last stable deployment tag or release
- Execute automated rollback via CI/CD pipelines
- Monitor application health during and after rollback
- Validate functionality through automated tests
- Communicate rollback status to stakeholders

### Disaster Recovery Implementation
- Implement geo-redundancy with Azure Traffic Manager and multi-region deployments
- Configure automated backups for databases and storage accounts
- Test restore procedures regularly with disaster recovery drills
- Implement cross-region failover capabilities
- Document recovery time objectives (RTO) and recovery point objectives (RPO)

### Incident Response Integration
- Integrate with incident response procedures from security guidelines
- Implement automated alerting for deployment failures
- Maintain runbooks for common deployment issues
- Conduct post-mortem reviews for failed deployments
- Update procedures based on lessons learned

## Validation

### Pre-deployment Checks
- Execute full test suite including unit, integration, and performance tests
- Run security scanning (SAST/DAST) and vulnerability assessments
- Validate infrastructure changes with Terraform plan or Bicep what-if
- Check resource availability and quota limits
- Review configuration and secret management

### Post-deployment Validation
- Implement health checks for all deployed services
- Execute smoke tests to validate core functionality
- Monitor application metrics and error rates
- Validate data integrity and service integrations
- Perform user acceptance testing in staging environments

### Monitoring and Alerting Setup
- Configure Azure Monitor and Application Insights for comprehensive observability
- Set up alerts for key performance indicators (response times, error rates, resource utilization)
- Implement structured logging with correlation IDs
- Configure dashboards for real-time monitoring
- Integrate with external monitoring tools if needed

## References

See 6-Docs/deployment-examples.md for detailed implementation examples and Azure deployment patterns.
See [Security Rules](security.md) for secret management and incident response procedures.
See [Process Rules](process.md) for additional deployment workflow guidelines.