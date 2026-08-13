# Clean Architecture Guidelines

The Hotshot Logistics project follows Clean Architecture principles with a numbered folder structure to enforce separation of concerns and dependency direction.

## Key Architectural Decisions
- Dependency injection throughout
- CQRS for command/query separation
- Repository pattern for data access
- Service layer for business logic
- Strict layering prevents circular dependencies
- Testable design with mocked dependencies

## File Placement Guidelines

When creating, moving, or adding files, place them in the appropriate numbered folder based on their purpose and architectural layer:

### 0-Base/
**Foundational code and abstractions shared across layers**
- Base classes, interfaces, utilities, extensions
- Cross-cutting concerns (logging, error handling, result types)
- Shared constants, enums, extension methods
- No dependencies on higher layers

### 1-Presentation/
**Presentation layer projects and components**
- API controllers, functions, endpoints (Azure Functions)
- Web dashboard components (Next.js/React)
- Mobile app screens and navigation (Expo React Native)
- HTTP request/response handling, routing, UI rendering
- Depends only on Application layer

### 2-Application/
**Core business logic and orchestration**
- Use cases, application services, business workflows
- Command/query handlers (CQRS)
- Business rules, validation logic, policies
- DTOs for layer communication
- Depends only on Domain layer

### 3-Domain/
**Domain models, contracts, and interfaces**
- Entity definitions, value objects, domain models
- Repository interfaces, service contracts
- Domain events, specifications
- No dependencies on other layers

### 4-Persistence/
**Data access implementations**
- Native ADO.NET repository implementations (Microsoft.Data.SqlClient) and FluentMigrator-managed migrations
- Repository implementations, connection management, and SQL-based data access
- Database seeding and external API integrations
- Implements Domain contracts

### 5-Test/
**Testing infrastructure and test files**
- Unit tests, integration tests, architecture tests
- Test utilities, fixtures, mock data
- Test configurations and helpers
- Uses xUnit, FluentAssertions

### 6-Docs/
**Documentation and specifications**
- README files, API documentation, guides
- Technical specifications, architecture docs
- Development workflows, contributing guidelines

### 7-Deployment/
**Infrastructure and deployment assets**
- CI/CD pipelines, Docker configurations
- Infrastructure as Code (Terraform, Bicep, Ansible)
- Deployment scripts, environment configurations

## Search Guidelines

When performing file searches:
1. First search within the most relevant architectural layer based on the query context
2. If not found, expand to related layers following dependency direction
3. Use folder prefixes (e.g., "2-Application/") to narrow searches
4. For cross-cutting concerns, check 0-Base/ first

## Enforcement

- All new files must be placed in the correct folder immediately upon creation
- File moves must maintain architectural integrity
- Regular audits should verify adherence to this structure
- Exceptions require explicit architectural review and documentation