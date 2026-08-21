using System.CommandLine;

namespace HotshotLogistics.DbSetup
{
    public class ArgumentParser
    {
        public ArgumentParser(string[] args)
        {
            if (args.Contains("--help") || args.Contains("-h") || args.Contains("-?"))
            {
                ShowHelp();
                Environment.Exit(0);
            }

            Option<string> serverOption = new("--server")
            {
                Description = "SQL Server instance (e.g., localhost\\SQLEXPRESS)"
            };

            Option<string> saConnectionStringOption = new("--sa-connection-string")
            {
                Description = "SA or privileged connection string for non-interactive mode"
            };

            Option<string> databaseNameOption = new("--db-name")
            {
                Description = "Target database name",
                DefaultValueFactory = _ => "hotshot_logistics"
            };

            Option<string> appUserOption = new("--app-user")
            {
                Description = "Application database user/login name",
                DefaultValueFactory = _ => "hotshot_app"
            };

            Option<string> passwordOption = new("--password")
            {
                Description = "Application user password (not recommended for CI; prefer env var)"
            };

            Option<bool> nonInteractiveOption = new("--non-interactive")
            {
                Description = "Run without interactive prompts for CI"
            };

            Option<bool> forceOption = new("--force")
            {
                Description = "Allow destructive operations"
            };

            Option<bool> persistEnvironmentOption = new("--persist-env")
            {
                Description = "Persist password to system environment variable (requires explicit consent)"
            };

            RootCommand rootCommand = new("Hotshot Logistics Database Setup CLI");
            rootCommand.Options.Add(serverOption);
            rootCommand.Options.Add(saConnectionStringOption);
            rootCommand.Options.Add(databaseNameOption);
            rootCommand.Options.Add(appUserOption);
            rootCommand.Options.Add(passwordOption);
            rootCommand.Options.Add(nonInteractiveOption);
            rootCommand.Options.Add(forceOption);
            rootCommand.Options.Add(persistEnvironmentOption);

            rootCommand.SetAction(parseResult =>
            {
                Server = parseResult.GetValue(serverOption);
                SaConnectionString = parseResult.GetValue(saConnectionStringOption);
                DatabaseName = parseResult.GetValue(databaseNameOption);
                AppUser = parseResult.GetValue(appUserOption);
                Password = parseResult.GetValue(passwordOption);
                NonInteractive = parseResult.GetValue(nonInteractiveOption);
                Force = parseResult.GetValue(forceOption);
                PersistEnvironment = parseResult.GetValue(persistEnvironmentOption);
            });

            rootCommand.Parse(args).Invoke();
        }

        public string? Server { get; private set; }
        public string? SaConnectionString { get; private set; }
        public string? DatabaseName { get; private set; }
        public string? AppUser { get; private set; }
        public string? Password { get; private set; }
        public bool NonInteractive { get; private set; }
        public bool Force { get; private set; }
        public bool PersistEnvironment { get; private set; }

        private static void ShowHelp()
        {
            Console.WriteLine("Description:");
            Console.WriteLine("  Hotshot Logistics Database Setup CLI");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  HotshotLogistics.DbSetup [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine(
                "  --server <server>                              SQL Server instance (e.g., localhost\\SQLEXPRESS)");
            Console.WriteLine(
                "  --sa-connection-string <sa-connection-string>  SA or privileged connection string for non-interactive mode");
            Console.WriteLine(
                "  --db-name <db-name>                            Target database name [default: hotshot_logistics]");
            Console.WriteLine(
                "  --app-user <app-user>                          Application database user/login name [default: hotshot_app]");
            Console.WriteLine(
                "  --password <password>                          Application user password (not recommended for CI; prefer env var)");
            Console.WriteLine(
                "  --non-interactive                              Run without interactive prompts for CI");
            Console.WriteLine("  --force                                        Allow destructive operations");
            Console.WriteLine(
                "  --persist-env                                  Persist password to system environment variable (requires explicit consent)");
            Console.WriteLine("  --version                                      Show version information");
            Console.WriteLine("  -?, -h, --help                                 Show help and usage information");
        }

        public void ApplyEnvironmentOverrides()
        {
            // Apply environment variable fallbacks if CLI args not provided or using defaults
            Server ??= Environment.GetEnvironmentVariable("HOTSHOT_DB_SERVER");

            // Apply env var if DatabaseName is still the default value
            if (DatabaseName == "hotshot_logistics")
            {
                string? envDbName = Environment.GetEnvironmentVariable("HOTSHOT_DB_NAME");
                if (!string.IsNullOrEmpty(envDbName))
                {
                    DatabaseName = envDbName;
                }
            }

            // Apply env var if AppUser is still the default value
            if (AppUser == "hotshot_app")
            {
                string? envAppUser = Environment.GetEnvironmentVariable("HOTSHOT_DB_APP_USER");
                if (!string.IsNullOrEmpty(envAppUser))
                {
                    AppUser = envAppUser;
                }
            }

            Password ??= Environment.GetEnvironmentVariable("HOTSHOT_DB_PASSWORD");
            SaConnectionString ??= Environment.GetEnvironmentVariable("CI_SA_CONNECTION_STRING");
        }

        public bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (NonInteractive && string.IsNullOrEmpty(SaConnectionString))
            {
                errorMessage =
                    "SA connection string is required in non-interactive mode. Provide --sa-connection-string or set CI_SA_CONNECTION_STRING environment variable.";
                return false;
            }

            if (string.IsNullOrEmpty(Server))
            {
                errorMessage = "Server is required. Provide --server or set HOTSHOT_DB_SERVER environment variable.";
                return false;
            }

            return true;
        }
    }
}
