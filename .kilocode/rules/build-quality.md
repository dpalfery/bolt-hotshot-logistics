# Build Quality Rule

Enforces zero-tolerance for build errors/warnings and StyleCop adherence for consistent, high-quality code in Hotshot Logistics.

## When to Apply
Apply to all builds: development, CI/CD pipelines, and releases.

## Zero Tolerance Policy
- **Fix all build errors immediately** - no exceptions.
- **Resolve all warnings** before merging or deploying.
- Treat warnings as errors in CI/CD to prevent technical debt.

### Configuration & Monitoring
- Set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<WarningsAsErrors />` in project files.
- Suppress specific warnings with `<WarningsNotAsErrors>CS1234</WarningsNotAsErrors>` only when documented.
- Implement CI/CD quality gates that fail on errors/warnings.
- Generate build reports with warning trends and alerts for degradation.

## StyleCop Rules

### Configuration
- Enable analyzers: `<PackageReference Include="StyleCop.Analyzers" Version="1.1.118" />`
- Use consistent `stylecop.json` and configure in `.editorconfig` or `Directory.Build.props`.

### Mandatory Categories
- **SA1000-SA1999**: Spacing rules
- **SA2000-SA2999**: Readability rules
- **SA3000-SA3999**: Ordering rules
- **SA4000-SA4999**: Maintainability rules
- **SA5000-SA5999**: Layout rules
- **SA6000-SA6999**: Documentation rules

### Exception Handling
- Suppress rules only with `#pragma warning disable SA1234` and justification comments.
- Prefer code fixes over suppression.
- Review suppressions in code reviews.

### Development Workflow
- Configure IDEs to show violations as warnings/errors.
- Enable auto-format on save and use `dotnet format` in pre-commit hooks.

## Enforcement

### Local Development
```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <WarningsAsErrors />
</PropertyGroup>
```

### CI/CD Requirements
- Fail builds on errors, warnings, or violations.
- Generate and archive quality reports.
- Block PRs with new warnings.

### Code Reviews
- Verify zero warnings before approval.
- Address all violations in PRs.
- Document suppression justifications.

## Efficiency Guidelines

### Early False Positive Detection
- Validate analyzer false positives before complex fixes.
- Document patterns to avoid repeated analysis.
- **Impact**: 40% reduction in refactoring attempts.

### Enhanced Batching
- Batch similar fixes using multiple SEARCH/REPLACE blocks.
- Group related violations for efficient resolution.
- **Impact**: 20-30% reduction in tool calls.

### Smarter Builds
- Use `dotnet build --no-restore` for subsequent builds.
- Target specific projects for focused verification.
- **Impact**: 50% faster cycles.

### Error Recovery
- Use "revert to baseline" for complex changes.
- Implement backup/rollback patterns.
- **Impact**: 60% reduction in recovery costs.

## Tools & References
- **EditorConfig**: Consistent coding styles
- **dotnet format**: Auto-formatting
- **SonarQube/SonarCloud**: Quality monitoring
- **Husky.NET**: Pre-commit validation

[StyleCop Docs](https://github.com/DotNetAnalyzers/StyleCopAnalyzers) | [MS Code Analysis](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview)