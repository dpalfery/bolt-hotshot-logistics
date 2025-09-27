# Hotshot Logistics Platform - Architecture

## System Architecture Overview

The Hotshot Logistics project follows Clean Architecture principles with a numbered folder structure to enforce separation of concerns and dependency direction. The architecture is designed for scalability, maintainability, and real-time operations in a cloud-native environment.

### Core Architecture Principles

- **Clean Architecture**: Strict separation of concerns with defined layers (Domain, Application, Infrastructure, Presentation)
- **Domain-Driven Design**: Business logic encapsulated in application services with clear domain boundaries
- **Event-Driven Architecture**: Real-time updates through WebSocket connections and event sourcing
- **Microservices-Ready**: Modular design allowing future decomposition into microservices
- **API-First Development**: RESTful APIs with comprehensive OpenAPI documentation
- **Security by Design**: Defense-in-depth with authentication, authorization, and encryption at all levels

### Layered Architecture Structure

Following the numbered folder structure defined in the project:

```
0-Base/
  └── Core utilities, base classes, cross-cutting concerns

1-Presentation/
  └── API Controllers, SignalR Hubs, Request/Response DTOs

2-Application/
  └── Business Logic, Use Cases, Application Services, Validators

3-Domain/
  └── Domain Models, Contracts, DTOs, Repository Interfaces

4-Persistence/
  └── Native ADO.NET repositories and FluentMigrator for schema & migrations

5-Test/
  └── Unit Tests, Integration Tests, Test Utilities

6-Docs/
  └── Documentation, Specifications, API Documentation

7-Deployment/
  └── Infrastructure as Code, Docker, CI/CD Pipelines
```

### Component Architecture

#### Backend API (.NET 8 Azure Functions)
- **HTTP APIs**: RESTful endpoints for CRUD operations
- **Real-time Communication**: SignalR hubs for live updates
- **Background Processing**: Azure Functions for scheduled tasks
- **External Integrations**: Payment gateways, mapping services, notifications

#### Admin Dashboard (Next.js/React)
- **Component Architecture**: Atomic design with reusable UI components
- **State Management**: React Query for server state, Context API for local state
- **Real-time Updates**: WebSocket integration for live data
- **Authentication**: Azure AD integration with role-based access

#### Mobile App (React Native/Expo)
- **Cross-platform**: Single codebase for iOS and Android
- **Offline Support**: Local data storage and sync capabilities
- **Background Processing**: Location tracking and notification handling
- **Native Integration**: Camera, GPS, and device sensors

### Data Architecture

#### Database Design
- **Primary Database**: SQL Server with optimized schema for logistics operations
- **Caching Layer**: Redis for session management and real-time data
- **File Storage**: Azure Blob Storage for documents and images
- **Message Queue**: Azure Service Bus for decoupled processing

#### Key Domain Models

**Job Management**
- Jobs with pickup/delivery locations, cargo details, pricing
- Real-time tracking with location updates and ETAs
- Status workflow: Pending → Assigned → In Progress → Completed

**Driver Management**
- Driver profiles with vehicle info, certifications, performance metrics
- Availability tracking and automated scheduling
- Earnings calculation and payment processing

**Customer Management**
- Customer profiles with credit terms and billing preferences
- Contract management and pricing agreements
- Communication preferences and history

**Financial Management**
- Automated invoice generation from completed jobs
- Payment processing with multiple gateways
- Accounts receivable tracking and reporting

### Integration Architecture

#### External Service Integrations
- **Payment Processing**: Stripe, PayPal for customer payments
- **Mapping Services**: Google Maps, Azure Maps for routing and geocoding
- **Communication**: Twilio SMS, SendGrid email, Azure Notification Hubs
- **Authentication**: Azure Active Directory for user management

#### API Design
- **RESTful Endpoints**: Resource-based URLs with proper HTTP methods
- **Versioning Strategy**: URL-based versioning (/api/v1/)
- **Authentication**: JWT tokens with Azure AD integration
- **Documentation**: OpenAPI/Swagger with interactive testing

### Security Architecture

#### Authentication & Authorization
- **User Authentication**: Azure AD with multi-factor authentication
- **API Security**: JWT tokens with role-based access control
- **Mobile Security**: Secure token storage and biometric authentication
- **Session Management**: Redis-based session storage with automatic expiry

#### Data Protection
- **Encryption at Rest**: AES-256 encryption for sensitive data
- **Encryption in Transit**: TLS 1.3 for all communications
- **Key Management**: Azure Key Vault for secret storage
- **Audit Logging**: Comprehensive logging of all user actions

### Performance Architecture

#### Scalability Patterns
- **Horizontal Scaling**: Azure Functions auto-scaling based on load
- **Database Optimization**: Read replicas and query optimization
- **Caching Strategy**: Multi-level caching with Redis
- **CDN Integration**: Azure Front Door for global content delivery

#### Real-time Architecture
- **WebSocket Communication**: SignalR for bidirectional communication
- **Event Streaming**: Azure Service Bus for event distribution
- **Live Updates**: Real-time dashboard updates and mobile notifications
- **Location Tracking**: Efficient GPS data processing and storage

### Deployment Architecture

#### Infrastructure as Code
- **Azure Resources**: Functions, SQL Database, Storage, Redis Cache
- **Networking**: Virtual networks, security groups, private endpoints
- **Monitoring**: Application Insights, Azure Monitor, health checks
- **Security**: Key Vault, managed identities, RBAC

#### CI/CD Pipeline
- **Build Process**: Automated builds with dependency management
- **Testing**: Unit, integration, and performance tests
- **Deployment**: Blue-green deployments with zero downtime
- **Rollback**: Automated rollback capabilities

### Key Technical Decisions

1. **Native ADO.NET over ORM**: Direct SQL control for performance and flexibility
2. **Azure Functions**: Serverless architecture for cost-effective scaling
3. **SignalR for Real-time**: WebSocket-based communication for live updates
4. **FluentMigrator**: Database versioning without Entity Framework dependencies
5. **Clean Architecture**: Strict layering for maintainability and testability
6. **Repository Pattern**: Abstraction over data access for flexibility
7. **CQRS Pattern**: Separation of read and write operations
8. **Dependency Injection**: Throughout the application for testability
9. **Azure Cloud Native**: Leveraging Azure services for scalability and reliability
10. **Cross-platform Mobile**: React Native for iOS and Android coverage

### Architecture Benefits

- **Maintainability**: Clear separation of concerns and modular design
- **Scalability**: Cloud-native architecture with auto-scaling capabilities
- **Reliability**: Comprehensive error handling and monitoring
- **Security**: Defense-in-depth with multiple security layers
- **Performance**: Optimized data access and caching strategies
- **Testability**: Dependency injection and mocking support
- **Flexibility**: Modular design allowing for future enhancements