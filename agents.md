# AGENTS.md

## Project Overview
This repository contains a C# Web API following the Clean Architecture pattern.  
The solution is organized into the following core projects:
- **Domain:** Contracts, DTOs, and domain objects (no dependencies, no business logic).
- **Application:** Use cases, business logic, validation, and application orchestration.
- **Infrastructure:** Data access, external services, and framework integrations.
- **API (Presentation):** Controllers, dependency injection, filters, and API configuration.

The API is built using .NET (currently targeting .NET 8+).

---

## Folder Structure

The repository root contains both configuration files and several key folders structured to reflect Clean Architecture, as well as supporting directories.  
**Note:** Only the first 10 folders/files are listed below. For the complete list, visit the [GitHub repository root](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development).

### Numbered Folders

The project uses a numbered folder convention to clearly express architectural layering and dependencies:

- [`0-Base`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/0-Base):  
  This folder contains foundational code and abstractions that are shared across all other layers of the solution. Typical contents might include:
  - Base classes and interfaces (e.g., for entities, value objects, or exceptions)
  - Cross-cutting utilities (such as logging, error handling, or result types)
  - Shared constants and enums
  - Extension methods that do not belong to a specific layer

  The 0-Base layer should have no dependencies on higher layers and should avoid application-specific logic. It acts as the “core utilities” library for the rest of the solution.

- [`1-Presentation`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/1-Presentation):  
  This folder contains any presentation layer projects, such as the API project. It is responsible for:
  - Handling HTTP requests and responses.
  - Implementing controllers (e.g., REST endpoints) that expose application functionality to clients.
  - Handling API-specific concerns such as routing, model binding, input validation (presentation-level), filters, and formatting responses.
  - Setting up dependency injection and API configuration.
  - Potentially including middleware, exception handling, and API documentation setup (e.g., Swagger).
  - This folder should not contain business logic; it interacts with the Application layer to execute use cases.

- [`2-Application`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/2-Application):  
  This folder contains all business logic and application orchestration. It is responsible for:
  - **Use Cases / Application Services:** Classes that coordinate tasks, implement workflows, and enforce business rules.
  - **Business Logic:** All application-specific rules and processes.
  - **Commands/Queries:** For CQRS-based projects, defines commands and queries with their handlers.
  - **DTOs (Data Transfer Objects):** Objects for passing data between layers and/or to the outside world.
  - **Validation Logic:** Input validation using either custom logic or external libraries like FluentValidation.
  - **Policies and Specifications:** Application-level business policies or rules.
  
  > **Note:** No business logic should be implemented in the Domain layer. The Application layer may depend only on the Domain layer’s contracts, DTOs, and domain objects. It must not contain infrastructure or presentation code.

- [`3-Domain`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/3-Domain):  
  This folder contains the core contracts, DTOs, and domain objects that define the essential data structures and abstractions for the solution.
  - **Contracts and Interfaces:** Abstractions for persistence, services, or external dependencies required by the application.
  - **Domain Objects:** Entity definitions, value objects, and other core types.
  - **DTOs:** Shared data transfer objects used across application boundaries.
  
  > **Note:** The Domain layer contains no business logic. Its sole purpose is to define the core structure and contracts upon which the Application layer builds. The Domain layer has no dependencies on other layers.

- [`4-Persistence`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/4-Persistence):
  This folder contains all infrastructure code related to data storage and retrieval. Its primary responsibilities are:
  - Implementing the contracts and interfaces defined in the Domain layer for data access (e.g., repositories, unit of work, data stores).
  - Integrating with databases using native ADO.NET (Microsoft.Data.SqlClient) with schema and migrations managed by FluentMigrator; other storage options (Dapper, MongoDB, external APIs) may also be used where appropriate.
  - Providing concrete repository implementations that follow the ADO.NET patterns (parameterized SQL, async operations, proper disposal) and surface only interfaces to the Application layer.
  - Managing database context, migrations, and data seeding if relevant.

  > **Note:** The Persistence layer should only contain data access logic and infrastructure-specific concerns. It must not contain business logic or knowledge of the presentation layer. All communications with data stores should occur through interfaces defined in the Domain layer, ensuring a clean separation of concerns.

- [`5-Test`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/5-Test):  
  This folder contains all test projects and resources related to verifying the correctness, reliability, and quality of the solution. Its responsibilities and contents include:
  - **Unit Tests:** Projects and files for testing individual units of code in isolation, particularly for the Application and Domain layers.
  - **Integration Tests:** Projects that verify the correct interaction between multiple components or layers, such as Application with Infrastructure or Persistence.
  - **Test Utilities & Fixtures:** Shared test helpers, mocks, fakes, and setup/teardown routines.
  - **Test Data:** Sample data, seeds, or datasets used to drive automated tests.
  - **Test Configurations:** Configuration files or settings specific to the test environment (e.g., launch profiles, appsettings.Test.json).

  > **Note:** The Test layer should not contain production code or business logic. All tests should be automated and runnable via the build pipeline to ensure ongoing code quality and regressions are quickly identified.

