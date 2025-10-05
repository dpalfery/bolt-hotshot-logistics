<#
.SYNOPSIS
    Creates an application user for the Hotshot Logistics database.

.DESCRIPTION
    This script provisions an application user account for the hotshot_logistic database
    using a privileged connection. The script reads credentials from environment variables
    and creates both server login and database user with appropriate permissions.

    SAFETY/OPS NOTES:
    - Run this script from a secure workstation only
    - HSL.SA_CONNECTION_STRING should be provided from Key Vault or CI secret injection
    - This script uses placeholders only; no real secrets are stored in the repository
    - Requires sqlcmd to be installed and available in PATH

.PARAMETER None
    All configuration is read from environment variables.

.EXAMPLE
    # Set environment variables (replace with actual values)
    $env:HSL.DBUser = "hotshot_app"
    $env:HSL.DB_Password = "SecurePassword123!"
    $env:HSL.SA_CONNECTION_STRING = "Server=myserver.database.windows.net;Database=master;User Id=sa;Password=saPassword;"

    # Run the script
    .\create-app-user.ps1

.NOTES
    Required Environment Variables:
    - HSL.DBUser: The application username to create (e.g., hotshot_app)
    - HSL.DB_Password: The application user's password
    - HSL.SA_CONNECTION_STRING: Privileged connection string with SA credentials

    Exit Codes:
    - 0: Success
    - 1: Missing required environment variables
    - 2: SQL execution failed
#>

# Stop on any error
$ErrorActionPreference = "Stop"

# Function to write safe log messages (no secrets)
function Write-SafeLog {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] $Message"
}

# Function to validate environment variables
function Test-EnvironmentVariables {
    $requiredVars = @("HSL.DBUser", "HSL.DB_Password", "HSL.SA_CONNECTION_STRING")

    foreach ($var in $requiredVars) {
        if (-not (Test-Path "env:$var")) {
            Write-Error "Missing required environment variable: $var"
            Write-Host "Please set the following environment variables:"
            Write-Host "  - HSL.DBUser: Application username to create"
            Write-Host "  - HSL.DB_Password: Application user password"
            Write-Host "  - HSL.SA_CONNECTION_STRING: Privileged connection string"
            exit 1
        }
    }
}

# Function to parse connection string and extract components
function Get-ConnectionStringComponents {
    param([string]$ConnectionString)

    $components = @{}

    # Split by semicolon and parse each component
    $pairs = $ConnectionString -split ';' | Where-Object { $_ -ne "" }

    foreach ($pair in $pairs) {
        $key, $value = $pair -split '=', 2
        if ($key -and $value) {
            $components[$key.Trim()] = $value.Trim()
        }
    }

    # Validate required components
    $requiredKeys = @("Server", "User Id", "Password")
    foreach ($key in $requiredKeys) {
        if (-not $components.ContainsKey($key)) {
            Write-Error "Missing required component in connection string: $key"
            exit 1
        }
    }

    return $components
}

# Function to escape single quotes for T-SQL
function Escape-SqlString {
    param([string]$Value)
    return $Value.Replace("'", "''")
}

# Main execution
try {
    Write-SafeLog "Starting application user creation..."

    # Validate environment variables
    Test-EnvironmentVariables

    # Get environment variables
    $dbUser = $env:HSL.DBUser
    $dbPassword = $env:HSL.DB_Password
    $saConnectionString = $env:HSL.SA_CONNECTION_STRING

    Write-SafeLog "Environment variables validated successfully"

    # Parse connection string
    $connComponents = Get-ConnectionStringComponents -ConnectionString $saConnectionString

    # Build connection string for sqlcmd (without database for master operations)
    $sqlcmdConnectionString = "Server=$($connComponents['Server']);User Id=$($connComponents['User Id']);Password=$($connComponents['Password'])"

    Write-SafeLog "Connection string parsed successfully"

    # Escape values for SQL injection prevention
    $escapedDbUser = Escape-SqlString -Value $dbUser
    $escapedDbPassword = Escape-SqlString -Value $dbPassword

    # Step 1: Create server login if not exists
    Write-SafeLog "Creating server login for user: $dbUser"

    $createLoginSql = @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$escapedDbUser')
BEGIN
    CREATE LOGIN [$escapedDbUser] WITH PASSWORD = N'$escapedDbPassword';
END
"@

    $sqlcmdArgs = @(
        "-S", $connComponents['Server'],
        "-U", $connComponents['User Id'],
        "-P", $connComponents['Password'],
        "-Q", $createLoginSql
    )

    # Execute login creation
    $result = & sqlcmd $sqlcmdArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create server login. Exit code: $LASTEXITCODE"
        Write-Error "Error details: $($result | Out-String)"
        exit 2
    }

    Write-SafeLog "Server login created successfully"

    # Step 2: Create database user and grant permissions
    Write-SafeLog "Creating database user and granting permissions"

    $createUserSql = @"
USE [hotshot_logistic];
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$escapedDbUser')
BEGIN
    CREATE USER [$escapedDbUser] FOR LOGIN [$escapedDbUser];
END
GRANT CONNECT TO [$escapedDbUser];
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[dbo] TO [$escapedDbUser];
"@

    $sqlcmdArgs = @(
        "-S", $connComponents['Server'],
        "-U", $connComponents['User Id'],
        "-P", $connComponents['Password'],
        "-Q", $createUserSql
    )

    # Execute user creation and permission grants
    $result = & sqlcmd $sqlcmdArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create database user or grant permissions. Exit code: $LASTEXITCODE"
        Write-Error "Error details: $($result | Out-String)"
        exit 2
    }

    Write-SafeLog "Database user created and permissions granted successfully"
    Write-SafeLog "Application user '$dbUser' is ready for use"

    exit 0

} catch {
    Write-Error "An unexpected error occurred: $($_.Exception.Message)"
    Write-Error "Stack trace: $($_.ScriptStackTrace)"
    exit 2
}