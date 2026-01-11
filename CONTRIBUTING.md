# Contributing to TestNest Admin API

Thank you for considering contributing to TestNest Admin API! This document provides guidelines and instructions for contributing to this project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)
- [Documentation Guidelines](#documentation-guidelines)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Branch Naming Conventions](#branch-naming-conventions)

---

## Code of Conduct

### Our Pledge

We as members, contributors, and leaders pledge to make participation in our community a harassment-free experience for everyone, regardless of age, body size, visible or invisible disability, ethnicity, sex characteristics, gender identity and expression, level of experience, education, socio-economic status, nationality, personal appearance, race, religion, or sexual identity and orientation.

### Our Standards

Examples of behavior that contributes to a positive environment:

- Using welcoming and inclusive language
- Being respectful of differing viewpoints and experiences
- Gracefully accepting constructive criticism
- Focusing on what is best for the community
- Showing empathy towards other community members

Examples of unacceptable behavior:

- The use of sexualized language or imagery
- Trolling, insulting/derogatory comments, and personal or political attacks
- Public or private harassment
- Publishing others' private information without explicit permission
- Other conduct which could reasonably be considered inappropriate in a professional setting

---

## Getting Started

### Prerequisites

Before you begin contributing, ensure you have:

1. **.NET 8.0 SDK** installed
2. **Visual Studio 2022** or **Visual Studio Code** with C# extension
3. **Git** configured with your identity
4. **Docker Desktop** (optional, for containerized development)
5. **SQL Server** (LocalDB or containerized)

### Fork and Clone

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/TestNest.CleanArchitecture.git
   cd TestNest.CleanArchitecture
   ```

3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture.git
   ```

4. **Verify remotes**:
   ```bash
   git remote -v
   # origin    https://github.com/YOUR-USERNAME/TestNest.CleanArchitecture.git (fetch)
   # origin    https://github.com/YOUR-USERNAME/TestNest.CleanArchitecture.git (push)
   # upstream  https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture.git (fetch)
   # upstream  https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture.git (push)
   ```

### Initial Setup

1. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

2. **Apply database migrations**:
   ```bash
   dotnet ef database update --project TestNest.Admin.Infrastructure --startup-project TestNest.Admin.API
   ```

3. **Run the application**:
   ```bash
   dotnet run --project TestNest.Admin.API
   ```

4. **Run the tests**:
   ```bash
   dotnet test
   ```

---

## Development Workflow

### 1. Stay Synchronized

Always sync with the upstream repository before starting new work:

```bash
# Fetch upstream changes
git fetch upstream

# Merge upstream changes into your main branch
git checkout main
git merge upstream/main

# Push to your fork
git push origin main
```

### 2. Create a Feature Branch

Create a new branch for your feature or bug fix:

```bash
# Create and checkout a new branch
git checkout -b feature/your-feature-name

# Or for bug fixes
git checkout -b fix/your-bug-fix-name
```

See [Branch Naming Conventions](#branch-naming-conventions) for naming guidelines.

### 3. Make Your Changes

- Write clean, well-documented code following our [Coding Standards](#coding-standards)
- Add or update tests for your changes
- Update documentation as needed
- Ensure all tests pass locally

### 4. Commit Your Changes

Follow our [Commit Message Guidelines](#commit-message-guidelines):

```bash
git add .
git commit -m "feat: add employee search by email functionality"
```

### 5. Push to Your Fork

```bash
git push origin feature/your-feature-name
```

### 6. Create a Pull Request

1. Go to the original repository on GitHub
2. Click "New Pull Request"
3. Select your fork and branch
4. Fill out the pull request template
5. Submit the pull request

---

## Coding Standards

### C# Coding Style

We follow the [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions) with these specific guidelines:

#### Naming Conventions

- **PascalCase** for classes, methods, properties, and public fields
  ```csharp
  public class EmployeeService
  {
      public EmployeeResponse GetEmployee(EmployeeId id) { }
  }
  ```

- **camelCase** for private fields with underscore prefix
  ```csharp
  private readonly IEmployeeRepository _employeeRepository;
  ```

- **camelCase** for method parameters and local variables
  ```csharp
  public void ProcessEmployee(EmployeeId employeeId)
  {
      var employee = GetEmployee(employeeId);
  }
  ```

- **PascalCase** for constants
  ```csharp
  public const int MaxEmployeesPerPage = 100;
  ```

#### Code Organization

- One class per file
- File name must match the class name
- Organize using statements alphabetically
- Remove unused using statements
- Use file-scoped namespaces (.NET 6+)
  ```csharp
  namespace TestNest.Admin.Application.Services;

  public class EmployeeService
  {
      // Implementation
  }
  ```

#### Method Structure

- Keep methods short and focused (single responsibility)
- Maximum method length: 50 lines (guideline, not strict rule)
- Extract complex logic into private methods
- Use meaningful names that describe what the method does

#### Comments and Documentation

- **XML documentation** required for all public APIs:
  ```csharp
  /// <summary>
  /// Creates a new employee in the system.
  /// </summary>
  /// <param name="request">The employee creation request.</param>
  /// <returns>The created employee response.</returns>
  /// <exception cref="ValidationException">Thrown when validation fails.</exception>
  public async Task<Result<EmployeeResponse>> CreateEmployeeAsync(EmployeeForCreationRequest request)
  ```

- Use comments sparingly for complex logic
- Prefer self-documenting code over comments

### Clean Architecture Principles

#### Layer Separation

- **Domain Layer**: No dependencies on other layers
- **Application Layer**: Depends only on Domain
- **Infrastructure Layer**: Depends on Domain and Application
- **API Layer**: Depends on Application only (not Infrastructure directly)

#### Dependency Rules

```
API ──────────> Application ──────────> Domain
                     ▲                      ▲
                     │                      │
              Infrastructure ───────────────┘
```

- Dependencies point inward
- Use interfaces to invert dependencies
- Never reference Infrastructure from Application or Domain

#### Repository Pattern

- One repository per aggregate root
- Repositories belong in Infrastructure layer
- Interfaces belong in Domain layer
  ```csharp
  // Domain Layer
  public interface IEmployeeRepository
  {
      Task<Employee?> GetByIdAsync(EmployeeId id);
  }

  // Infrastructure Layer
  public class EmployeeRepository : IEmployeeRepository
  {
      public async Task<Employee?> GetByIdAsync(EmployeeId id) { }
  }
  ```

### Domain-Driven Design Patterns

#### Value Objects

- Immutable
- Equality based on value, not identity
- Validation in constructor
- Static factory methods for creation
  ```csharp
  public sealed class EmailAddress : ValueObject
  {
      public string Value { get; }

      private EmailAddress(string value) => Value = value;

      public static Result<EmailAddress> Create(string email)
      {
          if (string.IsNullOrWhiteSpace(email))
              return Result<EmailAddress>.Failure(new Error("Email", "Email is required"));

          // Validation logic
          return Result<EmailAddress>.Success(new EmailAddress(email));
      }

      protected override IEnumerable<object> GetEqualityComponents()
      {
          yield return Value;
      }
  }
  ```

#### Entities

- Mutable
- Identity-based equality
- Rich domain models with behavior
- Private setters, public methods for modifications
  ```csharp
  public class Employee : Entity<EmployeeId>
  {
      public EmployeeNumber EmployeeNumber { get; private set; }
      public PersonName PersonName { get; private set; }

      private Employee() { } // For EF Core

      public static Result<Employee> Create(...)
      {
          // Creation logic with validation
      }

      public Result UpdatePersonName(PersonName newName)
      {
          // Business logic for updating name
          PersonName = newName;
          return Result.Success();
      }
  }
  ```

#### Strongly-Typed IDs

- Use strongly-typed identifiers
- Prevents accidental ID mixing
  ```csharp
  public readonly record struct EmployeeId(Guid Value);
  public readonly record struct EstablishmentId(Guid Value);
  ```

### Error Handling

#### Result Pattern

- Use `Result<T>` for operations that can fail
- Never throw exceptions for business logic failures
- Use exceptions only for unexpected errors
  ```csharp
  public async Task<Result<EmployeeResponse>> CreateEmployeeAsync(EmployeeForCreationRequest request)
  {
      // Validation
      if (await _repository.ExistsByEmailAsync(request.EmailAddress))
          return Result<EmployeeResponse>.Failure(
              new Error("DuplicateEmail", "Email already exists"),
              ErrorType.Conflict
          );

      // Success path
      var employee = Employee.Create(...);
      await _repository.AddAsync(employee);
      return Result<EmployeeResponse>.Success(employee.ToResponse());
  }
  ```

#### Exception Handling

- Catch specific exceptions
- Log exceptions with context
- Return appropriate HTTP status codes in API layer
- Never swallow exceptions

### Async/Await Guidelines

- Use `async`/`await` for all I/O operations
- Suffix async methods with `Async`
- Avoid `async void` (except event handlers)
- Use `ConfigureAwait(false)` in library code (not needed in ASP.NET Core)
- Don't block on async code (`Task.Wait()`, `.Result`)

---

## Testing Requirements

### Test Coverage

All contributions must include appropriate tests:

- **New features**: Add comprehensive tests covering happy path and edge cases
- **Bug fixes**: Add regression tests demonstrating the fix
- **Refactoring**: Ensure existing tests still pass

### Test Coverage Targets

- **Domain Layer**: ≥ 90% coverage
- **Application Layer**: ≥ 85% coverage
- **Infrastructure Layer**: ≥ 80% coverage
- **Overall Project**: ≥ 85% coverage

### Test Structure

Follow the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task CreateEmployee_ValidRequest_ReturnsSuccessResult()
{
    // Arrange
    var request = new EmployeeForCreationRequest { /* ... */ };
    var service = new EmployeeService(_mockRepository.Object);

    // Act
    var result = await service.CreateEmployeeAsync(request);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    _mockRepository.Verify(r => r.AddAsync(It.IsAny<Employee>()), Times.Once);
}
```

### Test Naming

Use descriptive test names following the pattern:
`MethodName_Scenario_ExpectedBehavior`

```csharp
[Fact]
public async Task CreateEmployee_DuplicateEmail_ReturnsConflictError() { }

[Fact]
public async Task GetEmployee_NonExistentId_ReturnsNotFoundError() { }

[Fact]
public async Task UpdateEmployee_ValidData_ReturnsUpdatedEmployee() { }
```

### Unit Test Guidelines

- Test one thing per test method
- Use mocking frameworks (Moq) for dependencies
- Avoid test interdependencies
- Make tests fast and isolated
- Use FluentAssertions for readable assertions

### Integration Test Guidelines

- Use test databases (in-memory or test containers)
- Clean up test data after each test
- Test the full stack when appropriate
- Mock external dependencies

---

## Pull Request Process

### Before Submitting

Ensure your pull request meets these requirements:

1. **All tests pass**:
   ```bash
   dotnet test
   ```

2. **Code builds without warnings**:
   ```bash
   dotnet build
   ```

3. **Code coverage meets targets**:
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
   ```

4. **Architecture tests pass**:
   ```bash
   dotnet test Tests/TestNest.Admin.ArchitectureTests
   ```

5. **No security vulnerabilities**:
   ```bash
   dotnet list package --vulnerable
   ```

### Pull Request Template

When creating a pull request, include:

#### Description
- Clear description of what the PR does
- Link to related issue(s) if applicable
- Screenshots for UI changes (if applicable)

#### Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to break)
- [ ] Documentation update

#### Checklist
- [ ] My code follows the coding standards of this project
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have added XML documentation for public APIs
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or my feature works
- [ ] New and existing unit tests pass locally with my changes
- [ ] Any dependent changes have been merged and published

### Review Process

1. **Automated Checks**: CI/CD pipeline runs automatically
2. **Code Review**: At least one maintainer must approve
3. **Address Feedback**: Make requested changes
4. **Squash Commits**: Maintainers may squash commits before merging
5. **Merge**: Maintainer merges the pull request

---

## Documentation Guidelines

### Code Documentation

- **XML comments** for all public APIs (classes, methods, properties)
- **Inline comments** for complex algorithms or non-obvious code
- **README updates** for new features or significant changes
- **API documentation** examples for Swagger

### Documentation Files

When adding new features, update:

- **README.md**: High-level feature description, usage examples
- **API_ERROR_CODES.md**: New error codes and scenarios
- **Postman Collection**: New endpoints
- **Architecture documentation**: For architectural changes

### Example Documentation

```csharp
/// <summary>
/// Creates a new employee in the system.
/// </summary>
/// <param name="request">The employee creation request containing employee details.</param>
/// <returns>
/// A <see cref="Result{EmployeeResponse}"/> containing the created employee if successful,
/// or error details if the operation fails.
/// </returns>
/// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
/// <remarks>
/// This method performs the following validations:
/// <list type="bullet">
/// <item><description>Validates that the employee email is unique</description></item>
/// <item><description>Validates that the employee number is unique</description></item>
/// <item><description>Validates that the assigned role exists</description></item>
/// <item><description>Validates that the assigned establishment exists</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var request = new EmployeeForCreationRequest
/// {
///     EmployeeNumber = "EMP-2024-001",
///     FirstName = "John",
///     LastName = "Doe",
///     EmailAddress = "john.doe@example.com",
///     EmployeeRoleId = roleId,
///     EstablishmentId = establishmentId
/// };
/// var result = await employeeService.CreateEmployeeAsync(request);
/// </code>
/// </example>
public async Task<Result<EmployeeResponse>> CreateEmployeeAsync(EmployeeForCreationRequest request)
{
    // Implementation
}
```

---

## Commit Message Guidelines

Follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

### Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code changes that neither fix a bug nor add a feature
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Changes to build process, auxiliary tools, or dependencies

### Scope (Optional)

The scope should be the name of the affected module or layer:

- `domain`
- `application`
- `infrastructure`
- `api`
- `tests`

### Examples

```
feat(api): add employee search by email endpoint

Add new GET endpoint to search employees by email address.
Includes pagination and filtering support.

Closes #123
```

```
fix(domain): correct employee number validation

Employee numbers must be exactly 11 characters in format EMP-YYYY-NNN.
Previous validation only checked minimum length.

Fixes #456
```

```
docs(readme): update installation instructions

Add instructions for setting up SQL Server with Docker.
Update screenshots to reflect new UI changes.
```

```
refactor(application): extract employee validation logic

Move validation logic from service to separate validator classes.
Improves testability and separation of concerns.
```

### Breaking Changes

For breaking changes, add `BREAKING CHANGE:` in the footer:

```
feat(api)!: change employee ID format to GUID

BREAKING CHANGE: Employee IDs are now GUIDs instead of integers.
Clients must update their integrations to use the new ID format.

Migration guide: docs/migration-v2.md
```

---

## Branch Naming Conventions

Use descriptive branch names following these patterns:

### Feature Branches

```
feature/short-description
feature/issue-number-short-description
```

Examples:
- `feature/employee-search`
- `feature/123-add-soft-delete`

### Bug Fix Branches

```
fix/short-description
fix/issue-number-short-description
```

Examples:
- `fix/validation-error`
- `fix/456-employee-duplicate-check`

### Documentation Branches

```
docs/short-description
```

Examples:
- `docs/update-readme`
- `docs/api-error-codes`

### Refactoring Branches

```
refactor/short-description
```

Examples:
- `refactor/extract-validation`
- `refactor/simplify-employee-service`

---

## Getting Help

If you need help or have questions:

- **GitHub Issues**: [Create an issue](https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture/issues)
- **GitHub Discussions**: [Start a discussion](https://github.com/DanteTuraSalvador/TestNest.CleanArchitecture/discussions)
- **Email**: For private inquiries, contact the maintainers

---

## License

By contributing to TestNest Admin API, you agree that your contributions will be licensed under the MIT License.

---

## Recognition

Contributors will be recognized in:

- The project README.md
- Release notes for their contributions
- The project's contributors page on GitHub

Thank you for contributing to TestNest Admin API! 🎉
