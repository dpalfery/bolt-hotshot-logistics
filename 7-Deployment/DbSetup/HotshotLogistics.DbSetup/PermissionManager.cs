using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup
{
    public class PermissionManager
    {
        private readonly ILogger<PermissionManager> _logger;

        public PermissionManager(ILogger<PermissionManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SetupApplicationPermissionsAsync(string connectionString, string databaseName,
            string appUserName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting up application permissions for user {UserName} in database {DatabaseName}",
                appUserName, databaseName);

            // First ensure the login exists at the server level
            bool loginExists = await LoginExistsAsync(connectionString, appUserName, cancellationToken);
            if (!loginExists)
            {
                _logger.LogInformation("Login {UserName} does not exist, creating it", appUserName);
                await CreateLoginAsync(connectionString, appUserName, cancellationToken);
            }

            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // First ensure the user exists in this database
            bool userExists = await UserExistsInDatabaseAsync(connection, appUserName, cancellationToken);
            if (!userExists)
            {
                _logger.LogInformation("User {UserName} does not exist in database {DatabaseName}, creating it",
                    appUserName, databaseName);
                await CreateUserInDatabaseAsync(connection, appUserName, cancellationToken);
            }

            // Grant CONNECT permission
            await GrantConnectPermissionAsync(connection, appUserName, cancellationToken);

            // Grant DML permissions on dbo schema
            await GrantDmlPermissionsAsync(connection, appUserName, cancellationToken);

            // Grant EXECUTE permissions on stored procedures if they exist
            await GrantExecutePermissionsAsync(connection, appUserName, cancellationToken);

            _logger.LogInformation("Application permissions setup completed for user {UserName}", appUserName);
        }

        public async Task SetupMigrationPermissionsAsync(string connectionString, string databaseName,
            string migrationUserName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Setting up migration permissions for user {UserName} in database {DatabaseName}",
                migrationUserName, databaseName);

            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Grant db_owner role for migrations (elevated permissions)
            await GrantDbOwnerRoleAsync(connection, migrationUserName, cancellationToken);

            _logger.LogInformation("Migration permissions setup completed for user {UserName}", migrationUserName);
        }

        public async Task RevokeMigrationPermissionsAsync(string connectionString, string databaseName,
            string migrationUserName, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Revoking elevated migration permissions for user {UserName} in database {DatabaseName}",
                migrationUserName, databaseName);

            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Revoke db_owner role after migrations
            await RevokeDbOwnerRoleAsync(connection, migrationUserName, cancellationToken);

            _logger.LogInformation("Migration permissions revoked for user {UserName}", migrationUserName);
        }

        public async Task<bool> CanUserConnectAsync(string connectionString, string databaseName, string userName,
            string password, CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName,
                UserID = userName,
                Password = password
            };

            try
            {
                await using SqlConnection connection = new(builder.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Test a simple query to verify permissions
                const string testQuery = "SELECT 1";
                await using SqlCommand command = new(testQuery, connection);
                object? result = await command.ExecuteScalarAsync(cancellationToken);

                return result != null && (int)result == 1;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User {UserName} cannot connect to database {DatabaseName}", userName,
                    databaseName);
                return false;
            }
        }

        public async Task<bool> CanUserPerformDmlAsync(string connectionString, string databaseName, string userName,
            string password, CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName,
                UserID = userName,
                Password = password
            };

            try
            {
                await using SqlConnection connection = new(builder.ConnectionString);
                await connection.OpenAsync(cancellationToken);

                // Try to create a test table (this tests DML permissions)
                string testTableName = $"TestPermissions_{Guid.NewGuid():N}";

                string createQuery = $"CREATE TABLE {GetQuotedIdentifier(testTableName)} (Id INT PRIMARY KEY)";
                await using (SqlCommand command = new(createQuery, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                // Clean up the test table
                string dropQuery = $"DROP TABLE {GetQuotedIdentifier(testTableName)}";
                await using (SqlCommand command = new(dropQuery, connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "User {UserName} cannot perform DML operations in database {DatabaseName}",
                    userName, databaseName);
                return false;
            }
        }

        private async Task GrantConnectPermissionAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            string query = $"GRANT CONNECT TO {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Granted CONNECT permission to user {UserName}", userName);
        }

        private async Task<bool> UserExistsInDatabaseAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            const string query =
                "SELECT name FROM sys.database_principals WHERE name = @userName AND type IN ('S', 'U')";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@userName", userName);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }

        private async Task CreateUserInDatabaseAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            // For this to work, the login must already exist at the server level
            string query = $"CREATE USER {GetQuotedIdentifier(userName)} FOR LOGIN {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Created user {UserName} in database", userName);
        }

        private async Task<bool> LoginExistsAsync(string connectionString, string loginName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            const string query =
                "SELECT name FROM sys.server_principals WHERE name = @loginName AND type IN ('S', 'U')";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }

        private async Task CreateLoginAsync(string connectionString, string loginName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Generate a temporary password for the login
            string password = GenerateTemporaryPassword();

            // SQL Server doesn't support parameterized passwords in CREATE LOGIN
            // Use dynamic SQL with proper escaping for the password
            string escapedPassword = password.Replace("'", "''");
            string query = $"CREATE LOGIN {GetQuotedIdentifier(loginName)} WITH PASSWORD = '{escapedPassword}'";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Created login {LoginName}", loginName);
        }

        private static string GenerateTemporaryPassword()
        {
            // Generate a random password for testing purposes
            Random random = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            return new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task GrantDmlPermissionsAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            string query = $"GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Granted DML permissions on dbo schema to user {UserName}", userName);
        }

        private async Task GrantExecutePermissionsAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            string query = $"GRANT EXECUTE ON SCHEMA::dbo TO {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Granted EXECUTE permissions on dbo schema to user {UserName}", userName);
        }

        private async Task GrantDbOwnerRoleAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            // First check if user exists
            string checkQuery = "SELECT name FROM sys.database_principals WHERE name = @userName";
            await using (SqlCommand checkCommand = new(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@userName", userName);
                object? result = await checkCommand.ExecuteScalarAsync(cancellationToken);

                if (result == null)
                {
                    throw new InvalidOperationException($"User {userName} does not exist in the database");
                }
            }

            // Add user to db_owner role
            string query = $"ALTER ROLE db_owner ADD MEMBER {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogDebug("Added user {UserName} to db_owner role", userName);
        }

        private async Task RevokeDbOwnerRoleAsync(SqlConnection connection, string userName,
            CancellationToken cancellationToken = default)
        {
            string query = $"ALTER ROLE db_owner DROP MEMBER {GetQuotedIdentifier(userName)}";
            await using SqlCommand command = new(query, connection);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogDebug("Removed user {UserName} from db_owner role", userName);
            }
            catch (SqlException ex) when (ex.Number == 15151) // Role member does not exist
            {
                _logger.LogDebug("User {UserName} was not a member of db_owner role", userName);
            }
        }

        private static string GetQuotedIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }
    }
}
