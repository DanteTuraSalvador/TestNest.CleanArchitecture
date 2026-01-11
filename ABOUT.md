# About TestNest.CleanArchitecture

## Project Overview

**TestNest.CleanArchitecture** is a comprehensive demonstration project showcasing modern .NET API development practices using Clean Architecture and Domain-Driven Design (DDD) principles. This project serves as both an educational resource and a production-ready template for building enterprise-grade APIs with .NET 8.0.

The project implements a complete Admin API for managing employee records, establishments, and related resources, demonstrating industry best practices in software architecture, security, observability, resilience, and DevOps automation.

---

## What We Built

### Core Application

A fully functional **Admin API** with the following capabilities:

#### Resource Management
- **Employee Management**: Complete CRUD operations with pagination, filtering, and sorting
- **Employee Roles**: Role-based access control and management
- **Establishment Management**: Multi-location business entity management
- **Establishment Components**: Addresses, contacts, phone numbers, and members
- **Social Media Platforms**: Platform registration and management

#### Key Features Implemented

**1. Clean Architecture Implementation**
- Strict layer separation (Domain, Application, Infrastructure, API)
- Dependency inversion with interface-based design
- Domain-centric approach with rich domain models
- Clear separation of concerns across all layers

**2. Domain-Driven Design Patterns**
- **Entities**: Employee, Establishment, EmployeeRole with identity-based equality
- **Value Objects**: PersonName, EmailAddress, PhoneNumber, Address, Money (immutable, value-based equality)
- **Aggregates**: Establishment with related addresses, contacts, phones, and members
- **Repositories**: Data access abstraction for each aggregate root
- **Specifications**: Encapsulated query logic for filtering and sorting
- **Strongly-Typed IDs**: Type-safe identifiers preventing ID mixing
- **Result Pattern**: Functional error handling without exceptions

**3. Authentication & Authorization**
- JWT-based authentication with access and refresh tokens
- Role-based authorization with custom policies (Admin, Manager, Staff)
- Azure Key Vault integration for secure secrets management
- Token revocation and refresh mechanism
- Password hashing with secure algorithms

**4. API Features**
- RESTful API design following HTTP standards
- API versioning (URL-based: `/api/v1.0/...`)
- Pagination with configurable page sizes
- Advanced filtering and sorting with Specification Pattern
- HATEOAS links for resource discoverability
- JSON Patch support for partial updates
- ProblemDetails error responses (RFC 7807)
- CORS configuration for cross-origin requests
- Response compression (Gzip/Brotli)
- Request/Response caching strategies

**5. Resilience & Performance**
- **Polly Resilience Policies**:
  - Retry with exponential backoff
  - Circuit breaker for fault isolation
  - Timeout policies
  - Fallback strategies
  - Bulkhead isolation
- Connection pooling optimization
- Database connection resilience
- Graceful shutdown handling
- Health checks for readiness and liveness probes

**6. Observability & Monitoring**
- **Structured Logging** with Serilog:
  - Console, File, and Seq sinks
  - Enrichers: CorrelationId, Environment, Thread, Process
  - Contextual logging with structured data
- **Distributed Tracing** with OpenTelemetry:
  - HTTP request tracing
  - SQL query tracing
  - Custom activity and span creation
- **Application Insights** integration:
  - Performance monitoring
  - Exception tracking
  - Usage analytics
  - Custom metrics and events
- **Health Check UI**: Visual dashboard for monitoring

**7. Data Management**
- **Soft Delete Pattern**: Preserves data integrity with logical deletion
- **Audit Trails**: Automatic tracking of CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
- **EF Core Interceptors**: SaveChanges interception for audit and soft delete
- **Database Migrations**: Version-controlled schema evolution
- **Data Seeding**: Automatic sample data generation with Bogus library
- **Global Query Filters**: Automatic filtering of soft-deleted entities

**8. DevOps & CI/CD**
- **Azure Pipelines** with multi-stage deployment:
  - Build and Test stage with code coverage
  - Code Quality Analysis stage
  - Docker build and push to Azure Container Registry
  - Environment-specific deployments (Dev, Staging, Production)
  - Approval gates for staging and production
  - Automated database migrations per environment
  - Smoke tests with health check validation
- **Docker Support**:
  - Multi-stage Dockerfile with security best practices
  - Non-root user execution
  - Health check configuration
  - Azure Container Registry integration
  - Docker Compose for local development
- **Database Migration Automation**:
  - EF Core migrations in deployment pipeline
  - Pre-migration database backups
  - Rollback scripts for emergency recovery

