using Microsoft.Extensions.Logging;
using HotshotLogistics.DbSetup;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();

try
{
    var commandLineArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();
    var parser = new ArgumentParser(commandLineArgs);
    parser.ApplyEnvironmentOverrides();

    if (!parser.Validate(out var errorMessage))
    {
        logger.LogError("Validation failed: {ErrorMessage}", errorMessage);
        Environment.Exit(1);
    }

    logger.LogInformation("Starting Hotshot Logistics Database Setup");
    logger.LogInformation("Server: {Server}", parser.Server);
    logger.LogInformation("Database: {Database}", parser.DatabaseName);
    logger.LogInformation("App User: {AppUser}", parser.AppUser);

    // TODO: Implement the rest of the components
    // - EnvironmentManager
    // - PasswordManager
    // - SqlServerProvisioner
    // - PermissionManager
    // - MigrationInvoker
    // - Interactive prompts

    Console.WriteLine("Database setup completed successfully!");
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during database setup");
    Environment.Exit(1);
}
