---
id: documentation-standard
title: Hotshot Logistics Documentation Standard
doc-type: governance
status: draft
owner: unassigned
last-reviewed: 2026-08-14
---

# Hotshot Logistics Documentation Standard

## Purpose and scope

This is the authoritative standard for human- and agent-authored documentation in Hotshot Logistics. Agents retrieve it first; humans must still be able to read it.

It applies to maintained runnable products, platform documentation under `6-Docs/`, operational procedures, and public repository surfaces that this project actually uses.

The repository uses Markdown as documentation-as-code. [catalog.md](catalog.md) is the complete inventory of cataloged components. Source lives in numbered Clean Architecture layers (`0-Base` through `7-Deployment`). Documentation lives in `6-Docs/`.

Documentation MUST match the tree. Do not describe a product, host, or runtime as current if the code is not in the repository.

## Locations and canonical sources

This repository uses a **hybrid** layout. Purpose folders hold platform documentation. Dedicated folders exist only for cataloged runnable products.

### Purpose folders (kebab-case)

Directory placement expresses the document's *purpose*, not its subject. The component a document describes is carried by the `component` frontmatter key defined in the [documentation ontology](documentation-ontology.md). Do not infer ownership from a purpose-folder name.

| Folder | Purpose |
| --- | --- |
| `6-Docs/system/` | System-wide onboarding, architecture, and operating context |
| `6-Docs/reference/` | Reusable configuration and technical reference |
| `6-Docs/rules/` | Agent and engineering rules (`doc-type: rule`) |
| `6-Docs/devops/` | Deployment and build procedures |
| `6-Docs/operations/` | Operating a deployed system |
| `6-Docs/plans/` | Working feature plans |
| `6-Docs/specs/` | Feature specifications (requirements, design, tasks) |
| `6-Docs/adr/` | Architecture decision records |
| `6-Docs/archive/` | Historical material; never current guidance |
| `6-Docs/demo/` | Demo prompts and walkthroughs |

A component-specific runbook lives in `6-Docs/operations/` and names its component in frontmatter.

### Runnable-product folders

Cataloged runnables are the only components that receive a dedicated documentation folder:

| Component | Source root | Detailed docs |
| --- | --- | --- |
| Api | `1-Presentation/HotshotLogistics.Api` | `6-Docs/api/` |
| admin-dashboard | `1-Presentation/admin-dashboard` | `6-Docs/admin-dashboard/` |
| DbSetup | `7-Deployment/DbSetup` | `6-Docs/dbsetup/` |

Each of those folders SHALL contain `onboarding.md` and `architecture.md`. Feature requirements do **not** live here; they belong under `6-Docs/specs/` when that flow is used.

Do not add a `6-Docs/<component>/` folder for shared libraries, test projects, or products that are not in the tree.

### Canonical-source rules

- A component README at its source root is a concise overview: purpose, boundaries, primary entry points, and links to its detailed documentation.
- `6-Docs/` contains canonical documentation only. Scratch notes, vendored packages, and git-ignored working files SHALL NOT live anywhere beneath it. Agent scratch output belongs in the path declared as **<agent-scratchpad>** in the root `AGENTS.md` Config Registry.
- Plans are working documents in `6-Docs/plans/`. The [plan index](plans/README.md) is the inventory **once that index exists**. Empty purpose folders are not authority; agents SHALL NOT treat a missing index as an entry point.
- Superseded or historical material belongs in `6-Docs/archive/` (ADRs: `6-Docs/archive/adrs/`) and must be visibly non-authoritative.
- GitHub community files that this repository uses remain at the repository root or under `.github/`. [SECURITY.md](../SECURITY.md) is the security policy. Do not add `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SUPPORT.md`, `CODEOWNERS`, or `REVIEW.md` unless the project adopts them.

Do not create a second canonical document for a topic. Link to the established source instead.

## Required content

### Root README

The root [README](../README.md) SHALL state the project purpose, supported status, concise onboarding path, maintained runnable map, documentation entry point, and community links. It SHALL link each cataloged component README, but SHALL not become a recursive file listing.

### Component README

A cataloged runnable README SHALL state its purpose, boundary, primary technology/entry point, and links to detailed documentation. It SHALL not duplicate setup or architecture maintained in `6-Docs`.