- [`6-Docs`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/6-Docs):
  This folder contains all documentation and specifications related to the project. Its responsibilities and contents include:
  - **Project Documentation:** README files, API documentation, and user guides.
  - **Technical Specifications:** Architecture decisions, design documents, and system specifications.
  - **API Documentation:** OpenAPI/Swagger specifications and endpoint documentation.
  - **Development Guides:** Contributing guidelines, coding standards, and development workflows.
  - **Legacy Documentation:** Information about deprecated features and migration guides.

  > **Note:** The Docs folder should contain all project-related documentation that helps developers understand, maintain, and extend the system. All documentation should be kept current and reviewed regularly as part of the development process.

- [`7-Deployment`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/7-Deployment):  
  This folder contains all assets and scripts necessary for deploying the application in various environments (development, staging, production, etc). Its contents typically include:
  - **CI/CD Pipelines:** YAML or configuration files for continuous integration and deployment systems (e.g., GitHub Actions, Azure DevOps, GitLab CI).
  - **Docker & Containerization:** Dockerfiles, docker-compose files, and related container orchestration manifests.
  - **Infrastructure as Code (IaC):** Scripts and templates for provisioning infrastructure (e.g., Terraform, Bicep, ARM templates, Ansible, Pulumi).
  - **Deployment Scripts:** Shell scripts, PowerShell scripts, or batch files to automate build, publish, and deployment steps.
  - **Environment Configuration:** Sample or template environment files (e.g., `.env`, `appsettings.Production.json`) and secrets management guidance.
  - **Kubernetes & Cloud Native:** Helm charts, Kubernetes manifests, or other cloud-native deployment definitions.
  - **Release Notes & Runbooks:** Documentation for deployment processes, rollback procedures, or operational runbooks.
  
  > **Note:** The Deployment layer should not contain application code. It is strictly for automating and documenting how the application is built, released, and managed in real-world environments.


### Other root folders and files

- [`.github`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/.github):  
  GitHub configuration (issues, PR templates, workflows, etc).

- [`.vscode`](https://github.com/dpalfery/bolt-hotshot-logistics/tree/development/.vscode):  
  VS Code workspace settings.

- [`.editorconfig`](https://github.com/dpalfery/bolt-hotshot-logistics/blob/development/.editorconfig):  
  Coding style and formatting.

- [`.gitignore`](https://github.com/dpalfery/bolt-hotshot-logistics/blob/development/.gitignore):  
  Git ignore settings.
---

## Coding Style

- Use [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- **Indentation:** 4 spaces.
- **File Structure:**  
  - Keep each class in its own file.
  - Group related files by feature or domain in folders.
- **Naming:**  
  - Classes & interfaces: PascalCase (`OrderService`, `ICustomerRepository`)
  - Methods & properties: PascalCase
  - Parameters & local variables: camelCase
  - Constants: PascalCase or UPPER_SNAKE_CASE
- **Namespaces:** Follow folder structure and project organization.
- **Async:** Use async/await throughout; name async methods with `Async` suffix.
- **Nullable:** Use nullable reference types (`#nullable enable`).

---

## Clean Architecture Rules

- **No project may reference a project “up” the architecture:**  
  - Domain has no dependencies.
  - Application depends only on Domain.
  - Infrastructure depends on Application and Domain.
  - API depends on Application, Infrastructure, and Domain.
- **Business logic** goes only in Application.
- **Infrastructure** contains native ADO.NET implementations, FluentMigrator migrations, external APIs, and implementation details.
- **API** contains only presentation and configuration logic.
- Use dependency injection for all external services and infrastructure dependencies.

---

## Testing

- All business logic must be covered by unit tests (xUnit recommended).
- Integration tests should cover key application and infrastructure boundaries.
- Mock dependencies in application tests.
- Use FluentAssertions for assertions.
- Aim for 80%+ code coverage for Application and Domain layers.

---

## Pull Request Guidelines

- Target the `develop` branch by default.
- Use descriptive PR titles and summaries.
- Reference related issues with `#issue-number`.
- Every PR must include:
  - A summary of changes.
  - Unit/integration tests for new or updated features.
  - Updates to documentation (if API surface changes).
- Ensure build passes and tests succeed before review.

---

## Dependency Management

- Use [NuGet](https://www.nuget.org/) for all dependencies.
- Keep dependencies up to date; avoid unnecessary packages.
- All new dependencies must be reviewed.

---

## Documentation

- Keep `README.md` up to date.
- All public APIs must include XML doc comments.
- Add/Update OpenAPI/Swagger documentation for endpoints.

---

## Known Limitations / TODOs

- Some legacy modules (listed in `/docs/LEGACY.md`) may not conform to the latest architecture—do not refactor unless specifically assigned.
- Multi-tenancy and distributed transactions not fully implemented.

---

## Security

- Do not log sensitive data.
- Use built-in ASP.NET Core Identity for authentication if possible.
- Validate all user input and use Data Annotations or FluentValidation.

---

## Examples of Good PRs

- [PR #42: Add Order Processing Use Case](https://github.com/org/repo/pull/42)
- [PR #57: Refactor Customer Aggregate](https://github.com/org/repo/pull/57)

---

## Other Notes for Codex

- Prioritize code readability, maintainability, and testability.
- When in doubt, add XML comments.
- Prefer explicit over implicit logic.
