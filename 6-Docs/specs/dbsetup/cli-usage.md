# CLI Usage Guide

This document: [`6-Docs/specs/dbsetup/cli-usage.md`](6-Docs/specs/dbsetup/cli-usage.md)

## Overview

The Hotshot Logistics Database Setup CLI automates the provisioning of SQL Server databases and application accounts. It supports both interactive developer workflows and non-interactive CI/CD pipeline execution.

## Basic Usage

### Interactive Mode (Default)

```bash
# Run interactively with prompts
dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj

# Or specify all parameters
dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj --server "localhost\\SQLEXPRESS" --db-name "hotshot_logistics"
```

### Non-Interactive Mode (CI/CD)

```bash
# Run without prompts using environment variables or command line arguments
dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj --non-interactive --server "localhost\\SQLEXPRESS" --sa-connection-string "Server=myserver;Database=master;User Id=sa;Password=mypassword;" --db-name "hotshot_logistics"
```

## Command Line Options

| Option | Short | Description | Example |
|--------|-------|-------------|---------|
| `--server` | `-s` | SQL Server instance | `localhost\\SQLEXPRESS` |
| `--sa-connection-string` | `--sa-cs` | SA or privileged connection string (required for non-interactive) | `Server=myserver;Database=master;User Id=sa;Password=***` |
| `--db-name` | | Target database name (default: hotshot_logistics) | `hotshot_logistics` |
| `--app-user` | | Application user/login name (default: hotshot_app) | `hotshot_app` |
| `--password` | | Application user password (prefer env var) | `MySecurePassword123!` |
| `--non-interactive` | | Run without prompts for CI/CD | `--non-interactive` |
| `--force` | | Allow destructive operations | `--force` |
| `--persist-env` | | Persist password to system environment variable | `--persist-env` |

## Environment Variables

The CLI supports the following environment variables as fallbacks:

| Variable | Description | Example |
|----------|-------------|---------|
| `HOTSHOT_DB_SERVER` | SQL Server instance | `localhost\\SQLEXPRESS` |
| `HOTSHOT_DB_NAME` | Database name | `hotshot_logistics` |
| `HOTSHOT_DB_APP_USER` | Application user name | `hotshot_app` |
| `HOTSHOT_DB_PASSWORD` | Application password | `MySecurePassword123!` |
| `CI_SA_CONNECTION_STRING` | SA connection string for CI | `Server=myserver;Database=master;User Id=sa;Password=***` |

## CI/CD Pipeline Examples

### GitHub Actions

```yaml
name: Database Setup
on:
  push:
    branches: [ main, develop ]

jobs:
  setup-database:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2019-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: ${{ secrets.CI_SQL_SA_PASSWORD }}
        ports:
          - 1433:1433
        options: >-
          --health-cmd="/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P ${{ secrets.CI_SQL_SA_PASSWORD }} -Q 'SELECT 1'"
          --health-interval=10s
          --health-timeout=5s
          --health-retries=3

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build projects
      run: dotnet build --no-restore

    - name: Setup Database
      run: |
        dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj \
          --non-interactive \
          --server "localhost" \
          --sa-connection-string "Server=localhost;Database=master;User Id=sa;Password=${{ secrets.CI_SQL_SA_PASSWORD }};" \
          --db-name "hotshot_logistics_test"
      env:
        HOTSHOT_DB_PASSWORD: ${{ secrets.HOTSHOT_DB_PASSWORD }}

    - name: Run Tests
      run: dotnet test --no-build --verbosity normal
```

### Azure DevOps Pipelines

```yaml
trigger:
- main
- develop

pool:
  vmImage: 'windows-latest'

variables:
  BuildConfiguration: 'Release'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'
    includePreviewVersions: true

- task: DotNetCoreCLI@2
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--no-restore --configuration $(BuildConfiguration)'

- script: |
    dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj ^
      --non-interactive ^
      --server "$(SQL_SERVER)" ^
      --sa-connection-string "$(SA_CONNECTION_STRING)" ^
      --db-name "hotshot_logistics_$(Build.BuildId)"
  displayName: 'Setup Database'
  env:
    HOTSHOT_DB_PASSWORD: $(HOTSHOT_DB_PASSWORD)
    SQL_SERVER: $(SQL_SERVER)
    SA_CONNECTION_STRING: $(SA_CONNECTION_STRING)

- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'
    arguments: '--no-build --configuration $(BuildConfiguration)'
```

### GitLab CI

