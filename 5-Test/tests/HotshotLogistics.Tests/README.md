# HotshotLogistics.Tests

xUnit test project containing unit, integration and architecture tests.
The tests verify the boundaries defined by Clean Architecture using tools like
xUnit, Moq and NetArchTest.

## Tech Stack
- xUnit
- Moq
- NetArchTest

## Run Tests

```bash
dotnet test
```

SQL-backed repository tests skip unless a connection string is configured.

## Database connection (user secrets)

Store the connection string outside the repo with the API user secrets (same ID the tests use):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj
```

Alternatively, set `CONNECTIONSTRINGS__DEFAULTCONNECTION` in the process environment (CI). Environment variables override user secrets.

If user secrets (or the environment) include `ConnectionStrings:AppConfig`, tests load Azure App Configuration next. They do not use `DefaultAzureCredential`, so an endpoint-only setup is ignored and user secrets remain the source.

Do not put connection strings in `.env` files or source under this repository.
