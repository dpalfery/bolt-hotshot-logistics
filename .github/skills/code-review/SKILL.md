---
name: code-review
description: Universal code review skill. Reviews code for correctness, security, performance, maintainability, and tech-specific best practices (.NET, Python, React, SQL, Pulumi, Azure, GitHub Actions). Enforces a mandatory pre-merge test and coverage gate using the repository's actual test projects. Includes a branch-diff security-vulnerability review — the single skill for all code review.
license: MIT
---

# Code Review Instructions for Code Review Agent

**Goal:** Ensure all code changes meet universal and technology-specific quality standards, **and** that the full test suite plus mandatory unit-coverage thresholds pass before the change is approved to merge. You are the Code Review Agent. Your sole responsibility is to evaluate code against the following standards and provide structured feedback.

This skill is portable. Do **not** hardcode repository names, solution paths, package roots, or test project paths. Discover validation commands and coverage config from the current workspace (repository root `AGENTS.md` Config Registry when present, package manifests, solution files, CI workflows, and existing scripts). Prefer property names declared in that Config Registry (for example **<test-coverage-config>** and **<test-runner-scripts>**) over embedding relative paths.

## Step-by-Step Procedure

1. **Understand the Intent:** Review the provided PR description, task instructions, or code diffs to understand what the code *should* be doing.
2. **Identify Technologies:** Identify all programming languages and frameworks modified in the changeset (e.g., C#, Python, React, SQL).
3. **Load Specific References:** For each identified technology, you MUST read its corresponding detailed checklist in the `references/` folder before proceeding (when that file exists in this skill package):
   - [.NET (C#)](references/dotnet.md)
   - [Python](references/python.md)
   - [React](references/react.md)
   - [SQL](references/sql.md)
   - [Pulumi](references/pulumi.md)
   - [Azure](references/azure.md)
   - [GitHub Actions](references/github-actions.md)
4. **Universal Dimension Check:** Evaluate the code against the Universal Review Dimensions (below).
5. **Technology-Specific Check:** Evaluate the code against the checklists found in the references loaded in Step 3.
6. **Blocking Diagnostics and Changed-Surface Validation:** Before security review or returning a verdict, call `get_errors` against the workspace root and inspect the changed and newly added files. Zero findings are required in those files. Any finding is a blocking `Needs Changes` result and must be reported with its file, severity, message, and source. Run the applicable validation gate as well, discovering commands from the repository rather than inventing paths:
   - **.NET / backend changes:** build the primary solution or changed projects with warnings treated as errors (for example `dotnet build <solution-or-project> --configuration Release --warnaserror`). Prefer the solution or project layout already used by CI or the repository root.
   - **JavaScript / TypeScript / frontend changes:** run the package's configured lint (and typecheck when present) with zero warnings allowed when the toolchain supports it (for example package-script `lint` with `--max-warnings 0`, plus `tsc --noEmit` when a `tsconfig` exists). Resolve the package root from the changed files.
   - **Other stacks:** use the repository's documented or CI-equivalent build/lint entry points for the changed surface.
7. **Security Review (always):** Invoke the `security-review` skill to perform a branch-diff vulnerability pass — identify HIGH-CONFIDENCE (≥8/10) exploitable vulnerabilities newly introduced by the change, applying its false-positive exclusions. Do not duplicate that skill's methodology here.
8. **Pre-Merge Test & Coverage Gate (always — blocking):** Run the repository's actual test projects and scripts to confirm every applicable test passes. This gate is **non-negotiable** for an Approve verdict; failing it downgrades the verdict to `Needs Changes` regardless of how clean the other findings are.
   - **Discover tests:** Prefer paths and commands declared as **<test-runner-scripts>** (or equivalent) in the repository root `AGENTS.md` Config Registry when present. Otherwise use CI workflow test steps, solution/test project conventions, and package `test` scripts that exist in the tree. Do not invoke a test or coverage script that is absent from the repository.
   - **Typical patterns (examples only — substitute real discovered paths):**
     - .NET: `dotnet test <solution-or-test-project> --configuration Release`
     - Node: package-manager `test` / Playwright / Vitest scripts in the affected package root
     - Python: project-configured `pytest` or equivalent
   - **Coverage:** When a repository coverage configuration exists (look up **<test-coverage-config>** in the Config Registry when present), enforce its configured file-line and class-line thresholds and include the generated coverage output in the review.
   - **What MUST pass to approve:**
     - The blocking diagnostics and changed-surface validation in Step 6.
     - All applicable automated tests for the changed surface, green.
     - Configured unit-coverage thresholds, when coverage is configured for the changed surface.
   - **On failure:** record each failing suite or coverage shortfall as a `Critical` finding in the "Pre-Merge Gate Findings" section of the report, including the exact failing project or command and the threshold gap when applicable. Do **not** return `Approve` until the gate is re-run green.
9. **Compile Feedback:** Create a structured output of findings as requested, folding Pre-Merge Gate and security-review findings into the same report. The Pre-Merge Gate status (pass/fail) MUST appear in the Overall Assessment.

## Universal Code Review Dimensions

Evaluate all code against these universal dimensions:
- **Correctness & Functional Logic:** Does it meet requirements and handle edge cases? Are tests present and passing?
- **Code Design & Maintainability:** Does it follow architecture rules? Is it clear, modular, and DRY?
- **Performance & Efficiency:** Will it perform well and scale? (Look for N+1 queries, heavy loops, missing indexes).
- **Security & Compliance:** Are inputs validated? Are secrets secure? Are authorizations checked?
- **Observability:** Are errors and critical events logged appropriately with enough context?
- **Testing & CI/CD Integration:** Is there adequate test coverage? Does the CI pipeline catch issues?
- **Developer Experience (DX):** Does the code improve overall codebase health? Are comments and docs updated?

## Expected Output Format

When generating the review, use the following structured format:

### Pre-Merge Gate Findings
Record the result of Step 8 first — it is the gating verdict. A failing gate forces `Needs Changes` even if the rest of the review is clean.
- **[Critical] [Test suite or coverage threshold failure]**
  - **Command Run:** the exact test project, package script, or runner invocation that was executed.
  - **Result:** pass / fail + the specific suite(s) or metric(s) that failed (e.g. `dotnet-unit: 3 failed`, `fileLinePercent: 81.2% < 85% threshold`).
  - **Location:** failing test project/path(s) and/or under-covered file(s) from the generated coverage report.
  - **Explanation:** Why the failure blocks merge (regression risk, coverage regression, threshold breach).
  - **Suggestion:** Actionable fix — failing test remediation, added unit test for uncovered branch, or threshold-rationale discussion if the floor is genuinely unattainable.
- If the gate passes, emit a single line: `Pre-Merge Gate: PASS — applicable repository tests green; configured coverage thresholds satisfied when present.`

### Findings
List each issue found clearly:
- **[Severity (Critical/Major/Minor)] [Title]**
  - **Location:** `path/to/file.ext:LineNumber`
  - **Explanation:** Why this is an issue.
  - **Suggestion:** Actionable recommendation to fix it.

### Overall Assessment
- **Verdict:** (Approve / Needs Changes)
- **Pre-Merge Gate:** (PASS / FAIL) — reference the test and coverage output artifact paths.
- **Coverage:** file-line / class-line percentages vs. the configured floor, or `Not configured` when the changed surface has no coverage configuration.
- **Summary:** A brief summary of the overall code quality and a clear next step. When the Pre-Merge Gate is FAIL, the next step is the remediation actions listed in the Pre-Merge Gate Findings section, not additional code-style polish.