```yaml
stages:
  - setup
  - test

setup_database:
  stage: setup
  image: mcr.microsoft.com/dotnet/sdk:8.0
  services:
    - mcr.microsoft.com/mssql/server:2019-latest
  variables:
    ACCEPT_EULA: "Y"
    SA_PASSWORD: $CI_SQL_SA_PASSWORD
  script:
    - dotnet restore
    - dotnet build --no-restore
    - |
      dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj \
        --non-interactive \
        --server "mssql" \
        --sa-connection-string "Server=mssql;Database=master;User Id=sa;Password=$CI_SQL_SA_PASSWORD;" \
        --db-name "hotshot_logistics_$CI_PIPELINE_ID"
  environment:
    name: test
```

## Docker Usage

### Local Development with Docker

```bash
# Start SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2019-latest

# Wait for SQL Server to be ready
sleep 30

# Run database setup
dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj \
  --server "localhost" \
  --sa-connection-string "Server=localhost;Database=master;User Id=sa;Password=YourStrong@Passw0rd;"

# Clean up
docker stop sqlserver
docker rm sqlserver
```

### CI with Docker Compose

```yaml
version: '3.8'
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      ACCEPT_EULA: Y
      SA_PASSWORD: ${CI_SQL_SA_PASSWORD}
    ports:
      - "1433:1433"

  dbsetup:
    build: .
    depends_on:
      - sqlserver
    environment:
      HOTSHOT_DB_SERVER: sqlserver
      HOTSHOT_DB_PASSWORD: ${HOTSHOT_DB_PASSWORD}
    command: >
      dotnet run --project 7-Deployment/DbSetup/HotshotLogistics.DbSetup/HotshotLogistics.DbSetup.csproj
        --non-interactive
        --sa-connection-string "Server=sqlserver;Database=master;User Id=sa;Password=${CI_SQL_SA_PASSWORD};"
        --db-name "hotshot_logistics"
```

## Security Best Practices

### Secret Management

1. **Never commit secrets to source control**
2. **Use pipeline secret variables** in CI/CD systems
3. **Store secrets in Key Vault** for production environments
4. **Use environment variables** for local development

### Azure Key Vault Integration

```bash
# Set up Key Vault secrets
az keyvault secret set --vault-name "mykeyvault" --name "HotshotDbPassword" --value "MySecurePassword"
az keyvault secret set --vault-name "mykeyvault" --name "HotshotSaPassword" --value "MySAPassword"

# Use in pipeline
export HOTSHOT_DB_PASSWORD=$(az keyvault secret show --vault-name "mykeyvault" --name "HotshotDbPassword" --query value -o tsv)
export SA_CONNECTION_STRING="Server=myserver;Database=master;User Id=sa;Password=$(az keyvault secret show --vault-name "mykeyvault" --name "HotshotSaPassword" --query value -o tsv);"
```

### AWS Secrets Manager

```bash
# Retrieve secrets in pipeline
HOTSHOT_DB_PASSWORD=$(aws secretsmanager get-secret-value --secret-id "hotshot/db/password" --query SecretString --output text)
SA_PASSWORD=$(aws secretsmanager get-secret-value --secret-id "hotshot/sa/password" --query SecretString --output text)
```

## Troubleshooting

### Common Issues

1. **Permission Denied**
   - Ensure SA credentials have sufficient privileges
   - Check SQL Server authentication mode
   - Verify Windows account permissions for Windows auth

2. **Connection Timeout**
   - Verify SQL Server is running and accessible
   - Check firewall settings
   - Ensure correct server name/instance

3. **Environment Variable Not Set**
   - Use `--persist-env` flag to set system environment variables
   - Manually set variables using platform-specific commands
   - Check variable scope (user vs machine level)

### Platform-Specific Environment Setup

#### Windows
```powershell
# Set user-level variable
[Environment]::SetEnvironmentVariable("HOTSHOT_DB_PASSWORD", "MyPassword", "User")

# Set machine-level variable (requires admin)
[Environment]::SetEnvironmentVariable("HOTSHOT_DB_PASSWORD", "MyPassword", "Machine")

# Using setx (requires admin for machine level)
setx HOTSHOT_DB_PASSWORD "MyPassword" /M
```

#### macOS/Linux
```bash
# Add to shell profile
echo 'export HOTSHOT_DB_PASSWORD="MyPassword"' >> ~/.bashrc
source ~/.bashrc

# Or for zsh
echo 'export HOTSHOT_DB_PASSWORD="MyPassword"' >> ~/.zshrc
source ~/.zshrc
```

## Exit Codes

- `0`: Success
- `1`: Validation or execution error
- `2`: Permission denied
- `3`: Connection failed

## Support

For issues and questions:
1. Check the troubleshooting section above
2. Review application logs for detailed error messages
3. Ensure all prerequisites are met
4. Verify SQL Server configuration and permissions