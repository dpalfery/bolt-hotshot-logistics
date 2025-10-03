# Hotshot Logistics Database Setup Tool

This document: [`7-Deployment/DbSetup/README.md`](7-Deployment/DbSetup/README.md)

## Overview

The Hotshot Logistics Database Setup CLI is a cross-platform .NET 8+ console application that automates the provisioning of SQL Server databases and application accounts for the Hotshot Logistics platform. It integrates with the existing FluentMigrator migrations to set up a complete database environment.

## Features

- **Cross-platform support**: Windows, macOS, and Linux
- **Interactive and non-interactive modes**: Supports both developer workflows and CI/CD pipelines
- **Secure password generation**: Cryptographically secure password generation with policy enforcement
- **Environment variable management**: Secure handling and persistence of database credentials
- **Comprehensive logging**: Structured logging with secret masking for security
- **Rollback capabilities**: Automatic cleanup on failure
- **Integration testing**: Comprehensive test suite with real database operations

## Prerequisites

- .NET 8+ SDK
- SQL Server instance (local, Docker, or remote)
- Appropriate permissions to create databases and logins

## Quick Start

### Interactive Mode (Recommended for Development)

```bash
# Navigate to the project directory
cd 7-Deployment/DbSetup/HotshotLogistics.DbSetup

# Run with default settings (interactive prompts)
dotnet run

# Or specify parameters
dotnet run --server "localhost\\SQLEXPRESS" --db-name "hotshot_logistics"
```

### Non-Interactive Mode (CI/CD)

```bash
dotnet run --non-interactive \
  --server "localhost" \
  --sa-connection-string "Server=localhost;Database=master;User Id=sa;Password=YourStrong@Passw0rd;" \
  --db-name "hotshot_logistics"
```

## Installation

### As a .NET Tool (Recommended)

```bash
# Install as a global .NET tool
dotnet pack
dotnet tool install --global --add-source ./nupkg HotshotLogistics.DbSetup

# Use the tool
hotshot-db-setup --server "localhost\\SQLEXPRESS"
```

### As a Local Project

1. Clone the repository
2. Navigate to the project: `cd 7-Deployment/DbSetup/HotshotLogistics.DbSetup`
3. Restore dependencies: `dotnet restore`
4. Run: `dotnet run [arguments]`

## Command Line Options

| Option | Description | Default | Example |
|--------|-------------|---------|---------|
| `--server`, `-s` | SQL Server instance | Required | `localhost\\SQLEXPRESS` |
| `--sa-connection-string`, `--sa-cs` | SA connection string for privileged operations | Required for non-interactive | `Server=myserver;Database=master;User Id=sa;Password=***` |
| `--db-name` | Target database name | `hotshot_logistics` | `my_app_db` |
| `--app-user` | Application user/login name | `hotshot_app` | `my_app_user` |
| `--password` | Application user password | Auto-generated | `MySecurePassword123!` |
| `--non-interactive` | Run without prompts | `false` | `--non-interactive` |
| `--force` | Allow destructive operations | `false` | `--force` |
| `--persist-env` | Persist password to system environment | `false` | `--persist-env` |

## Environment Variables

The tool supports the following environment variables as fallbacks:

| Variable | Description | Example |
|----------|-------------|---------|
| `HOTSHOT_DB_SERVER` | SQL Server instance | `localhost\\SQLEXPRESS` |
| `HOTSHOT_DB_NAME` | Database name | `hotshot_logistics` |
| `HOTSHOT_DB_APP_USER` | Application user name | `hotshot_app` |
| `HOTSHOT_DB_PASSWORD` | Application password | `MySecurePassword123!` |
| `CI_SA_CONNECTION_STRING` | SA connection string for CI | `Server=myserver;Database=master;User Id=sa;Password=***` |

## Security Features

### Password Generation

- **Cryptographically secure**: Uses `RandomNumberGenerator` for entropy
- **Policy enforcement**: Minimum 16 characters with mixed character classes
- **Configurable length**: Default 24 characters
- **No plaintext storage**: Passwords are not logged or stored unnecessarily

### Secret Masking

- **Automatic masking**: All logs automatically mask passwords and connection strings
- **Pattern recognition**: Detects common password patterns and masks them
- **Custom secrets**: Register additional secrets for masking
- **Safe logging**: Structured logging with security-first design

### Environment Variable Handling

- **Cross-platform support**: Works on Windows, macOS, and Linux
- **Secure persistence**: Only persists with explicit user consent
- **Privilege management**: Handles administrator requirements appropriately
- **Fallback strategies**: Graceful degradation when persistence fails

## Architecture

The tool follows Clean Architecture principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│                    Program.cs                           │
│                 (Entry Point)                           │
└─────────────────────┬───────────────────────────────────┘
                      │
    ┌─────────────────┼─────────────────┐
    │                 ▼                 │
