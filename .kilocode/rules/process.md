# Process Rule

Enforces development process guidelines for Hotshot Logistics, including task tracking and command-line operations.

## When to Apply
Apply when managing tasks, creating status files, or executing development workflows in .NET projects.

## Task Tracking
- Create todo lists and status tracking markdown files in the 6-docs/status folder.
- Use these files to maintain state and memory during complex tasks.
- After task completion, ask the user (or assigning agent) if they want to delete the created files.

## Development Workflows
- Use appropriate CLI commands for development, building, testing, database management, package management, publishing, and containerization.
- Execute commands from the project root directory.
- Ensure proper configuration (e.g., connection strings, SDK versions) before running database or deployment commands.

## Development Environment Setup Procedures

### Prerequisites
- Install .NET 8 SDK (or later) from Microsoft
- Install Node.js 18+ and npm for frontend development
- Install Expo CLI for React Native mobile development
- Install SQL Server (LocalDB or full instance) for local database development
- Install Visual Studio 2022 or VS Code with C# and Azure Functions extensions
- Install Git for version control
- Install Docker Desktop for containerized development

### Repository Setup
1. Clone the repository: `git clone https://github.com/dpalfery/bolt-hotshot-logistics.git`
2. Navigate to project root: `cd bolt-hotshot-logistics`
3. Restore .NET dependencies: `dotnet restore`
4. Build the solution: `dotnet build HotshotLogistics.sln`

### Database Setup
1. Ensure SQL Server is running locally
2. Run migrations to set up database schema: `dotnet run --project 4-Persistence/MigrationRunner/MigrationRunner.csproj`
3. Verify database connection in `appsettings.Development.json`

### Backend API Setup
1. Configure Azure Functions local settings in `1-Presentation/HotshotLogistics.Api/local.settings.json`
2. Start the API locally: `dotnet run --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj`
3. Verify API endpoints at `http://localhost:7071/api`

### Frontend Dashboard Setup
1. Navigate to admin dashboard directory (when created)
2. Install dependencies: `npm install`
3. Start development server: `npm run dev`
4. Access dashboard at configured port

### Mobile App Setup
1. Navigate to mobile app directory (when created)
2. Install dependencies: `npm install`
3. Start Expo development server: `npx expo start`
4. Use Expo Go app on device or simulator to test

### Environment Validation
- Run full test suite to ensure setup completeness
- Verify all services can communicate (API, database, external services)
- Check logging and monitoring configurations

## Database Migration Workflows

### Creating New Migrations
1. Navigate to `4-Persistence/HotshotLogistics.Data/Migrations/`
2. Create new migration class following naming convention: `YYYYMMDDHHMM_Description.cs`
3. Implement `Up()` method for schema changes
4. Implement `Down()` method for rollback capability
5. Use FluentMigrator API for table creation, column changes, indexes, etc.

### Running Migrations
- Development: `dotnet run --project 4-Persistence/MigrationRunner/MigrationRunner.csproj`
- Production: Migrations run automatically during deployment via CI/CD
- Specific version: Use `--version` parameter to target specific migration

### Rollback Procedures
1. Identify target migration version for rollback
2. Execute rollback: `dotnet run --project 4-Persistence/MigrationRunner/MigrationRunner.csproj -- --rollback --version <target_version>`
3. Verify data integrity after rollback
4. Re-run forward migrations if needed

### Testing Migrations
- Test migrations on development database first
- Use transaction rollbacks in tests to maintain clean state
- Validate data migration scripts with sample data
- Test rollback procedures before production deployment

### Migration Best Practices
- Always include rollback paths
- Test migrations on copy of production data
- Use descriptive migration names
- Avoid large data transformations in single migration
- Document any manual steps required

## Testing Execution Patterns

