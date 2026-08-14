---
id: rules/scripts
title: Scripts Rule
doc-type: rule
status: current
owner: unassigned
last-reviewed: 2026-08-14
---

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

## Code Generation
- `dotnet new` - Create new projects, files, or solutions
- `dotnet new sln` - Create a new solution file
- `dotnet sln add Project.csproj` - Add a project to the solution

## Cleaning
- `dotnet clean` - Clean the build output
- `dotnet nuget locals all --clear` - Clear all NuGet caches
