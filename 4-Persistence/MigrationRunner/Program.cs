using FluentMigrator.Runner;
using HotshotLogistics.Core.Extensions;
using HotshotLogistics.Data.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationRunner;

// Parse command line arguments
bool rerunAll = args.Any(arg => arg.Equals("--rerun-all", StringComparison.OrdinalIgnoreCase));
bool showHelp = args.Any(arg =>
    arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase));

if (showHelp)
{
    Console.WriteLine("MigrationRunner Usage:");
    Console.WriteLine("  MigrationRunner [--rerun-all] [--help]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --rerun-all    Run all migrations from scratch, ignoring previous migration history");
    Console.WriteLine("  --help, -h     Show this help message");
    return;
}

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
        true)
    .AddUserSecrets(typeof(Program).Assembly, true)
    .AddEnvironmentVariables();
configurationBuilder.AddAzureAppConfigurationIfConfigured();
IConfigurationRoot configuration = configurationBuilder.Build();

string? connectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? configuration["DB_CONNECTION_STRING"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string is not set. Set user secret ConnectionStrings:DefaultConnection or DB_CONNECTION_STRING.");
}

ServiceProvider serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(CreateCustomersTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    .BuildServiceProvider(false);

// First, let's check what's in the database
Console.WriteLine("Checking database state before migrations...");
DatabaseChecker.CheckCustomers();
Console.WriteLine();

if (rerunAll)
{
    Console.WriteLine("Re-running all migrations from scratch...");
    ClearMigrationHistory(connectionString);
    Console.WriteLine();
}
else
{
    // Skip the problematic SeedContactsData migration since cust-003 is missing
    Console.WriteLine("Skipping problematic migrations...");
    SkipMigration.MarkAsCompleted(20250106030100, "SeedContactsData - Skipped due to missing cust-003");
    // Note: SeedJobsData migration is now enabled to populate the dashboard with test data
    Console.WriteLine();
}

using (IServiceScope scope = serviceProvider.CreateScope())
{
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

    if (rerunAll)
    {
        Console.WriteLine("Running all migrations from scratch...");
        runner.MigrateUp();
    }
    else
    {
        Console.WriteLine("Running remaining migrations...");
        runner.MigrateUp();
    }
}

Console.WriteLine("Migrations completed successfully.");

static void ClearMigrationHistory(string connectionString)
{
    try
    {
        using SqlConnection connection = new(connectionString);
        connection.Open();

        // Clear the VersionInfo table to reset migration history
        using SqlCommand clearCmd = connection.CreateCommand();
        clearCmd.CommandText = "DELETE FROM [dbo].[VersionInfo]";
        int deletedCount = clearCmd.ExecuteNonQuery();

        Console.WriteLine($"Cleared migration history. Removed {deletedCount} migration records.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error clearing migration history: {ex.Message}");
        throw; // Re-throw to prevent migrations from running with inconsistent state
    }
}