Shared libraries and test projects MAY have a source-root README. They are not catalog components. Document public contracts and non-obvious decisions there, and link to system or reference docs instead of creating a dedicated `6-Docs/<library>/` folder.

### Detailed runnable documentation

Each cataloged runnable documentation folder SHALL contain:

1. `onboarding.md` — dependencies, setup, debug path, and non-standard operating procedures.
2. `architecture.md` — overview, architecture, components and interfaces, data models, error handling, and testing strategy. Use Mermaid only where it clarifies a relationship.

Do not add `requirements.md` beside those files. Numbered feature requirements belong in `6-Docs/specs/{feature-name}/` when a specification is opened.

### Frontmatter

Every document in scope of the [documentation ontology](documentation-ontology.md) SHALL begin with a YAML frontmatter block conforming to that schema. The ontology is authoritative for the key set, the closed vocabularies, and the required-key matrix; this standard does not restate them.

Three rules are load-bearing:

1. **`id` is permanent.** It is assigned once and never changed, so graph edges survive file moves and renames.
2. **`code-refs` and `api-endpoints` values are taken from the CodeGraph index, never hand-written.** A value that does not resolve is entity drift.
3. **A fact carried in frontmatter is not repeated in the body.** Remove the legacy `**Component:**` / `**Status:**` / `**Date:**` bold key-value lines when adding frontmatter that supersedes them.