**9. Testing Strategy**
- **Domain Tests**: Entity and value object behavior validation (xUnit, FluentAssertions)
- **Application Tests**: Service layer business logic testing (xUnit, Moq)
- **Infrastructure Tests**: Repository and data access verification (xUnit, EF Core InMemory)
- **Shared Library Tests**: Common utilities and types testing
- **Architectural Tests**: Architecture rules enforcement (ArchUnitNET)
- **Test Coverage**: 834+ passing tests with >85% overall coverage

**10. Documentation & Developer Experience**
- **OpenAPI/Swagger**: Interactive API documentation with examples
- **XML Documentation**: Comprehensive inline documentation for all public APIs
- **Postman Collection**: Ready-to-use collection with automatic token management
- **API Error Codes Guide**: Complete error handling reference (400+ error scenarios)
- **README**: Detailed setup instructions and project overview
- **CONTRIBUTING**: Development workflow, coding standards, and PR guidelines
- **Pipeline Setup Guide**: CI/CD configuration documentation
- **Database Migrations Guide**: Migration management best practices
- **Docker Deployment Guide**: Container deployment instructions

---

## Technology Stack

### Backend Framework
- **.NET 8.0** - Latest LTS version with modern C# features
- **ASP.NET Core** - Web API framework with minimal APIs support
- **Entity Framework Core 8.0** - Object-relational mapper with SQL Server

### Architecture & Design Patterns
- **Clean Architecture** - Layer separation with dependency inversion
- **Domain-Driven Design (DDD)** - Rich domain models with entities and value objects
- **Repository Pattern** - Data access abstraction
- **Specification Pattern** - Encapsulated query logic
- **Result Pattern** - Functional error handling
- **CQRS (Command Query Responsibility Segregation)** - Read/write separation in design

### Authentication & Security
- **JWT (JSON Web Tokens)** - Stateless authentication
- **Microsoft.AspNetCore.Authentication.JwtBearer** - JWT authentication middleware
- **Azure Key Vault** - Secure secrets management
- **Azure.Identity** - Azure authentication and authorization
- **Azure.Extensions.AspNetCore.Configuration.Secrets** - Key Vault configuration

### Database & Data Access
- **SQL Server** - Relational database (LocalDB for development)
- **Entity Framework Core** - ORM with code-first migrations
- **EF Core Interceptors** - SaveChanges interception for audit and soft delete
- **Global Query Filters** - Automatic soft delete filtering

### Resilience & Performance
- **Polly** - Resilience and transient fault handling
- **Microsoft.Extensions.Http.Resilience** - HTTP client resilience
- **Response Caching** - In-memory response caching
- **Response Compression** - Gzip and Brotli compression

### Observability & Monitoring
- **Serilog** - Structured logging with multiple sinks
  - Serilog.AspNetCore
  - Serilog.Sinks.Console
  - Serilog.Sinks.File
  - Serilog.Sinks.Seq
  - Serilog.Sinks.ApplicationInsights
  - Serilog.Enrichers.Environment
  - Serilog.Enrichers.Thread
  - Serilog.Enrichers.Process
  - Serilog.Enrichers.CorrelationId
- **OpenTelemetry** - Distributed tracing and metrics
  - OpenTelemetry.Extensions.Hosting
  - OpenTelemetry.Instrumentation.AspNetCore
  - OpenTelemetry.Instrumentation.Http
  - OpenTelemetry.Instrumentation.SqlClient
  - OpenTelemetry.Exporter.Console
  - OpenTelemetry.Exporter.OpenTelemetryProtocol
  - Azure.Monitor.OpenTelemetry.Exporter
- **Application Insights** - Azure monitoring and analytics
  - Microsoft.ApplicationInsights.AspNetCore
- **Health Checks** - Application health monitoring
  - AspNetCore.HealthChecks.SqlServer
  - AspNetCore.HealthChecks.UI
  - AspNetCore.HealthChecks.UI.Client
  - AspNetCore.HealthChecks.UI.InMemory.Storage

### API Features
- **Asp.Versioning.Mvc** - API versioning
- **Asp.Versioning.Mvc.ApiExplorer** - API version discovery
- **Swashbuckle.AspNetCore** - OpenAPI/Swagger documentation
- **Swashbuckle.AspNetCore.Filters** - Request/response examples
- **Swashbuckle.AspNetCore.Annotations** - Enhanced Swagger annotations
- **Microsoft.AspNetCore.JsonPatch** - JSON Patch support
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson** - JSON serialization

### Testing Frameworks
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for unit tests
- **FluentAssertions** - Readable test assertions
- **ArchUnitNET** - Architecture validation
- **ArchUnitNET.xUnit** - xUnit integration for architecture tests
- **EF Core InMemory** - In-memory database for testing
- **Coverlet** - Code coverage collection
- **ReportGenerator** - Code coverage report generation

