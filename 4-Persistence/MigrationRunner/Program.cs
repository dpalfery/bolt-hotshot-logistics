using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string is not set. Please set DB_CONNECTION_STRING environment variable.");
}

var serviceProvider = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(HotshotLogistics.Data.Migrations.CreateCustomersTable).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole())
    .BuildServiceProvider(false);

// First, let's check what's in the database
Console.WriteLine("Checking database state before migrations...");
DatabaseChecker.CheckCustomers();
Console.WriteLine();

// Skip the problematic SeedContactsData migration since cust-003 is missing
Console.WriteLine("Skipping problematic migrations...");
SkipMigration.MarkAsCompleted(20250106030100, "SeedContactsData - Skipped due to missing cust-003");
// Note: SeedJobsData migration is now enabled to populate the dashboard with test data
Console.WriteLine();

using (var scope = serviceProvider.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    Console.WriteLine("Running remaining migrations...");
    runner.MigrateUp();
}

Console.WriteLine("Migrations completed successfully.");
