---
id: rules/file-organization
title: File Organization Rule
doc-type: rule
status: draft
owner: unassigned
last-reviewed: 2026-08-14
---

# File Organization Rule

This rule enforces the Clean Architecture numbered folder structure for all file operations in the Hotshot Logistics project. It must be applied whenever files are created, moved, added, or searched to maintain separation of concerns and dependency direction.

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
- API controllers and endpoints (ASP.NET Core Web API on Azure Container Apps)
- Web dashboard components (Next.js)
- HTTP request and response handling, routing, UI rendering
- Depends only on Application layer
- There is no mobile project in this tree

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
- Native ADO.NET implementations with schema and migrations managed by FluentMigrator
- Repository implementations using ADO.NET (parameterized SQL, async operations, proper disposal)
- Database seeding, external API integrations
- Implements Domain contracts

### 5-Test/
**Testing infrastructure and test files**
- Unit tests, integration tests, architecture tests
- Test utilities, fixtures, mock data
- Test configurations and helpers
- Uses xUnit, FluentAssertions
