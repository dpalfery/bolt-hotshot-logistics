# HotshotLogistics.Api

ASP.NET Core Web API for the Hotshot Logistics platform, hosted on Azure Container Apps.

## Tech Stack

- ASP.NET Core Web API (.NET 8)
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

Set up your connection string in `appsettings.json` or use Azure App Configuration and environment variables.

The API listens on port 7060 by default.

