---
id: technology-stack
title: Technology stack
doc-type: reference
status: current
owner: unassigned
last-reviewed: 2026-08-14
---

# Technology Stack

## Backend (.NET)

- **Framework**: .NET 8 ASP.NET Core Web API (Azure Container Apps)
- **Database**: SQL Server with native ADO.NET (Entity Framework is prohibited)
- **Testing**: xUnit, FluentAssertions
- **Configuration**: Azure App Configuration, Key Vault; local API uses .NET user secrets
- **Build Tools**: dotnet CLI
- **Migrations**: FluentMigrator (required for all schema changes)

## Frontend (Admin Dashboard)

- **Framework**: Next.js 15, React 19
- **Styling**: Tailwind CSS
- **Charts**: Recharts
- **Tables**: TanStack Table
- **State**: TanStack Query
- **Validation**: Zod
- **UI**: Headless UI, Heroicons, Radix
- **Auth**: MSAL
- **Realtime**: SignalR client
- **Tests**: Playwright

## Infrastructure

- **Cloud**: Azure (Container Apps, SQL Server, App Configuration, Key Vault)
- **Containerization**: Docker Compose under `7-Deployment/`
- **CI/CD**: GitHub Actions
- **Deployment**: Terraform (`7-Deployment/Azure-deploy/`)

## Development Tools

- **Version Control**: Git
- **Code Quality**: StyleCop, EditorConfig
- **Package Management**: NuGet (backend), npm (dashboard)

## Key constraints

- Native ADO.NET for all data access
- No Entity Framework
- No Pulumi, Bicep, or Ansible
- No driver mobile app in this tree

## Current Status

- Active development
- Cataloged runnables are documentation status `current`
- ADO.NET migration from Entity Framework is complete
