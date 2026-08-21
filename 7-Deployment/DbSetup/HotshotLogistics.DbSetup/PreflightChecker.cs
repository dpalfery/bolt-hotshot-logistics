using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup
{
    public class PreflightChecker
    {
        private readonly ILogger<PreflightChecker> _logger;

        public PreflightChecker(ILogger<PreflightChecker> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> PerformPreflightChecksAsync(string connectionString, string databaseName,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting preflight checks for database: {DatabaseName}", databaseName);

            List<(string Name, Func<Task<bool>> Check)> checks =
            [
                ("SQL Server Connectivity", () => CheckSqlServerConnectivityAsync(connectionString, cancellationToken)),
                ("Privileged Credentials", () => CheckPrivilegedCredentialsAsync(connectionString, cancellationToken)),
                ("Create Database Permission",
                    () => CheckCreateDatabasePermissionAsync(connectionString, cancellationToken)),
                ("Create Login Permission", () => CheckCreateLoginPermissionAsync(connectionString, cancellationToken)),
                ("Database Access", () => CheckDatabaseAccessAsync(connectionString, databaseName, cancellationToken))
            ];

            bool allPassed = true;

            foreach ((string name, Func<Task<bool>> check) in checks)
            {
                try
                {
                    _logger.LogDebug("Running preflight check: {CheckName}", name);
                    bool passed = await check();

                    if (passed)
                    {
                        _logger.LogInformation("✓ {CheckName} passed", name);
                    }
                    else
                    {
                        _logger.LogError("✗ {CheckName} failed", name);
                        allPassed = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "✗ {CheckName} failed with exception", name);
                    allPassed = false;
                }
            }

            if (allPassed)
            {
                _logger.LogInformation("All preflight checks passed");
            }
            else
            {
                _logger.LogError("Some preflight checks failed. Please review the errors above.");
            }

            return allPassed;
        }

        private async Task<bool> CheckSqlServerConnectivityAsync(string connectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqlConnection connection = new(connectionString);
                await connection.OpenAsync(cancellationToken);

                // Test with a simple query
                const string query = "SELECT SERVERPROPERTY('ProductVersion')";
                await using SqlCommand command = new(query, connection);
                object result = await command.ExecuteScalarAsync(cancellationToken);

                _logger.LogDebug("SQL Server connectivity test successful. Version: {Version}", result);
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError("SQL Server connectivity failed: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during SQL Server connectivity check");
                return false;
            }
        }

        private async Task<bool> CheckPrivilegedCredentialsAsync(string connectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqlConnection connection = new(connectionString);
                await connection.OpenAsync(cancellationToken);

                // Check if we have sysadmin or sufficient privileges
                const string query = @"
                SELECT
                    IS_SRVROLEMEMBER('sysadmin') as IsSysAdmin,
                    IS_SRVROLEMEMBER('serveradmin') as IsServerAdmin,
                    HAS_PERMS_BY_NAME(null, null, 'CREATE ANY DATABASE') as CanCreateDatabase,
                    HAS_PERMS_BY_NAME(null, null, 'ALTER ANY LOGIN') as CanAlterLogin";

                await using SqlCommand command = new(query, connection);
                await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

                if (await reader.ReadAsync(cancellationToken))
                {
                    bool isSysAdmin = reader.GetBoolean(0);
                    bool isServerAdmin = reader.GetBoolean(1);
                    bool canCreateDatabase = reader.GetBoolean(2);
                    bool canAlterLogin = reader.GetBoolean(3);

                    _logger.LogDebug(
                        "Privilege check results: SysAdmin={IsSysAdmin}, ServerAdmin={IsServerAdmin}, CanCreateDatabase={CanCreateDatabase}, CanAlterLogin={CanAlterLogin}",
                        isSysAdmin, isServerAdmin, canCreateDatabase, canAlterLogin);

                    // We need either sysadmin, serveradmin, or specific permissions
                    return isSysAdmin || isServerAdmin || (canCreateDatabase && canAlterLogin);
                }

                return false;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Privileged credentials check failed: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during privileged credentials check");
                return false;
            }
        }

        private async Task<bool> CheckCreateDatabasePermissionAsync(string connectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqlConnection connection = new(connectionString);
                await connection.OpenAsync(cancellationToken);

                // Try to create a test database (we'll drop it immediately)
                string testDbName = $"TestDb_{Guid.NewGuid():N}";

                string createQuery = $"CREATE DATABASE {GetQuotedIdentifier(testDbName)}";
                await using (SqlCommand command = new(createQuery, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Clean up the test database
                string dropQuery = $"DROP DATABASE {GetQuotedIdentifier(testDbName)}";
                await using (SqlCommand command = new(dropQuery, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                _logger.LogDebug("Create database permission test successful");
                return true;
            }
            catch (SqlException ex) when (ex.Number == 262) // Database already exists
            {
                // This shouldn't happen with GUID, but handle it gracefully
                _logger.LogWarning("Test database already exists, skipping cleanup");
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Create database permission check failed: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during create database permission check");
                return false;
            }
        }

        private async Task<bool> CheckCreateLoginPermissionAsync(string connectionString,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using SqlConnection connection = new(connectionString);
                await connection.OpenAsync(cancellationToken);

                // Try to create a test login (we'll drop it immediately)
                string testLoginName = $"TestLogin_{Guid.NewGuid():N}";
                string testPassword = "TempPassword123!";

                string createQuery = $"CREATE LOGIN {GetQuotedIdentifier(testLoginName)} WITH PASSWORD = @password";
                await using (SqlCommand command = new(createQuery, connection))
                {
                    command.Parameters.AddWithValue("@password", testPassword);
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Clean up the test login
                string dropQuery = $"DROP LOGIN {GetQuotedIdentifier(testLoginName)}";
                await using (SqlCommand command = new(dropQuery, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                _logger.LogDebug("Create login permission test successful");
                return true;
            }
            catch (SqlException ex) when (ex.Number == 15025) // Login already exists
            {
                // This shouldn't happen with GUID, but handle it gracefully
                _logger.LogWarning("Test login already exists, skipping cleanup");
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Create login permission check failed: {ErrorMessage}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during create login permission check");
                return false;
            }
        }

        private async Task<bool> CheckDatabaseAccessAsync(string connectionString, string databaseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                SqlConnectionStringBuilder builder = new(connectionString)
                {
                    InitialCatalog = databaseName
                };

                await using SqlConnection connection = new(builder.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Test basic connectivity to the target database
                const string query = "SELECT DB_NAME()";
                await using SqlCommand command = new(query, connection);
                object? result = await command.ExecuteScalarAsync(cancellationToken);

                string? actualDbName = result?.ToString();
                if (actualDbName != databaseName)
                {
                    _logger.LogWarning("Connected to database '{ActualDbName}' but expected '{ExpectedDbName}'",
                        actualDbName, databaseName);
                }

                _logger.LogDebug("Database access test successful for database: {DatabaseName}", databaseName);
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError("Database access check failed for {DatabaseName}: {ErrorMessage}", databaseName,
                    ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during database access check for {DatabaseName}", databaseName);
                return false;
            }
        }

        public string GetRemediationInstructions()
        {
            return @"
To resolve privilege issues:

1. Ensure you're using SQL Server authentication with sufficient privileges
2. Use 'sa' account or an account with:
   - sysadmin role, OR
   - serveradmin role, OR
   - Both CREATE ANY DATABASE and ALTER ANY LOGIN permissions

3. For Windows authentication, ensure your Windows account has sufficient SQL Server privileges

4. In non-interactive mode, provide --sa-connection-string with privileged credentials

5. For Azure SQL Database, ensure the account has the necessary Azure RBAC roles

Example connection strings:
- Windows: Server=myserver;Database=master;Trusted_Connection=True;
- SQL Server: Server=myserver;Database=master;User Id=sa;Password=mypassword;
- Azure SQL: Server=myserver.database.windows.net;Database=master;User Id=myuser;Password=mypassword;
";
        }

        private static string GetQuotedIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }
    }
}
