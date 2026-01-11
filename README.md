# TestNest.CleanArchitecture

![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)
![License](https://img.shields.io/badge/license-MIT-blue)
![Build](https://img.shields.io/badge/build-passing-brightgreen)
![Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen)
![Tests](https://img.shields.io/badge/tests-834%20passing-brightgreen)
![Docker](https://img.shields.io/badge/Docker-enabled-2496ED?logo=docker)
![Azure](https://img.shields.io/badge/Azure-Pipelines-0078D4?logo=azuredevops)

**TestNest.CleanArchitecture** is a comprehensive demonstration of modern .NET API development practices using Clean Architecture and Domain-Driven Design (DDD) principles. This project provides a complete Admin API platform for managing resources with enterprise-grade features including authentication, authorization, resilience patterns, observability, and comprehensive DevOps tooling.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Authentication](#authentication)
- [Project Structure](#project-structure)
- [Testing](#testing)
- [CI/CD Pipeline](#cicd-pipeline)
- [Docker Support](#docker-support)
- [Contributing](#contributing)

---

## Overview

The **TestNest.CleanArchitecture** project serves as a demonstration and educational resource for showcasing modern API development practices using .NET and Domain-Driven Design principles. It provides a comprehensive Admin API with endpoints for managing employee records, establishments, and related resources, following RESTful principles and utilizing a **Service-Based Implementation** with a rich domain model.

---

## Key Features

### Core Features

- **Resource Management**: Full CRUD operations for employees, establishments, employee roles, and related entities
- **Service-Based Implementation**: Dedicated service layer containing core business logic, separated from HTTP concerns
- **Rich Domain Model**: Entities encapsulate behavior and data with Value Objects for immutable domain concepts
- **Automatic Data Seeding**: Uses Bogus library to populate realistic sample data on startup
- **Data Validation**: Robust server-side validation with Guard Clauses and Result Pattern
- **Error Handling**: Consistent error responses using RFC 7807 ProblemDetails standard
- **Asynchronous Operations**: Built with async/await throughout for improved performance

### Authentication & Security

- **JWT Authentication**: Secure token-based authentication with access and refresh tokens
- **Role-Based Authorization**: Fine-grained access control with policies (Admin, Manager, Staff)
- **Azure Key Vault Integration**: Secure secrets management for sensitive configuration
- **API Security Hardening**: Rate limiting, CORS policies, and security headers

### Data Access & Persistence

- **Entity Framework Core**: Code-first approach with SQL Server
- **Repository Pattern**: Abstracts data access with testable repositories
- **Unit of Work Pattern**: Manages transactions and persistence
- **Soft Delete Support**: Preserves data integrity with soft delete pattern
- **Audit Interceptors**: Automatic tracking of created/modified timestamps and users

### API Features

- **API Versioning**: URL-based versioning (e.g., `/api/v1.0/employees`)
- **Pagination**: Efficient handling of large datasets with customizable page sizes
- **Filtering & Sorting**: Specification Pattern for complex queries
- **HATEOAS Links**: Hypermedia links in collection responses for discoverability
- **Strongly-Typed IDs**: Type-safe identifiers throughout the API
- **JSON Patch Support**: Partial updates using JSON Patch documents

### Resilience & Performance

- **Polly Resilience Patterns**: Retry, circuit breaker, timeout, and fallback policies
- **Health Checks**: Comprehensive health monitoring for readiness and liveness
- **Response Caching**: Optimized response times with caching strategies
- **Database Connection Resilience**: Automatic retry on transient failures

### Observability & Monitoring

- **Structured Logging**: Serilog with enrichers for correlation IDs, process info, and environment
- **Distributed Tracing**: OpenTelemetry integration with Azure Application Insights
- **Application Insights**: Performance monitoring, exception tracking, and usage analytics
- **Health Check UI**: Visual dashboard for monitoring service health
- **Seq Integration**: Centralized log aggregation and analysis

### DevOps & CI/CD

- **Azure Pipelines**: Automated build, test, code quality, and deployment stages
- **Docker Support**: Production-ready multi-stage Dockerfile with security best practices
- **Database Migrations**: Automated EF Core migrations in deployment pipeline
- **Environment-Specific Deployments**: Dev, Staging, and Production with approval gates
- **Comprehensive Documentation**: API documentation, deployment guides, and architecture docs

### Documentation & Developer Experience

- **OpenAPI/Swagger**: Interactive API documentation with examples
- **XML Documentation**: Comprehensive inline documentation for all endpoints
- **Postman Collection**: Ready-to-use collection for all API endpoints
- **Error Code Reference**: Complete guide to error handling and status codes
- **Contributing Guidelines**: Clear instructions for new contributors

### Testing

- **Comprehensive Test Coverage**: Multi-layered testing approach
  - **Domain Tests**: Validating core domain entities and value objects
  - **Application Service Tests**: Business logic validation with Moq
  - **Repository Tests**: Data access layer verification
  - **Architectural Tests**: Enforcing design principles with ArchUnitNET
  - **Integration Tests**: End-to-end API testing

---

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────┐
│            API Layer (Presentation)             │
│  Controllers, Middleware, Program.cs            │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│          Application Layer                      │
│  Services, DTOs, Specifications, Interfaces     │
└────────────┬────────────────────────────────────┘
             │
┌────────────▼────────────────────────────────────┐
│            Domain Layer                         │
│  Entities, Value Objects, Domain Logic          │
└─────────────────────────────────────────────────┘
             ▲
┌────────────┴────────────────────────────────────┐
│        Infrastructure Layer                     │
│  EF Core, Repositories, External Services       │
└─────────────────────────────────────────────────┘
```

### Domain-Driven Design Concepts

- **Entities**: Employee, Establishment, EmployeeRole, etc.
- **Value Objects**: PersonName, EmailAddress, PhoneNumber, Address, etc.
- **Aggregates**: Establishment with related addresses, contacts, phones, and members
- **Repositories**: Data access abstraction for each aggregate root
- **Specifications**: Encapsulated query logic for filtering and sorting
- **Domain Events**: (Planned for future implementation)

---

## Getting Started

### Prerequisites

Before running the application, ensure you have the following installed:

- **.NET 8.0 SDK** or later: [Download here](https://dotnet.microsoft.com/download)
- **SQL Server**: LocalDB (included with Visual Studio) or SQL Server 2019+
- **Docker Desktop**: [Download here](https://www.docker.com/products/docker-desktop) (optional, for containerized database)
- **Visual Studio 2022** or **Visual Studio Code** (recommended)
- **Git**: [Download here](https://git-scm.com/downloads)

### Optional Tools

- **Azure Data Studio** or **SQL Server Management Studio** for database management
- **Postman** or **Insomnia** for API testing
- **Seq** for log aggregation (optional): [Download here](https://datalust.co/seq)

### Installation

1. **Clone the repository:**

```bash
git clone https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture.git
cd TestNest.CleanArchitecture
```

2. **Restore NuGet packages:**

```bash
dotnet restore
```

3. **Set up the database:**

The application supports two database options:

#### Option A: Using LocalDB (Recommended for Development)

LocalDB is installed with Visual Studio and requires no additional setup.

```bash
# Apply database migrations
dotnet ef database update --project TestNest.Admin.Infrastructure --startup-project TestNest.Admin.API
```

#### Option B: Using Docker SQL Server

```bash
# Start SQL Server container
docker compose up -d

# Apply database migrations
dotnet ef database update --project TestNest.Admin.Infrastructure --startup-project TestNest.Admin.API
```

4. **Configure Application Settings (Optional):**

The default configuration works out of the box. For custom settings:

```bash
# Copy sample configuration
cp TestNest.Admin.API/appsettings.json TestNest.Admin.API/appsettings.Development.json

# Edit configuration
# Update connection strings, JWT settings, etc.
```

### Running the Application

#### Using Visual Studio

1. Open `TestNest.CleanArchitecture.sln` in Visual Studio 2022
2. Ensure `TestNest.Admin.API` is set as the startup project
3. Press **F5** or click **Debug > Start Debugging**
4. The API will launch in your browser at `https://localhost:5001`

#### Using .NET CLI

```bash
# Navigate to the API project
cd TestNest.Admin.API

# Run the application
dotnet run

# The API will be available at:
# - HTTPS: https://localhost:5001
# - HTTP: http://localhost:5000
```

#### Using Docker

```bash
# Build and run with Docker Compose
docker compose up --build

# The API will be available at:
# - https://localhost:5001
# - http://localhost:5000
```

### First-Time Setup

On first run, the application will:

1. **Create the database** if it doesn't exist
2. **Apply all migrations** to create the schema
3. **Seed sample data** using Bogus library:
   - Default admin user
   - Sample employees, roles, and establishments
   - Realistic test data for development

### Verifying the Installation

1. **Open Swagger UI:**
   - Navigate to: `https://localhost:5001/swagger`
   - You should see the interactive API documentation

2. **Check Health Status:**
   - Navigate to: `https://localhost:5001/health`
   - You should see a JSON response with status "Healthy"

3. **Login with Default Credentials:**
   ```json
   POST /api/v1.0/Auth/login
   {
     "email": "admin@testnest.com",
     "password": "Admin@123"
   }
   ```

4. **View Health Check UI** (Development only):
   - Navigate to: `https://localhost:5001/health-ui`
   - Visual dashboard of all health checks

---

## API Documentation

### Swagger/OpenAPI

Interactive API documentation is available in **Development** environment:

- **Swagger UI**: https://localhost:5001/swagger
- **OpenAPI JSON**: https://localhost:5001/swagger/v1/swagger.json

The Swagger UI provides:
- Complete endpoint documentation with examples
- Request/response schemas
- Authentication support (click "Authorize" to add JWT token)
- "Try it out" functionality for testing endpoints

### Postman Collection

Import the Postman collection for ready-to-use API requests:

1. **Import Collection:**
   - File: `docs/TestNest.Admin.API.postman_collection.json`
   - Import into Postman

2. **Set Environment Variables:**
   - `base_url`: `https://localhost:5001`
   - `api_version`: `1.0`

3. **Authenticate:**
   - Run the "Login" request
   - Access token will be automatically saved to collection variables
   - All subsequent requests will use the token

### Additional Documentation

- **API Error Codes**: `docs/API_ERROR_CODES.md` - Complete error handling guide
- **Pipeline Setup**: `docs/PIPELINE_SETUP.md` - CI/CD pipeline configuration
- **Database Migrations**: `docs/DATABASE_MIGRATIONS.md` - Migration management guide
- **Docker Deployment**: `docs/DOCKER_DEPLOYMENT.md` - Container deployment guide

---

## Authentication

The API uses **JWT (JSON Web Token)** based authentication with refresh tokens.

### Authentication Flow

1. **Login** to obtain tokens:
   ```bash
   POST /api/v1.0/Auth/login
   Content-Type: application/json

   {
     "email": "admin@testnest.com",
     "password": "Admin@123"
   }
   ```

   Response:
   ```json
   {
     "accessToken": "eyJhbGc...",
     "refreshToken": "eyJhbGc...",
     "expiresIn": 3600
   }
   ```

2. **Use Access Token** in subsequent requests:
   ```bash
   GET /api/v1.0/Employees
   Authorization: Bearer eyJhbGc...
   ```

3. **Refresh Token** when access token expires:
   ```bash
   POST /api/v1.0/Auth/refresh
   Content-Type: application/json

   {
     "refreshToken": "eyJhbGc..."
   }
   ```

4. **Revoke Token** on logout:
   ```bash
   POST /api/v1.0/Auth/revoke
   Content-Type: application/json

   {
     "refreshToken": "eyJhbGc..."
   }
   ```

### Authorization Policies

The API implements role-based authorization with the following policies:

| Policy | Required Role | Endpoints |
|--------|---------------|-----------|
| `RequireAdmin` | Admin | Create/Update/Delete Roles, Delete Establishments |
| `RequireManager` | Manager | Most write operations |
| `RequireManagerOrAdmin` | Manager or Admin | Employee and Establishment management |
| `RequireStaff` | Staff | Read operations |

### Default Users

| Email | Password | Role | Description |
|-------|----------|------|-------------|
| admin@testnest.com | Admin@123 | Admin | Full system access |

---

## Project Structure

```
TestNest.CleanArchitecture/
├── TestNest.Admin.API/                 # API Layer (Presentation)
│   ├── Controllers/                     # API Controllers
│   ├── Middleware/                      # Custom middleware
│   ├── Swagger/                         # Swagger examples and filters
│   ├── Helpers/                         # Helper utilities
│   ├── Program.cs                       # Application entry point
│   └── appsettings.json                 # Configuration
│
├── TestNest.Admin.Application/          # Application Layer
│   ├── Contracts/                       # Service interfaces
│   ├── Services/                        # Business logic services
│   ├── Specifications/                  # Query specifications
│   └── Validators/                      # Validation logic
│
├── TestNest.Admin.Domain/               # Domain Layer
│   ├── Entities/                        # Domain entities
│   ├── ValueObjects/                    # Value objects
│   ├── Exceptions/                      # Domain exceptions
│   └── Interfaces/                      # Domain interfaces
│
├── TestNest.Admin.Infrastructure/       # Infrastructure Layer
│   ├── Persistence/                     # EF Core DbContext
│   │   ├── Configurations/              # Entity configurations
│   │   ├── Repositories/                # Repository implementations
│   │   ├── Interceptors/                # EF Core interceptors
│   │   └── Seeders/                     # Data seeders
│   ├── External/                        # External services
│   └── Identity/                        # Authentication services
│
├── TestNest.Admin.SharedLibrary/        # Shared Library
│   ├── Common/                          # Common utilities
│   ├── Dtos/                            # Data Transfer Objects
│   ├── StronglyTypedIds/                # Strongly-typed identifiers
│   ├── Exceptions/                      # Shared exceptions
│   └── Authorization/                   # Authorization policies
│
├── Tests/                               # Test Projects
│   ├── TestNest.Admin.DomainTests/      # Domain layer tests
│   ├── TestNest.Admin.ApplicationTests/ # Service layer tests
│   ├── TestNest.Admin.InfrastructureTests/ # Repository tests
│   ├── TestNest.Admin.SharedLibraryTests/ # Shared library tests
│   └── TestNest.Admin.ArchitectureTests/ # Architecture validation
│
├── docs/                                # Documentation
│   ├── API_ERROR_CODES.md               # Error handling guide
│   ├── PIPELINE_SETUP.md                # CI/CD setup guide
│   ├── DATABASE_MIGRATIONS.md           # Migration guide
│   ├── DOCKER_DEPLOYMENT.md             # Docker guide
│   └── TestNest.Admin.API.postman_collection.json # Postman collection
│
├── scripts/                             # Utility scripts
│   └── rollback-migration.ps1           # Database rollback script
│
├── Dockerfile                           # Production Dockerfile
├── docker-compose.yml                   # Docker Compose configuration
├── azure-pipelines.yml                  # CI/CD pipeline
├── .dockerignore                        # Docker build exclusions
└── README.md                            # This file
```

---

## Testing

The project includes comprehensive test coverage across multiple layers:

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/TestNest.Admin.DomainTests

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View coverage report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coveragereport
```

### Test Projects

| Project | Purpose | Framework |
|---------|---------|-----------|
| `TestNest.Admin.DomainTests` | Domain entities and value objects | xUnit, FluentAssertions |
| `TestNest.Admin.ApplicationTests` | Business logic and services | xUnit, Moq, FluentAssertions |
| `TestNest.Admin.InfrastructureTests` | Repository and data access | xUnit, EF Core InMemory |
| `TestNest.Admin.SharedLibraryTests` | Utilities and common types | xUnit, FluentAssertions |
| `TestNest.Admin.ArchitectureTests` | Architecture rules enforcement | xUnit, ArchUnitNET |

### Test Coverage Goals

- **Domain Layer**: >90% coverage
- **Application Layer**: >85% coverage
- **Infrastructure Layer**: >80% coverage
- **Overall**: >85% coverage

---

## CI/CD Pipeline

The project includes a comprehensive Azure Pipelines configuration.

### Pipeline Stages

1. **Build and Test**: Restore, build, and run all tests with code coverage
2. **Code Quality**: Static analysis and code quality checks
3. **Docker**: Build and push container images to Azure Container Registry
4. **Deploy Dev**: Automatic deployment to Development environment
5. **Deploy Staging**: Manual approval required for Staging deployment
6. **Deploy Production**: Manual approval required for Production deployment

### Pipeline Features

- **Automated Testing**: All tests run on every commit
- **Code Coverage Reporting**: Cobertura format with threshold enforcement
- **Database Migrations**: Automatic migration application per environment
- **Docker Support**: Container builds with BuildID and latest tags
- **Approval Gates**: Manual approval for Staging and Production
- **Smoke Tests**: Health check validation after deployment

### Setting Up the Pipeline

See `docs/PIPELINE_SETUP.md` for detailed instructions.

---

## Docker Support

### Local Docker Development

```bash
# Build the Docker image
docker build -t testnest-admin-api .

# Run the container
docker run -p 8080:8080 -e ConnectionStrings__DefaultConnection="your-connection-string" testnest-admin-api

# Using Docker Compose (recommended)
docker compose up --build
```

### Docker Image Features

- **Multi-stage build**: Optimized for size and security
- **Non-root user**: Runs as unprivileged `appuser`
- **Health checks**: Built-in container health monitoring
- **Environment variables**: Full configuration support
- **Production-ready**: Security best practices applied

### Azure Container Registry

Images are automatically pushed to Azure Container Registry via the CI/CD pipeline.

See `docs/DOCKER_DEPLOYMENT.md` for deployment guides.

---

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines on:

- Code of Conduct
- Development workflow
- Coding standards
- Pull request process
- Testing requirements
- Documentation guidelines

---

## Support and Resources

### Documentation

- [API Error Codes](docs/API_ERROR_CODES.md)
- [Pipeline Setup](docs/PIPELINE_SETUP.md)
- [Database Migrations](docs/DATABASE_MIGRATIONS.md)
- [Docker Deployment](docs/DOCKER_DEPLOYMENT.md)

### Getting Help

- **Issues**: [GitHub Issues](https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture/issues)
- **Discussions**: [GitHub Discussions](https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture/discussions)

### Learning Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [OpenTelemetry](https://opentelemetry.io/docs/)

---

## Acknowledgments

Built with ❤️ using:

- **.NET 8.0** - Modern, high-performance framework
- **Entity Framework Core** - Object-relational mapper
- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing and observability
- **Polly** - Resilience and transient fault handling
- **Swagger/OpenAPI** - API documentation
- **ArchUnitNET** - Architecture testing
- **Bogus** - Realistic test data generation
- **FluentAssertions** - Readable test assertions
- **Moq** - Mocking framework for unit tests

---

**Happy Coding!** 🚀