Frontmatter conformance and code-entity resolution are checked with Kyber-Weave; see [Validation](#validation).

### Coverage

Every cataloged runnable SHALL have a catalog entry, a source-root README, and a detailed documentation path (`onboarding.md` and `architecture.md`). Shared libraries and test projects are not catalog rows. Document public contracts and non-obvious decisions; do not document every private class.

Do not catalog a product that is not in the tree (for example a driver mobile app). Add it when the source exists.

## Ownership, lifecycle, and review

- Each catalog entry has an owner, documentation status, and last-reviewed date.
- The catalog is the authoritative ownership and review-date record for cataloged runnables; update it when material changes occur.
- Feature plans SHALL use the lifecycle in [the plan index](plans/README.md) **after that index exists**. A completed plan is reviewed against the implemented behavior and its canonical documentation before it is archived. A plan that has not received that review is never implicitly considered complete.
- Archived documentation must never be followed as current guidance.
- Documentation changes are required when a runnable's public interface, configuration, architecture, supported runtime, operations, or user workflow changes.

## Writing, safety, and links

- Write for a defined reader and task. Prefer short headings, prerequisite-first instructions, stable relative links, and commands that are safe to copy.
- Never include credentials, tokens, passwords, private endpoints, customer data, or unsafe direct deployment commands. Use placeholders and link to the approved workflow. Secrets MUST NOT exist anywhere in the repository working tree; see the absolute secrets ban in the root `AGENTS.md`.
- State facts verified from source. Label proposals, historical content, and environment-specific examples clearly.
- Check internal links whenever files move or a README changes. Use descriptive link text rather than raw URLs where practical.

## Plan lifecycle and agent workflow

The plan index is the only entry point for agent work on plans, **and only once that index file exists**. Empty `6-Docs/plans/` is not an inventory. Agents SHALL read the index before opening a plan and SHALL open only the plan selected by the task and listed there as `Draft`, `Ready`, `In progress`, or `Blocked`. `Draft` supports planning work only; implementation requires a `Ready`, `In progress`, or `Blocked` plan. `Review required`, `Completed`, `Superseded`, and `Archived` plans are historical records and are not implementation authority.

New plans SHALL contain `Status`, `Date`, and `Goal` fields directly below the title, and their status SHALL be kept in sync with the index. Use only these statuses:

- `Draft` — being prepared; not approved for implementation.
- `Ready` — approved and ready for implementation.
- `In progress` — implementation is underway.
- `Blocked` — work cannot proceed; the blocker must be stated in the plan.
- `Review required` — temporary migration state; implementation completion has not been verified. Agents must not act on it.
- `Completed` — implementation and documentation are verified; archive it promptly.
- `Superseded` — replaced by a named plan or canonical document; archive it promptly.
- `Archived` — historical only; this status is used in `6-Docs/archive/`.

When implementation completes, the owner SHALL: verify the plan's acceptance criteria, update the affected canonical documentation, add the implementation reference and archive date to the plan index, move the plan to `6-Docs/archive/plans/`, and change its status to `Archived`. Do not archive a plan merely because its Markdown was finalized.

## Specification lifecycle

A specification is a three-document set — `requirements.md`, `design.md`, `tasks.md` — under `6-Docs/specs/{feature-name}/`, produced before implementation begins. This is where numbered feature requirements live. Per-runnable folders do not carry `requirements.md`.

**A specification has the same shelf life as a plan, and is governed the same way.** It records what was intended at a point in time and goes stale the moment implementation diverges from it. It is never canonical guidance: an agent that answers from a specification is quoting a proposal, not the system. The [specification index](specs/README.md) is the only entry point **once that index exists**. It carries the same status vocabulary as plans, and agents SHALL open only a specification listed there as `Draft`, `Ready`, `In progress`, or `Blocked`.

The three documents share one status, tracked in the index and stated in each document's `Status` field. They move through it together; a specification whose design is approved but whose tasks are unwritten is still `Draft`.

Specification closeout mirrors plan closeout. When the tasks are delivered and their tests pass, the agent SHALL verify the specification's requirements against the implementation evidence, update the affected canonical documentation so the durable content survives the archive, and update the specification index. Only then may it move the whole `{feature-name}/` directory to `6-Docs/archive/specs/` and set the status to `Archived`; otherwise the specification remains `Review required` or returns to an active status.

Archiving a specification without first migrating its durable content is the failure this lifecycle exists to prevent. The specification is the scaffolding; the canonical documentation is the building.

## Architecture decision records

An ADR records a decision that constrains future work. It lives in `6-Docs/adr/` as `ADR-{date}-{slug}.md` and carries `doc-type: adr`.

Unlike plans and specifications, an ADR does **not** go stale on delivery — it is a durable record of why the system is shaped the way it is, and it stays current until superseded. Superseding an ADR means writing a new one that names the old one in its `supersedes` frontmatter; the old one is then archived to `6-Docs/archive/adrs/`. Do not edit a decision out of an accepted ADR — the record of a decision that was later reversed is exactly what makes the reversal legible.

Write an ADR when a decision meets all three tests:

1. It **constrains future work** — later changes must live with it.
2. It had **viable alternatives that were rejected**, and the rejection is not self-evident.
3. It would be **expensive to revisit** — reversing it means reworking code, data, or infrastructure.

A choice that fails any of these is a normal implementation decision and belongs in the code and its documentation, not in an ADR. Recording every discussed trade-off devalues the records that matter.

Documents affected by a decision link to it through the `decided-by` frontmatter key, which is how the documentation graph carries the decision to the components it governs.

## Agent workflow

Before changing code or documentation, an agent SHALL read this standard and the catalog, identify the affected runnables, and inspect their existing README and detailed documentation. If a task refers to a plan or specification, the agent SHALL also read the corresponding index **if it exists** and follow the lifecycle above. After the change, the agent SHALL update the owning runnable docs and catalog entry, or explain why no documentation impact exists.

## Validation

Kyber-Weave is the intended documentation gate. Markdown under `6-Docs/` is in scope, including rules after conversion from `.mdc`. GitHub Actions in this repository do not yet run these commands; do not treat a green application-build workflow as documentation validation.

Frontmatter validation runs in two tiers:

| Tier | Command | Rule codes | Runs when |
| --- | --- | --- | --- |
| Schema | `kyber-weave docs validate` | `KW-DOC-SPEC-001`–`006` | documentation changes |
| Drift | `kyber-weave docs drift` | `KW-DOC-DRIFT-001`–`003` | code or documentation changes |

The drift tier resolves every `code-refs` and `api-endpoints` value against `.codegraph/codegraph.db`. A renamed or deleted symbol that leaves a dangling documentation reference is a failure.

When a root README and `6-Docs/README.md` exist, reviewers SHALL verify they retain a navigable path to the affected content.

Legacy documents without frontmatter are brought into full schema scope when they are materially revised. Until then, link and secret checks still apply.
