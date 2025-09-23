# Scripts Rule

Provides common command-line scripts for Hotshot Logistics .NET project development, building, testing, and database management.

## When to Apply
Use when working with .NET project for development, builds, testing, or database operations.

## Development Commands

### Hot Reload Development
- `dotnet watch` - Start app with hot reload for development
- `dotnet watch run` - Run app with file watching and auto-restart

### Building
- `dotnet build` - Build project in debug mode
- `dotnet build -c Release` - Build project in release mode for production

### Running
- `dotnet run` - Run app locally
- `dotnet run --launch-profile "Development"` - Run with specific launch profile

### Testing
- `dotnet test` - Run all unit and integration tests
- `dotnet test --filter "Category=Unit"` - Run only unit tests
- `dotnet test --filter "Category=Integration"` - Run only integration tests

## Database Commands (FluentMigrator)

### Migration Commands
- `fluentmigrator migrate` - Apply all pending migrations to database
- `fluentmigrator migrate -t 20250101000000` - Migrate to specific version
- `fluentmigrator rollback` - Rollback last migration
- `fluentmigrator rollback -t 20250101000000` - Rollback to specific version

### Migration Management
- `fluentmigrator list` - List all available migrations
- `fluentmigrator validate` - Validate current database schema against migrations

## Package Management
- `dotnet add package PackageName` - Add NuGet package to project
- `dotnet remove package PackageName` - Remove NuGet package
- `dotnet restore` - Restore all NuGet packages

## Publishing
- `dotnet publish -c Release` - Publish app for deployment
- `dotnet publish -c Release -r win-x64` - Publish for Windows x64 runtime
- `dotnet publish -c Release -r linux-x64` - Publish for Linux x64 runtime

## Docker Commands (if applicable)
- `docker build -t hotshot-logistics .` - Build Docker image
- `docker run -p 8080:80 hotshot-logistics` - Run app in Docker

## Notes
- Run commands from project root
- Check .NET SDK version with `dotnet --version`
- Ensure connection strings configured for database operations
- Use `dotnet clean` to clean build artifacts before rebuilding