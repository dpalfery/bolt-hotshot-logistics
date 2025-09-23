# Scripts Rule

This rule provides common command-line scripts and commands used in the Hotshot Logistics .NET project for development, building, testing, and database management.

## When to Apply
Use these commands whenever working with the .NET project for development tasks, builds, testing, or database operations.

## Development Commands

### Hot Reload Development
- `dotnet watch` - Start the application with hot reload for development
- `dotnet watch run` - Run the application with file watching and automatic restart

### Building
- `dotnet build` - Build the project in debug mode
- `dotnet build -c Release` - Build the project in release mode for production

### Running
- `dotnet run` - Run the application locally
- `dotnet run --launch-profile "Development"` - Run with specific launch profile

### Testing
- `dotnet test` - Run all unit and integration tests
- `dotnet test --filter "Category=Unit"` - Run only unit tests
- `dotnet test --filter "Category=Integration"` - Run only integration tests

## Database Commands (FluentMigrator)

### Migration Commands
- `fluentmigrator migrate` - Apply all pending migrations to the database
- `fluentmigrator migrate -t 20250101000000` - Migrate to specific version
- `fluentmigrator rollback` - Rollback the last migration
- `fluentmigrator rollback -t 20250101000000` - Rollback to specific version

### Migration Management
- `fluentmigrator list` - List all available migrations
- `fluentmigrator validate` - Validate the current database schema against migrations

## Package Management
- `dotnet add package PackageName` - Add a NuGet package to the project
- `dotnet remove package PackageName` - Remove a NuGet package
- `dotnet restore` - Restore all NuGet packages

## Publishing
- `dotnet publish -c Release` - Publish the application for deployment
- `dotnet publish -c Release -r win-x64` - Publish for specific runtime (Windows x64)
- `dotnet publish -c Release -r linux-x64` - Publish for Linux x64

## Docker Commands (if applicable)
- `docker build -t hotshot-logistics .` - Build Docker image
- `docker run -p 8080:80 hotshot-logistics` - Run the application in Docker

## Notes
- Always run commands from the project root directory
- Use `dotnet --version` to check your .NET SDK version
- For database operations, ensure connection strings are properly configured
- Use `dotnet clean` to clean build artifacts before rebuilding