### Unit Testing
- Run all unit tests: `dotnet test 5-Test/tests/HotshotLogistics.Tests/HotshotLogistics.Tests.csproj`
- Run specific test class: `dotnet test --filter "ClassName=JobServiceTests"`
- Run with coverage: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov`
- Minimum coverage requirement: 80% for Application and Domain layers

### Integration Testing
- Run integration tests: `dotnet test --filter "Category=Integration"`
- Ensure test database is available and seeded
- Use `Testcontainers` or local SQL Server instance
- Clean up test data between test runs

### End-to-End Testing
- Run E2E tests against local environment
- Use tools like Playwright for API testing
- Test complete workflows from request to database
- Validate external service integrations

### CI/CD Testing
- All tests run automatically on pull requests
- Coverage reports generated and archived
- Quality gates prevent merging on test failures
- Performance benchmarks included in pipeline

### Test Data Management
- Use FluentMigrator seed data for consistent test setup
- Mock external dependencies in unit tests
- Use factory patterns for test data creation
- Clean up test data after each test run

## Deployment Procedures

### Local Deployment Testing
1. Build solution: `dotnet build HotshotLogistics.sln --configuration Release`
2. Run tests: `dotnet test --configuration Release`
3. Create deployment package: `dotnet publish 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj --configuration Release`
4. Test deployment locally with Docker: `docker-compose up`

### Azure Deployment
1. Ensure Terraform configuration is up to date in `7-Deployment/Azure-deploy/`
2. Plan deployment: `terraform plan -var-file=environments/dev/terraform.tfvars`
3. Apply changes: `terraform apply -var-file=environments/dev/terraform.tfvars`
4. Verify deployment through Azure portal or CLI

### CI/CD Pipeline
- Automated builds triggered on push to develop/main branches
- Multi-stage pipeline: build → test → security scan → deploy
- Environment-specific deployments (dev → staging → prod)
- Rollback capability with previous release tags

### Environment Management
- Use Azure App Configuration for environment-specific settings
- Store secrets in Azure Key Vault
- Validate configurations before deployment
- Monitor environment health post-deployment

### Rollback Procedures
1. Identify last stable deployment tag
2. Execute rollback via CI/CD pipeline
3. Monitor application health during rollback
4. Validate functionality after rollback completion

## Code Review Checklists

### Architecture Compliance
- [ ] Files placed in correct numbered folder structure
- [ ] Dependencies flow downward (Domain → Application → Infrastructure)
- [ ] Repository pattern implemented for data access
- [ ] CQRS pattern followed for commands/queries
- [ ] Clean Architecture principles maintained

### Code Quality
- [ ] StyleCop rules pass without warnings
- [ ] No build errors or warnings
- [ ] Async/await used for all I/O operations
- [ ] Proper error handling with meaningful exceptions
- [ ] XML documentation on public APIs

### Security
- [ ] No secrets committed to source control
- [ ] Input validation implemented
- [ ] SQL injection prevention (parameterized queries)
- [ ] Authentication/authorization properly configured
- [ ] CORS and other security headers set

### Testing
- [ ] Unit tests cover new functionality
- [ ] Integration tests for data access
- [ ] Test coverage meets minimum requirements
- [ ] Mock external dependencies appropriately
- [ ] Test data properly managed

### Documentation
- [ ] Code changes documented in commit messages
- [ ] API changes reflected in OpenAPI specs
- [ ] README updated for new features
- [ ] Migration notes included for breaking changes

## Task Management Standards

### When to Create Todo Lists
- Complex tasks involving multiple steps or files
- Tasks requiring coordination across architectural layers
- Multi-day development efforts
- Tasks with unclear requirements needing iterative refinement
- When working with unfamiliar codebase sections

### Todo List Format
- Use markdown checklist format with [ ] for pending, [x] for completed, [-] for in progress
- List tasks in logical execution order
- Include specific, actionable descriptions
- Break down large tasks into smaller, verifiable steps
- Update status immediately after completing each item

### Status Tracking
- Create status files in `6-Docs/status/` for complex tasks
- Include task context, current progress, and next steps
- Update status files after each significant milestone
- Archive or delete status files after task completion

### File Management
- Store temporary status files in `6-Docs/status/` directory
- Use descriptive filenames (e.g., `20250126-implement-job-tracking.md`)
- Clean up files after task completion unless needed for documentation
- Consider moving completed status files to `6-Docs/archive/` for historical reference

### Integration with Memory Bank
- Update memory bank context.md after significant task completion
- Document new patterns or procedures discovered during tasks
- Add repetitive tasks to tasks.md for future reference
- Maintain project knowledge continuity across sessions

## References
See 6-Docs/process-examples.md for detailed command examples and usage patterns.
See 6-Docs/ado-net-examples.md for data access implementation examples.
See 6-Docs/architecture-examples.md for architectural pattern implementations.
See 6-Docs/code-quality-examples.md for code quality and testing examples.
See 6-Docs/security-examples.md for security implementation patterns.