---
id: catalog
title: Component and owner catalog
doc-type: reference
status: draft
owner: 'unassigned'
last-reviewed: 2026-08-17
---

# Component and owner catalog

This table is the **authoritative vocabulary** for the `component` and `owner`
frontmatter keys. A document naming a component with no row here fails
`KW-DOC-SPEC-004`. The check exists so that components cannot be invented one
document at a time until nobody can say how many there are.

Replace the example row below with the real units of your system. One row per
component that genuinely exists — a catalog with forty rows is a list of files.

| Component | Type | Source root | Overview | Detailed documentation | Owner | Last reviewed | Status |
|---|---|---|---|---|---|---|---|
| admin-dashboard | Service | `1-Presentation/admin-dashboard` | Next.js admin UI for jobs, drivers, tracking, and billing. | [architecture](admin-dashboard/architecture.md), [onboarding](admin-dashboard/onboarding.md) | unassigned | 2026-08-17 | current |
| Api | Service | `1-Presentation/HotshotLogistics.Api` | ASP.NET Core HTTP and SignalR edge for the platform. | [architecture](api/architecture.md), [onboarding](api/onboarding.md) | unassigned | 2026-08-17 | current |
| DbSetup | Tool | `7-Deployment/DbSetup` | Console tool that provisions SQL Server and applies migrations. | [architecture](dbsetup/architecture.md), [onboarding](dbsetup/onboarding.md) | unassigned | 2026-08-17 | current |

## How the columns are read

Only **Component** (index 1) and **Owner** (index 6) are parsed, counting the empty
cell produced by the leading pipe. The other columns are for human readers and may
be reworded freely. Moving either parsed column requires a matching
`ontology.catalog` override in `.kyber-weave/kyber-weave.yml`.