### DevOps & CI/CD
- **Azure Pipelines** - CI/CD automation
- **Docker** - Containerization
  - Multi-stage builds
  - Azure Container Registry (ACR)
  - Docker Compose for local development
- **Azure DevOps** - Project management and work tracking
- **GitHub** - Source control and collaboration

### Code Quality
- **SonarAnalyzer.CSharp** - Static code analysis
- **EditorConfig** - Consistent coding styles
- **Code Coverage** - >85% target coverage
- **Architecture Tests** - ArchUnitNET for architecture enforcement

### Development Tools
- **Visual Studio 2022** - Primary IDE
- **Visual Studio Code** - Alternative editor
- **Postman** - API testing and documentation
- **Azure Data Studio** - Database management
- **Seq** - Log aggregation and analysis (optional)
- **Docker Desktop** - Container management

### Data Generation
- **Bogus** - Realistic fake data generation for seeding

---

## Architecture Layers

### 1. Domain Layer (`TestNest.Admin.Domain`)
The innermost layer containing core business logic and domain models.

**Contents:**
- **Entities**: Employee, Establishment, EmployeeRole, User, RefreshToken
- **Value Objects**: PersonName, EmailAddress, PhoneNumber, Address, Money, EstablishmentName, etc.
- **Domain Interfaces**: Repository interfaces (IEmployeeRepository, IEstablishmentRepository)
- **Domain Exceptions**: Custom business rule exceptions
- **Enumerations**: Business enumerations and constants

**Dependencies**: None (pure domain logic)

### 2. Application Layer (`TestNest.Admin.Application`)
Contains application business logic and orchestrates domain operations.

**Contents:**
- **Services**: IEmployeeService, IEstablishmentService, IAuthService implementations
- **DTOs**: Request/Response data transfer objects
- **Specifications**: Filtering and sorting specifications
- **Validators**: Business rule validation logic
- **Service Contracts**: Service interfaces

**Dependencies**: Domain Layer only

### 3. Infrastructure Layer (`TestNest.Admin.Infrastructure`)
Implements external concerns and technical capabilities.

**Contents:**
- **Persistence**: EF Core DbContext, configurations, repositories
- **Interceptors**: SaveChanges interceptors for audit and soft delete
- **Seeders**: Database seeding logic with Bogus
- **External Services**: Third-party integrations
- **Identity**: Authentication and authorization implementations

**Dependencies**: Domain and Application layers

### 4. API Layer (`TestNest.Admin.API`)
The outermost layer handling HTTP requests and responses.

**Contents:**
- **Controllers**: API endpoints for each resource
- **Middleware**: Custom middleware components
- **Filters**: Action filters and exception filters
- **Program.cs**: Application startup and configuration
- **Swagger Examples**: Request/response examples for documentation

**Dependencies**: Application layer (not Infrastructure directly)

### 5. Shared Library (`TestNest.Admin.SharedLibrary`)
Cross-cutting concerns shared across layers.

**Contents:**
- **Common Utilities**: Result pattern, error handling
- **DTOs**: Shared data transfer objects
- **Strongly-Typed IDs**: Type-safe identifiers
- **Exceptions**: Common exception types
- **Authorization**: Authorization policies and constants

**Dependencies**: None (shared across all layers)

---

## Design Principles Applied

### SOLID Principles
- **Single Responsibility**: Each class has one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes are substitutable
- **Interface Segregation**: Client-specific interfaces
- **Dependency Inversion**: Depend on abstractions, not concretions

### Clean Code Practices
- Meaningful names for classes, methods, and variables
- Small, focused methods (single responsibility)
- Minimal method parameters (prefer objects)
- Comments only where necessary (self-documenting code)
- Consistent code formatting and organization

### Domain-Driven Design Tactical Patterns
- **Entities**: Identity-based objects with mutable state
- **Value Objects**: Immutable objects with value-based equality
- **Aggregates**: Consistency boundaries with root entities
- **Repositories**: Collection-like interface for domain objects
- **Domain Services**: Stateless domain operations
- **Specifications**: Reusable query logic

### API Design Best Practices
- RESTful resource naming and HTTP verb usage
- Proper HTTP status codes (200, 201, 204, 400, 401, 403, 404, 409, 500, 503)
- Versioned APIs for backward compatibility
- Pagination for large datasets
- HATEOAS for discoverability
- ProblemDetails for consistent error responses

---

## Security Features

### Authentication
- JWT-based stateless authentication
- Secure token generation with HMAC SHA-256
- Access tokens (short-lived, 1 hour)
- Refresh tokens (long-lived, 7 days)
- Token revocation mechanism

