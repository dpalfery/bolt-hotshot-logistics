# Rules Categorization Report

## Overview
This report categorizes all project rules into four distinct categories to optimize token usage and improve developer experience. The categorization helps developers focus on relevant rules based on their current work context.

## Categories

### 1. General Rules (Apply to All Development)
These rules provide foundational standards that apply across all development activities, regardless of technology stack or layer.

| Rule File | Description | Key Topics |
|-----------|-------------|------------|
| `README.md` | Overview of all rules and enforcement mechanisms | Rule structure, core rules, memory bank, enforcement |
| `architecture.md` | Clean Architecture principles and layered structure | Layer separation, dependency direction, file placement, ASP.NET Core practices |
| `code-quality.md` | Build quality, validation, and observability standards | Zero warnings policy, StyleCop, input validation, structured logging, testing coverage |
| `security.md` | Security practices and implementation guidelines | Secrets management, authentication, authorization, threat modeling, incident response |
| `process.md` | Development workflows and procedures | Task tracking, environment setup, database migrations, deployment procedures |
| `testing.md` | Testing standards and execution patterns | Unit testing, integration testing, performance testing, coverage requirements |

### 2. C# / Backend Rules (Specific to .NET Development)
These rules focus on .NET-specific patterns, data access, and backend development practices.

| Rule File | Description | Key Topics |
|-----------|-------------|------------|
| `ado-net.md` | Native ADO.NET data access implementation standards | Entity Framework prohibition, repository pattern, SQL best practices, performance optimization |
| `deployment.md` | Azure deployment standards and procedures | Infrastructure as code, CI/CD pipelines, environment management, rollback procedures |
| `architecture.md` (Backend sections) | .NET-specific architectural patterns | ASP.NET Core practices, middleware configuration, API documentation, health checks |
| `code-quality.md` (Backend sections) | .NET-specific code quality standards | Build quality, StyleCop rules, async/await patterns, error handling |
| `security.md` (Backend sections) | ASP.NET Core security implementation | Azure AD integration, JWT management, policy-based authorization, RBAC setup |
| `testing.md` (Backend sections) | .NET-specific testing patterns | xUnit testing, Moq mocking, database testing, performance benchmarking |

### 3. Frontend Developer Rules (Specific to Next.js/React Native)
These rules govern frontend development practices for the admin dashboard and mobile applications.

| Rule File | Description | Key Topics |
|-----------|-------------|------------|
| `frontend.md` | Frontend development standards for Next.js and React Native | Component architecture, state management, API integration, performance optimization |
| `architecture.md` (Frontend sections) | Frontend architectural patterns | Next.js admin dashboard, React Native mobile app, cross-platform development |
| `testing.md` (Frontend sections) | Frontend-specific testing approaches | React Testing Library, component testing, E2E testing, accessibility testing |

### 4. CI/CD Rules (DevOps and Deployment Pipeline)
These rules focus on continuous integration, deployment, and operational practices.

| Rule File | Description | Key Topics |
|-----------|-------------|------------|
| `deployment.md` | Azure deployment standards and infrastructure as code | Infrastructure as code, CI/CD pipelines, environment management, rollback procedures |
| `process.md` (CI/CD sections) | Development workflow automation and testing | Testing execution patterns, deployment procedures, CI/CD pipeline setup |
| `code-quality.md` (CI/CD sections) | Build pipeline quality gates and automation | Build quality enforcement, automated checks, code review quality gates |
| `security.md` (CI/CD sections) | Security scanning and validation in pipelines | SAST/DAST integration, dependency vulnerability scanning, security testing procedures |
| `testing.md` (CI/CD sections) | Automated testing in deployment pipelines | CI/CD integration, test execution automation, quality gates, performance benchmarking |

### 5. Memory Bank Rules
These rules manage project knowledge and documentation practices.

| Rule File | Description | Key Topics |
|-----------|-------------|------------|
| `memory-bank-instructions.md` | Memory bank usage guidelines and procedures | Memory bank structure, initialization, updates, task documentation |
| `memory-bank/brief.md` | Project foundation and core requirements | Problem statement, solution overview, business objectives, success metrics |
| `memory-bank/product.md` | Product definition and user experience goals | Core problems solved, end-to-end workflow, user experience goals, competitive advantages |
| `memory-bank/context.md` | Current development status and progress tracking | Work focus, completed components, in-progress items, next priorities |
| `memory-bank/architecture.md` | System architecture and technical decisions | Architecture principles, component architecture, data architecture, security architecture |
| `memory-bank/tech.md` | Technology stack and development environment | Backend/frontend/mobile technologies, development setup, build processes, performance targets |

## Usage Guidelines

### For Backend Developers
- **Primary Focus**: C# / Backend Rules + General Rules
- **Secondary Reference**: Memory Bank Rules for project context
- **Skip**: Frontend Developer Rules (unless working on integration points)

