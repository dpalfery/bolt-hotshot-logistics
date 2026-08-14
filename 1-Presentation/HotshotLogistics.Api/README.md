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

Local values come from .NET user secrets. Azure App Configuration is added only when it is configured.

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj
```

Optional App Configuration (either key). If neither is set, user secrets and environment variables are used as-is:

```bash
dotnet user-secrets set "AppConfiguration:Endpoint" "https://<store-name>.azconfig.io" --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj
# or
dotnet user-secrets set "ConnectionStrings:AppConfig" "<app-config-connection-string>" --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj
```

The API listens on port 7060 by default.

