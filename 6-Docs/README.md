---
id: documentation-index
title: Documentation index
doc-type: index
status: current
owner: unassigned
last-reviewed: 2026-08-14
---

# Documentation index

Canonical documentation for Hotshot Logistics. The [documentation standard](documentation-standard.md) is the contract. The [catalog](catalog.md) is the runnable inventory.

## Start here

- [Documentation standard](documentation-standard.md)
- [Documentation ontology](documentation-ontology.md)
- [Component catalog](catalog.md)
- [Project overview](project-overview.md)
- [Clean Architecture layout](architecture.md)
- [Technology stack](technology-stack.md)
- [Data access](data-access.md)
- [Security guidelines](security.md)

## Cataloged runnables

| Component | Onboarding | Architecture |
| --- | --- | --- |
| Api | [onboarding](api/onboarding.md) | [architecture](api/architecture.md) |
| admin-dashboard | [onboarding](admin-dashboard/onboarding.md) | [architecture](admin-dashboard/architecture.md) |
| DbSetup | [onboarding](dbsetup/onboarding.md) | [architecture](dbsetup/architecture.md) |

## Rules

Engineering rules live under [rules/](rules/file-organization.md). Agents must follow them; see the root `AGENTS.md`.

## Deployment

Terraform and compose files: [7-Deployment README](../7-Deployment/README.md).
