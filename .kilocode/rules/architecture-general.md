name: "Folder-Rule"
description: "Enforces fundamental Clean Architecture principles and layered structure that apply across all development activities in Hotshot Logistics."
when-to-apply:
"Apply when creating, moving, adding, or searching files to maintain separation of concerns and dependency direction across all technology stacks."
rule: |

### File Placement Guidelines

Place files in appropriate numbered folder based on purpose and architectural layer:

#### 0-Base/
**Shared foundational code and abstractions**
- Base classes, interfaces, utilities, extensions
- Cross-cutting concerns (logging, error handling, result types)
- Shared constants, enums, extension methods
- No dependencies on higher layers

#### 1-Presentation/
**Presentation layer projects and components**
- API controllers,ASP.NET Core Web API, functions, endpoints (Azure Functions)
- Web dashboard components (Next.js/React)
- Mobile app screens and navigation (Expo React Native)
- HTTP request/response handling, routing, UI rendering
- Depends only on Application layer

#### 2-Application/
**Core business logic and orchestration**
- Use cases, application services, business workflows
- Command/query handlers (CQRS)
- Business rules, validation logic, policies
- DTOs for layer communication
- Depends only on Domain layer

#### 3-Domain/
**Domain models, contracts, and interfaces**
- There are two projects in this folder Contracts and Domain. **ALL DTO Objects** go in Domain, **ALL Interfaces** go in Contracts. Interfaces or DTOs should not be defined anywhere else in the solution unless we are creating a single use project for deployment or utility and in that case you should confirm with the Human
- Entity definitions, value objects, domain models
- Repository interfaces, service contracts
- Domain events, specifications
- No dependencies on other layers

#### 4-Persistence/
**Data access implementations**
- Native ADO.NET implementations with schema and migrations managed by FluentMigrator
- Repository implementations using ADO.NET (parameterized SQL, async operations, proper disposal)
- Database seeding, external API integrations
- Implements Domain contracts

#### 5-Test/
**Testing infrastructure and test files**
- Unit tests, integration tests, architecture tests
- Test utilities, fixtures, mock data
- Test configurations and helpers
- Uses xUnit, FluentAssertions

#### 6-Docs/
**Documentation and specifications**
- README files, API documentation, guides
- Technical specifications, architecture docs
- Development workflows, contributing guidelines

#### 7-Deployment/
**Infrastructure and deployment assets**
- CI/CD pipelines, Docker configurations
- Infrastructure as Code (Terraform, Bicep, Ansible)
- Deployment scripts, environment configurations

### Search Guidelines

When searching files:
1. Search within most relevant architectural layer based on query context
2. If not found, expand to related layers following dependency direction
3. Use folder prefixes (e.g., "2-Application/") to narrow searches
4. For cross-cutting concerns, check 0-Base/ first

### Enforcement
- Place new files in correct folder immediately upon creation
- File moves must maintain architectural integrity
- Regular audits verify adherence to this structure
- Exceptions require explicit architectural review and documentation

