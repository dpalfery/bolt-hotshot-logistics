---
name: resharper-clt
description: Use when running static analysis, code quality inspections, or automated code formatting on .NET C# solutions using JetBrains ReSharper Command Line Tools (InspectCode and CleanupCode). Mandatory for C# development verification and code review gates.
license: MIT
metadata:
  author: David R Palfery
  version: 1.0.0
---

# ReSharper Command Line Tools (CLT) Skill

This skill guides the execution, analysis, and remediation of JetBrains ReSharper Command Line Tools (`InspectCode` and `CleanupCode`) across .NET solutions and C# source files.

## Tool Overview & Setup

The repository tracks ReSharper CLT via the local .NET tool manifest ([.config/dotnet-tools.json](file:///.config/dotnet-tools.json)).

### Restore Tools
Before invoking ReSharper commands, ensure the tools are restored:
```bash
dotnet tool restore
```

### Core CLI Commands

1. **Full Solution Static Analysis (`inspectcode`):**
   ```bash
   dotnet jb inspectcode HotshotLogistics.sln --output=.agents-scratchpad/inspectcode-results.xml --format=Xml
   ```

2. **Project-Scoped Static Analysis:**
   ```bash
   dotnet jb inspectcode HotshotLogistics.sln --project="HotshotLogistics.Api" --output=.agents-scratchpad/api-inspect.xml --format=Xml
   ```

3. **SARIF Output (for CI / Automated Scanning):**
   ```bash
   dotnet jb inspectcode HotshotLogistics.sln --output=.agents-scratchpad/results.sarif --format=Sarif --severity=WARNING
   ```

4. **Automated Code Formatting & Cleanup (`cleanupcode`):**
   ```bash
   dotnet jb cleanupcode HotshotLogistics.sln
   ```

---

## Workflow for C# Developer (`csharp-dev` / `dotnet-dev`)

When implementing, modifying, or refactoring C# code:

1. **Pre-Completion Inspection:**
   Run `inspectcode` on the solution or affected projects:
   ```bash
   dotnet jb inspectcode HotshotLogistics.sln --output=.agents-scratchpad/dev-inspect.xml --format=Xml
   ```

2. **Remediate Findings:**
   Address all ERRORS and WARNINGS introduced by the changes:
   - **`PossibleMultipleEnumeration`**: Materialize `IEnumerable` with `.ToList()` or `.ToArray()` before iterating multiple times.
   - **`ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract`**: Fix contradictory null checks or annotate nullable references accurately on DTOs and API models.
   - **`InheritdocInvalidUsage`**: Replace invalid `<inheritdoc />` tags on classes/records with proper XML `<summary>` doc comments.
   - **`UseCollectionExpression`**: Modernize array/list initializations with C# collection expressions `[...]`.
   - **`CheckNamespace`**: Align file namespaces to the directory hierarchy.
   - **`UnusedParameter.Local` / `PrivateFieldCanBeConvertedToLocalVariable`**: Remove dead parameters or convert single-assignment fields to local variables.
   - **`RedundantSuppressNullableWarningExpression`**: Remove unnecessary `!` operators.
   - **Non-Breaking Public API Contracts**: Do not delete public API action methods, interface members, or DTO serialization properties.

3. **Build & Quality Gate:**
   Ensure both `dotnet build` and `dotnet jb inspectcode` complete with **0 errors and 0 code/logic warnings** before declaring the task complete.

---

## Workflow for Code Reviewer (`code-reviewer`)

When reviewing pull requests or completed implementation tasks touching C# files:

1. **Execute Inspection Gate:**
   Run `inspectcode` to evaluate static quality and rule compliance:
   ```bash
   dotnet jb inspectcode HotshotLogistics.sln --output=.agents-scratchpad/review-inspect.xml --format=Xml
   ```

2. **Evaluate Inspection Report:**
   - Verify that no new `WARNING` or `ERROR` findings were introduced in modified or added files.
   - Check that style rules, nullability invariants, and coding standards are upheld.

3. **Disposition in Review Report:**
   - If findings exist: Issue **Changes Requested** detailing the exact file, line number, rule ID, and required fix.
   - If clean: Record **ReSharper CLT Gate: PASSED (0 warnings/errors)** in the review summary.
