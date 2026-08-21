using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup
{
    public class SqlServerProvisioner
    {
        private readonly List<string> _createdArtifacts;
        private readonly ILogger<SqlServerProvisioner> _logger;

        public SqlServerProvisioner(ILogger<SqlServerProvisioner> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createdArtifacts = new List<string>();
        }

        public async Task<bool> ProvisionDatabaseAsync(string connectionString, string databaseName, string loginName,
            string password, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting database provisioning for database: {DatabaseName}", databaseName);

            try
            {
                // First, check if database exists
                bool databaseExists = await DatabaseExistsAsync(connectionString, databaseName, cancellationToken);

                if (!databaseExists)
                {
                    _logger.LogInformation("Database {DatabaseName} does not exist, creating it", databaseName);
                    await CreateDatabaseAsync(connectionString, databaseName, cancellationToken);
                    _createdArtifacts.Add($"Database:{databaseName}");
                }
                else
                {
                    _logger.LogInformation("Database {DatabaseName} already exists", databaseName);
                }

                // Check if login exists
                bool loginExists = await LoginExistsAsync(connectionString, loginName, cancellationToken);

                if (!loginExists)
                {
                    _logger.LogInformation("Login {LoginName} does not exist, creating it", loginName);
                    await CreateLoginAsync(connectionString, loginName, password, cancellationToken);
                    _createdArtifacts.Add($"Login:{loginName}");
                }
                else
                {
                    _logger.LogInformation("Login {LoginName} already exists", loginName);
                }

                // Create user in database if it doesn't exist
                bool userExists =
                    await UserExistsInDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);

                if (!userExists)
                {
                    _logger.LogInformation("User {LoginName} does not exist in database {DatabaseName}, creating it",
                        loginName, databaseName);
                    await CreateUserInDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);
                    _createdArtifacts.Add($"User:{databaseName}:{loginName}");
                }
                else
                {
                    _logger.LogInformation("User {LoginName} already exists in database {DatabaseName}", loginName,
                        databaseName);
                }

                // Grant permissions
                await GrantPermissionsAsync(connectionString, databaseName, loginName, cancellationToken);

                _logger.LogInformation("Database provisioning completed successfully for {DatabaseName}", databaseName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to provision database {DatabaseName}", databaseName);
                await RollbackAsync(connectionString, cancellationToken);
                throw;
            }
        }

        private async Task<bool> DatabaseExistsAsync(string connectionString, string databaseName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = "SELECT database_id FROM sys.databases WHERE name = @dbName";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@dbName", databaseName);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null && result != DBNull.Value;
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

        private async Task<bool> UserExistsInDatabaseAsync(string connectionString, string databaseName,
            string loginName, CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string query =
                "SELECT name FROM sys.database_principals WHERE name = @userName AND type IN ('S', 'U')";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@userName", loginName);

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result != null;
        }

        private async Task CreateDatabaseAsync(string connectionString, string databaseName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Use dynamic SQL with QUOTENAME for safe identifier handling
            const string query = @"
DECLARE @sql NVARCHAR(MAX) = 'CREATE DATABASE ' + QUOTENAME(@dbName);
EXEC sp_executesql @sql, N'@dbName NVARCHAR(128)', @dbName;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@dbName", databaseName);

            try
            {
                await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Created database {DatabaseName}", databaseName);
            }
            catch (SqlException ex) when (ex.Number == 1801) // Database already exists
            {
                _logger.LogWarning("Database already exists: {Message}", ex.Message);
            }
        }

        private async Task CreateLoginAsync(string connectionString, string loginName, string password,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            // CREATE LOGIN does not accept a parameterized password; quote it with QUOTENAME.
            const string query = @"
DECLARE @sql NVARCHAR(MAX) = N'CREATE LOGIN ' + QUOTENAME(@loginName)
    + N' WITH PASSWORD = ' + QUOTENAME(@password, '''');
EXEC sp_executesql @sql;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);
            command.Parameters.AddWithValue("@password", password);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Created login {LoginName}", loginName);
        }

        private async Task CreateUserInDatabaseAsync(string connectionString, string databaseName, string loginName,
            CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = @"
DECLARE @sql NVARCHAR(MAX) = 'CREATE USER ' + QUOTENAME(@loginName) + ' FOR LOGIN ' + QUOTENAME(@loginName);
EXEC sp_executesql @sql, N'@loginName NVARCHAR(128)', @loginName;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Created user {LoginName} in database {DatabaseName}", loginName, databaseName);
        }

        private async Task GrantPermissionsAsync(string connectionString, string databaseName, string loginName,
            CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            // Grant CONNECT permission
            const string connectQuery = @"
DECLARE @sql NVARCHAR(MAX) = 'GRANT CONNECT TO ' + QUOTENAME(@loginName);
EXEC sp_executesql @sql, N'@loginName NVARCHAR(128)', @loginName;
";
            await using (SqlCommand command = new(connectQuery, connection))
            {
                command.Parameters.AddWithValue("@loginName", loginName);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            // Grant DML permissions on dbo schema
            const string dmlQuery = @"
DECLARE @sql NVARCHAR(MAX) = 'GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::dbo TO ' + QUOTENAME(@loginName);
EXEC sp_executesql @sql, N'@loginName NVARCHAR(128)', @loginName;
";
            await using (SqlCommand command = new(dmlQuery, connection))
            {
                command.Parameters.AddWithValue("@loginName", loginName);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Granted permissions to user {LoginName} in database {DatabaseName}", loginName,
                databaseName);
        }

        private async Task RollbackAsync(string connectionString, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting rollback of created artifacts");

            // Rollback in reverse order
            for (int i = _createdArtifacts.Count - 1; i >= 0; i--)
            {
                string artifact = _createdArtifacts[i];
                await RollbackArtifactAsync(connectionString, artifact, cancellationToken);
            }

            _createdArtifacts.Clear();
        }

        private async Task RollbackArtifactAsync(string connectionString, string artifact,
            CancellationToken cancellationToken = default)
        {
            string[] parts = artifact.Split(':');
            string type = parts[0];

            try
            {
                switch (type)
                {
                    case "User":
                        string databaseName = parts[1];
                        string loginName = parts[2];
                        await DropUserFromDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);
                        break;

                    case "Login":
                        string loginName2 = parts[1];
                        await DropLoginAsync(connectionString, loginName2, cancellationToken);
                        break;

                    case "Database":
                        string databaseName2 = parts[1];
                        await DropDatabaseAsync(connectionString, databaseName2, cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback artifact: {Artifact}", artifact);
            }
        }

        private async Task DropUserFromDatabaseAsync(string connectionString, string databaseName, string loginName,
            CancellationToken cancellationToken = default)
        {
            SqlConnectionStringBuilder builder = new(connectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection connection = new(builder.ConnectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = @"
DECLARE @sql NVARCHAR(MAX) = 'DROP USER IF EXISTS ' + QUOTENAME(@loginName);
EXEC sp_executesql @sql, N'@loginName NVARCHAR(128)', @loginName;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Dropped user {LoginName} from database {DatabaseName}", loginName, databaseName);
        }

        private async Task DropLoginAsync(string connectionString, string loginName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = @"
DECLARE @sql NVARCHAR(MAX) = 'DROP LOGIN IF EXISTS ' + QUOTENAME(@loginName);
EXEC sp_executesql @sql, N'@loginName NVARCHAR(128)', @loginName;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Dropped login {LoginName}", loginName);
        }

        private async Task DropDatabaseAsync(string connectionString, string databaseName,
            CancellationToken cancellationToken = default)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync(cancellationToken);

            const string query = @"
DECLARE @sql NVARCHAR(MAX) = 'DROP DATABASE IF EXISTS ' + QUOTENAME(@dbName);
EXEC sp_executesql @sql, N'@dbName NVARCHAR(128)', @dbName;
";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@dbName", databaseName);

            await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation("Dropped database {DatabaseName}", databaseName);
        }
    }
}