### Authorization
- Role-based access control (Admin, Manager, Staff)
- Policy-based authorization
- Resource-level permissions
- Endpoint protection with [Authorize] attributes

### Data Protection
- Password hashing with secure algorithms
- Connection string encryption
- Azure Key Vault for secrets management
- SQL injection prevention (parameterized queries)
- XSS protection with response encoding

### API Security
- CORS configuration
- Rate limiting
- Request size limits
- HTTPS enforcement
- Security headers (HSTS, CSP, X-Frame-Options)
- Input validation and sanitization

---

## Performance Optimizations

### Database Optimizations
- Connection pooling
- Query optimization with EF Core
- Eager loading for related entities
- Projection for read operations
- Batch operations where applicable

### Caching Strategies
- Response caching for GET requests
- In-memory caching for frequently accessed data
- Cache invalidation on updates

### Compression
- Response compression (Gzip/Brotli)
- Reduced payload sizes for bandwidth optimization

### Async/Await
- Non-blocking I/O operations
- Asynchronous database queries
- Asynchronous HTTP calls

---

## What Makes This Project Special

### 1. Production-Ready Code Quality
- Comprehensive error handling with Result Pattern
- Extensive test coverage (834+ tests)
- Architecture validation with ArchUnitNET
- Code quality analysis with SonarAnalyzer

### 2. Enterprise-Grade Features
- Complete CI/CD pipeline with Azure Pipelines
- Docker containerization with ACR integration
- Multi-environment deployments (Dev, Staging, Production)
- Comprehensive observability stack

### 3. Developer Experience
- Extensive documentation (README, CONTRIBUTING, API docs)
- Interactive Swagger UI with examples
- Postman collection for immediate testing
- Clear error messages and logging

### 4. Educational Value
- Demonstrates Clean Architecture in practice
- Shows DDD tactical patterns implementation
- Illustrates modern .NET best practices
- Provides reusable patterns and templates

### 5. Scalability & Maintainability
- Clear layer separation for easy modifications
- Interface-based design for testability
- Modular architecture for feature additions
- Versioned API for backward compatibility

---

## Use Cases

This project serves as:

### 1. Learning Resource
- Study modern .NET API development
- Understand Clean Architecture implementation
- Learn Domain-Driven Design patterns
- Explore enterprise application patterns

### 2. Project Template
- Bootstrap new API projects
- Reuse infrastructure setup
- Copy DevOps pipeline configuration
- Adapt domain models for similar domains

### 3. Reference Implementation
- Best practices demonstration
- Code review reference
- Architecture decision examples
- Testing strategy patterns

### 4. Portfolio Showcase
- Demonstrate technical skills
- Show understanding of enterprise patterns
- Highlight DevOps capabilities
- Prove code quality standards

---

## Project Statistics

- **Total Lines of Code**: ~50,000+ (excluding tests and generated code)
- **Test Coverage**: >85% overall
- **Test Count**: 834+ passing tests
- **Epics Completed**: 9
- **Features Implemented**: 42
- **User Stories Completed**: 16
- **Documentation Files**: 7+ comprehensive guides
- **API Endpoints**: 50+ fully documented
- **Controllers**: 10
- **Domain Entities**: 15+
- **Value Objects**: 20+
- **Git Commits**: 10+ with clean history
- **Development Time**: Multiple sprints across 7 epics

---

## Future Enhancements

Potential areas for expansion:

### Technical Improvements
- GraphQL API alongside REST
- gRPC services for internal communication
- Redis caching for distributed scenarios
- Message queue integration (RabbitMQ/Azure Service Bus)
- Event Sourcing implementation
- CQRS with separate read/write models

### Features
- Advanced reporting and analytics
- Bulk operations support
- File upload and management
- Notification system (email, SMS, push)
- Scheduled background jobs
- Data export (CSV, Excel, PDF)

### Infrastructure
- Kubernetes deployment manifests
- Terraform infrastructure as code
- Azure Service Fabric deployment
- Multi-region deployment
- Auto-scaling configuration
- Disaster recovery setup

---

## Acknowledgments

This project was built using industry-standard libraries and frameworks:

- Microsoft .NET Team for the excellent framework
- Entity Framework Core team for the powerful ORM
- Serilog contributors for structured logging
- Polly contributors for resilience patterns
- OpenTelemetry community for observability standards
- ArchUnitNET team for architecture testing
- Bogus library for realistic test data
- All open-source contributors whose libraries made this possible

---

## License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## Contact & Contributions

- **Author**: Dante Tura Salvador
- **GitHub**: https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture
- **Azure DevOps**: https://dev.azure.com/tengtium-io/TestNest.CleanArchitecture

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

**Built with ❤️ using .NET 8.0 and Clean Architecture principles**
