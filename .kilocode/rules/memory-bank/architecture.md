The Hotshot Logistics project follows Clean Architecture principles with a numbered folder structure to enforce separation of concerns and dependency direction. See [`file-organization.md`](file-organization.md) for detailed folder structure and file placement rules.

Key architectural decisions:
- Dependency injection throughout
- CQRS for command/query separation
- Repository pattern for data access
- Service layer for business logic
- Strict layering prevents circular dependencies
- Testable design with mocked dependencies