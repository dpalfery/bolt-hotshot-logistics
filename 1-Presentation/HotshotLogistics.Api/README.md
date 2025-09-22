# HotshotLogistics.Api

Azure Functions API for the Hotshot Logistics platform.

## Tech Stack

- Azure Functions v4 (.NET 8)
- Native ADO.NET (Microsoft.Data.SqlClient) with FluentMigrator-managed migrations (Entity Framework is prohibited)
- SQL Server via Microsoft provider
- Azure App Configuration
- Azure Key Vault

## Development

```bash
dotnet build
dotnet run
```

## Configuration

Set up your connection string in `local.settings.json` or use Azure App Configuration.

The API listens on port 7060 by default.

