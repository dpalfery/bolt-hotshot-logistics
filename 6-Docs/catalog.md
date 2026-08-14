---
id: catalog
title: Component and owner catalog
doc-type: reference
status: draft
owner: unassigned
last-reviewed: 2026-08-14
---

# Component and owner catalog

This table is the **authoritative vocabulary** for the `component` and `owner`
frontmatter keys. A document naming a component with no row here fails
`KW-DOC-SPEC-004`. The check exists so that components cannot be invented one
document at a time until nobody can say how many there are.

Catalog **runnable products only**. Shared libraries and test projects are not
rows. Do not add a product that is not in the tree.

| Component | Type | Source root | Overview | Detailed documentation | Owner | Last reviewed | Status |
|---|---|---|---|---|---|---|---|
| Api | Service | `1-Presentation/HotshotLogistics.Api` | ASP.NET Core Web API | `6-Docs/api/` | unassigned | 2026-08-14 | draft |
| admin-dashboard | Application | `1-Presentation/admin-dashboard` | Next.js admin dashboard | `6-Docs/admin-dashboard/` | unassigned | 2026-08-14 | draft |
| DbSetup | Tool | `7-Deployment/DbSetup` | Database setup CLI | `6-Docs/dbsetup/` | unassigned | 2026-08-14 | draft |

## How the columns are read

Only **Component** (index 1) and **Owner** (index 6) are parsed, counting the empty
cell produced by the leading pipe. The other columns are for human readers and may
be reworded freely. Moving either parsed column requires a matching
`ontology.catalog` override in `.kyber-weave/kyber-weave.yml`.
