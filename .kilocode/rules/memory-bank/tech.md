## Technologies and Dependencies

### Backend (.NET)
- **Framework**: .NET 8, Azure Functions v4
- **Database**: SQL Server with native ADO.NET (Microsoft.Data.SqlClient) and FluentMigrator for schema management
- **Testing**: xUnit, FluentAssertions
- **Configuration**: Azure App Configuration, Key Vault
- **Build Tools**: dotnet CLI

### Frontend (Admin Dashboard)
- **Framework**: Next.js (React 18)
- **Styling**: TailwindCSS
- **Charts**: Recharts
- **Tables**: React Table
- **State Management**: React Query
- **Validation**: Zod
- **UI Components**: Headless UI, Heroicons
- **Development**: ESLint, Prettier, TypeScript

### Mobile (Driver App)
- **Framework**: Expo React Native (0.79+)
- **Icons**: Lucide React Native
- **Navigation**: Expo Router
- **Fonts**: Google Fonts
- **Maps**: React Native Maps

### Infrastructure
- **Cloud**: Azure (Functions, SQL Server, App Configuration, Key Vault)
- **Containerization**: Docker, Docker Compose
- **CI/CD**: GitHub Actions
- **Deployment**: Terraform, Bicep, Ansible

### Development Tools
- **Version Control**: Git
- **Code Quality**: StyleCop, EditorConfig
- **Package Management**: NuGet (backend), npm (frontend/mobile)

### Key Dependencies
- native ADO.NET (Microsoft.Data.SqlClient) and FluentMigrator for schema management for ORM
- Azure Functions for serverless API
- SQL Server for relational data
- React ecosystem for frontend/mobile
- Azure services for cloud integration

Current development status: Active development with .NET 8 upgrade completed, testing infrastructure in place, and cloud deployment configured.