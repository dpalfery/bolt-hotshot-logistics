using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup;

public class SqlServerProvisioner
{
    private readonly ILogger<SqlServerProvisioner> _logger;
    private readonly List<string> _createdArtifacts;

    public SqlServerProvisioner(ILogger<SqlServerProvisioner> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createdArtifacts = new List<string>();
    }

    public async Task<bool> ProvisionDatabaseAsync(string connectionString, string databaseName, string loginName, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting database provisioning for database: {DatabaseName}", databaseName);

        try
        {
            // First, check if database exists
            var databaseExists = await DatabaseExistsAsync(connectionString, databaseName, cancellationToken);

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
            var loginExists = await LoginExistsAsync(connectionString, loginName, cancellationToken);

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
            var userExists = await UserExistsInDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);

            if (!userExists)
            {
                _logger.LogInformation("User {LoginName} does not exist in database {DatabaseName}, creating it", loginName, databaseName);
                await CreateUserInDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);
                _createdArtifacts.Add($"User:{databaseName}:{loginName}");
            }
            else
            {
                _logger.LogInformation("User {LoginName} already exists in database {DatabaseName}", loginName, databaseName);
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

    public async Task<bool> DatabaseExistsAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "SELECT database_id FROM sys.databases WHERE name = @dbName";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@dbName", databaseName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null && result != DBNull.Value;
    }

    public async Task<bool> LoginExistsAsync(string connectionString, string loginName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "SELECT name FROM sys.server_principals WHERE name = @loginName AND type IN ('S', 'U')";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@loginName", loginName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }

    public async Task<bool> UserExistsInDatabaseAsync(string connectionString, string databaseName, string loginName, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        const string query = "SELECT name FROM sys.database_principals WHERE name = @userName AND type IN ('S', 'U')";
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@userName", loginName);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }

    private async Task CreateDatabaseAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // Use QUOTENAME for safe identifier handling
        var query = $"CREATE DATABASE {GetQuotedIdentifier(databaseName)}";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Created database {DatabaseName}", databaseName);
    }

    private async Task CreateLoginAsync(string connectionString, string loginName, string password, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        // SQL Server doesn't support parameterized passwords in CREATE LOGIN
        // Use dynamic SQL with proper escaping for the password
        var escapedPassword = password.Replace("'", "''");
        var query = $"CREATE LOGIN {GetQuotedIdentifier(loginName)} WITH PASSWORD = '{escapedPassword}'";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Created login {LoginName}", loginName);
    }

    private async Task CreateUserInDatabaseAsync(string connectionString, string databaseName, string loginName, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var query = $"CREATE USER {GetQuotedIdentifier(loginName)} FOR LOGIN {GetQuotedIdentifier(loginName)}";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Created user {LoginName} in database {DatabaseName}", loginName, databaseName);
    }

    private async Task GrantPermissionsAsync(string connectionString, string databaseName, string loginName, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        // Grant CONNECT permission
        var connectQuery = $"GRANT CONNECT TO {GetQuotedIdentifier(loginName)}";
        await using (var command = new SqlCommand(connectQuery, connection))
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        // Grant DML permissions on dbo schema
        var dmlQuery = $"GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO {GetQuotedIdentifier(loginName)}";
        await using (var command = new SqlCommand(dmlQuery, connection))
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        _logger.LogInformation("Granted permissions to user {LoginName} in database {DatabaseName}", loginName, databaseName);
    }

    private async Task RollbackAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting rollback of created artifacts");

        // Rollback in reverse order
        for (int i = _createdArtifacts.Count - 1; i >= 0; i--)
        {
            var artifact = _createdArtifacts[i];
            await RollbackArtifactAsync(connectionString, artifact, cancellationToken);
        }

        _createdArtifacts.Clear();
    }

    private async Task RollbackArtifactAsync(string connectionString, string artifact, CancellationToken cancellationToken = default)
    {
        var parts = artifact.Split(':');
        var type = parts[0];

        try
        {
            switch (type)
            {
                case "User":
                    var databaseName = parts[1];
                    var loginName = parts[2];
                    await DropUserFromDatabaseAsync(connectionString, databaseName, loginName, cancellationToken);
                    break;

                case "Login":
                    var loginName2 = parts[1];
                    await DropLoginAsync(connectionString, loginName2, cancellationToken);
                    break;

                case "Database":
                    var databaseName2 = parts[1];
                    await DropDatabaseAsync(connectionString, databaseName2, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback artifact: {Artifact}", artifact);
        }
    }

    private async Task DropUserFromDatabaseAsync(string connectionString, string databaseName, string loginName, CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = databaseName
        };

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var query = $"DROP USER IF EXISTS {GetQuotedIdentifier(loginName)}";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Dropped user {LoginName} from database {DatabaseName}", loginName, databaseName);
    }

    private async Task DropLoginAsync(string connectionString, string loginName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var query = $"DROP LOGIN IF EXISTS {GetQuotedIdentifier(loginName)}";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Dropped login {LoginName}", loginName);
    }

    private async Task DropDatabaseAsync(string connectionString, string databaseName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var query = $"DROP DATABASE IF EXISTS {GetQuotedIdentifier(databaseName)}";
        await using var command = new SqlCommand(query, connection);

        await command.ExecuteNonQueryAsync(cancellationToken);
        _logger.LogInformation("Dropped database {DatabaseName}", databaseName);
    }

    private static string GetQuotedIdentifier(string identifier)
    {
        return $"[{identifier.Replace("]", "]]")}]";
    }
}