### For Frontend Developers
- **Primary Focus**: Frontend Developer Rules + General Rules
- **Secondary Reference**: Memory Bank Rules for project context
- **Skip**: C# / Backend Rules (unless working on API integration)

### For Full-Stack Developers
- **Context-Aware Usage**: Switch between rule categories based on current task
- **Integration Points**: Reference both Frontend and Backend rules when working on API integration
- **Architecture**: Use General Rules and Memory Bank for architectural decisions

### For DevOps/Infrastructure
- **Primary Focus**: CI/CD Rules + Deployment Rules + Security Rules + General Rules
- **Secondary Reference**: Memory Bank Architecture and Tech files
- **Pipeline Development**: Process Rules for workflow implementation

## Token Optimization Benefits

1. **Reduced Context Window Usage**: Developers can focus on ~20% of rules relevant to their work (vs ~25% with 4 categories)
2. **Faster Rule Lookup**: Clear categorization reduces search time across 5 focused categories
3. **Context-Aware Development**: Rules are presented based on current development context with CI/CD separation
4. **Reduced Cognitive Load**: Fewer rules to remember and reference within each category
5. **Mode-Specific Loading**: Custom modes can load only 1-2 category files instead of entire rule sets
6. **Improved Maintenance**: Category-specific files are easier to update and version

## Maintenance

This categorization should be reviewed quarterly or when:
- New rule files are added
- Existing rules are significantly modified
- Development team structure changes
- New technology stacks are introduced

## Recommendations for Splitting Files into Category-Specific Files

To prepare for custom mode implementation, I recommend restructuring the monolithic rule files into category-specific files. This will enable more granular token optimization and better mode-specific rule loading.

### Proposed File Structure

```
.kilocode/rules/
├── general/
│   ├── architecture-general.md
│   ├── code-quality-general.md
│   ├── security-general.md
│   ├── process-general.md
│   └── testing-general.md
├── backend/
│   ├── architecture-backend.md
│   ├── ado-net.md
│   ├── code-quality-backend.md
│   ├── security-backend.md
│   ├── testing-backend.md
│   └── deployment-backend.md
├── frontend/
│   ├── architecture-frontend.md
│   ├── frontend.md
│   ├── code-quality-frontend.md
│   └── testing-frontend.md
├── cicd/
│   ├── deployment-cicd.md
│   ├── process-cicd.md
│   ├── code-quality-cicd.md
│   ├── security-cicd.md
│   └── testing-cicd.md
├── memory-bank/
│   ├── instructions.md
│   ├── brief.md
│   ├── product.md
│   ├── context.md
│   ├── architecture.md
│   └── tech.md
└── README.md (updated to reference new structure)
```

### Splitting Strategy

#### 1. **General Rules** (Cross-cutting concerns)
- Extract sections that apply to all development from existing files
- Focus on universal standards, principles, and practices
- Include fundamental architecture concepts and quality gates

#### 2. **Backend Rules** (.NET/C# specific)
- Combine ADO.NET rules with .NET-specific sections from other files
- Include ASP.NET Core patterns, database optimization, and backend testing
- Focus on server-side development practices

#### 3. **Frontend Rules** (Next.js/React Native specific)
- Consolidate all frontend development standards
- Include React/Next.js patterns, mobile development, and UI/UX guidelines
- Focus on client-side development practices

#### 4. **CI/CD Rules** (DevOps and automation)
- Extract deployment and pipeline content from existing files
- Include security scanning, testing automation, and release management
- Focus on operational and infrastructure concerns

#### 5. **Memory Bank** (Keep as separate category)
- Maintain current structure as it's already well-organized
- These files serve a different purpose (knowledge management vs. standards)

### Implementation Benefits

1. **Token Efficiency**: Load only ~20-25% of rules relevant to current context
2. **Mode-Specific Loading**: Custom modes can load only their category files
3. **Maintenance**: Easier to update category-specific content
4. **Discovery**: Developers can quickly find relevant rules
5. **Modularity**: Categories can evolve independently

### Migration Approach

1. **Phase 1**: Create new category directories and extract content
2. **Phase 2**: Update existing files to reference new structure
3. **Phase 3**: Implement mode-specific rule loading
4. **Phase 4**: Archive old monolithic files
5. **Phase 5**: Update documentation and team processes

### Content Extraction Guidelines

- **General**: Universal standards, fundamental principles, cross-cutting concerns
- **Backend**: .NET-specific patterns, database operations, server-side logic
- **Frontend**: UI/UX patterns, component architecture, client-side development
- **CI/CD**: Automation, deployment, pipeline, operational concerns
- **Memory Bank**: Keep separate as knowledge/documentation category

## File Locations

- **General Rules**: `.kilocode/rules/general/`
- **C# / Backend Rules**: `.kilocode/rules/backend/`
- **Frontend Developer Rules**: `.kilocode/rules/frontend/`
- **CI/CD Rules**: `.kilocode/rules/cicd/`
- **Memory Bank Rules**: `.kilocode/rules/memory-bank/`
- **This Report**: `6-Docs/rules-categorization-report.md`