┌─────────┐  ┌──────────────┐  ┌──────────────┐
│Argument │  │Environment   │  │Interactive   │
│Parser   │  │Manager       │  │Prompter      │
└─────────┘  └──────────────┘  └──────────────┘
    │                 │                 │
    └─────────────────┼─────────────────┘
                      │
    ┌─────────────────┼─────────────────┐
    │                 ▼                 │
┌─────────┐  ┌──────────────┐  ┌──────────────┐
│Password │  │Preflight     │  │Secure        │
│Manager  │  │Checker       │  │Logger        │
└─────────┘  └──────────────┘  └──────────────┘
    │                 │                 │
    └─────────────────┼─────────────────┘
                      │
    ┌─────────────────┼─────────────────┐
    │                 ▼                 │
┌─────────┐  ┌──────────────┐  ┌──────────────┐
│SqlServer│  │Permission    │  │Migration     │
│Provi-   │  │Manager       │  │Invoker       │
│sioner   │  │              │  │              │
└─────────┘  └──────────────┘  └──────────────┘
```

## Development Workflow

### 1. Project Structure

The tool is organized into logical components:

- **Core Components**:
  - `ArgumentParser`: Command-line argument processing
  - `EnvironmentManager`: Cross-platform environment variable handling
  - `PasswordManager`: Secure password generation and validation
  - `InteractivePrompter`: User interaction and prompts

- **Database Operations**:
  - `SqlServerProvisioner`: Database and login creation with rollback
  - `PermissionManager`: Minimal permission assignment
  - `MigrationInvoker`: FluentMigrator integration

- **Infrastructure**:
  - `PreflightChecker`: Pre-deployment validation
  - `SecureLogger`: Security-focused logging with masking

### 2. Adding New Features

When adding new functionality:

1. **Follow Clean Architecture**: Keep business logic separate from infrastructure
2. **Add tests**: Include unit and integration tests for new components
3. **Update documentation**: Keep CLI usage and design docs current
4. **Security first**: Never log secrets, use parameterized queries
5. **Cross-platform**: Test on Windows, macOS, and Linux

### 3. Testing Strategy

```bash
# Run all tests
dotnet test

# Run with coverage (if configured)
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Integration"
```

## Deployment Scenarios

### Local Development

```bash
# Windows with SQL Server Express
dotnet run --server "localhost\\SQLEXPRESS" --persist-env

# macOS with Docker
dotnet run --server "localhost" --sa-connection-string "Server=host.docker.internal;..."

# Linux with remote SQL Server
dotnet run --server "sqlserver.example.com" --non-interactive
```

### CI/CD Pipelines

See [`6-Docs/specs/dbsetup/cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md) for complete pipeline examples.

### Production Deployment

```bash
# Use managed identity or service principal
dotnet run --non-interactive \
  --server "production-sql.database.windows.net" \
  --sa-connection-string "$(AZURE_SA_CONNECTION_STRING)" \
  --db-name "hotshot_logistics_prod"
```

## Troubleshooting

### Common Issues

1. **Permission Denied**
   ```bash
   # Ensure SA credentials have sufficient privileges
   # Check SQL Server authentication mode
   # Verify Windows account permissions for Windows auth
   ```

2. **Connection Timeout**
   ```bash
   # Verify SQL Server is running and accessible
   # Check firewall settings
   # Ensure correct server name/instance
   ```

3. **Environment Variable Not Set**
   ```bash
   # Use --persist-env flag to set system environment variables
   # Manually set variables using platform-specific commands
   # Check variable scope (user vs machine level)
   ```

### Debug Mode

```bash
# Enable debug logging
export HOTSHOT_LOG_LEVEL=Debug
dotnet run --server "localhost\\SQLEXPRESS"

# Or use structured logging
dotnet run --server "localhost\\SQLEXPRESS" --verbosity detailed
```

### Health Checks

```bash
# Test connectivity only
dotnet run --server "localhost\\SQLEXPRESS" --dry-run

# Validate permissions
dotnet run --server "localhost\\SQLEXPRESS" --validate-permissions
```

## Contributing

1. **Code Style**: Follow Microsoft C# Coding Conventions
2. **Testing**: Add tests for all new functionality
3. **Documentation**: Update docs for API changes
4. **Security**: Never commit secrets, use secure coding practices
5. **Architecture**: Maintain Clean Architecture boundaries

## License

This project is part of the Hotshot Logistics platform and follows the same licensing terms.

## Support

For issues and questions:
1. Check the troubleshooting section above
2. Review application logs for detailed error messages
3. Ensure all prerequisites are met
4. Verify SQL Server configuration and permissions
5. Check the [CLI usage guide](6-Docs/specs/dbsetup/cli-usage.md) for detailed examples