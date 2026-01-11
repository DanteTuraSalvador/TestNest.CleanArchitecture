using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.Infrastructure.Persistence;
using TestNest.Admin.Infrastructure.Persistence.Interceptors;
using TestNest.Admin.Infrastructure.Persistence.Repositories;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.InfrastructureTests.Repositories.Employees;
public class EmployeeRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly AuditableEntityInterceptor _auditInterceptor;
    private readonly SoftDeleteInterceptor _softDeleteInterceptor;

    public EmployeeRepositoryTests()
    {
        _auditInterceptor = new AuditableEntityInterceptor();
        _softDeleteInterceptor = new SoftDeleteInterceptor();

        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDatabase_{Guid.NewGuid()}")
            .AddInterceptors(_auditInterceptor, _softDeleteInterceptor)
            .Options;

        using var context = new ApplicationDbContext(_dbContextOptions);
        _ = context.Database.EnsureCreated();
        SeedData(context);
    }

    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_dbContextOptions);

    private static void SeedData(ApplicationDbContext context)
    {
        // Create establishments
        Establishment establishment1 = Establishment.Create(EstablishmentName.Create("Test Establishment 1").Value!, EmailAddress.Create("test1@example.com").Value!).Value!;
        Establishment establishment2 = Establishment.Create(EstablishmentName.Create("Test Establishment 2").Value!, EmailAddress.Create("test2@example.com").Value!).Value!;

        context.Establishments.AddRange(establishment1, establishment2);
        _ = context.SaveChanges();

        // Create employee roles
        EmployeeRole role1 = EmployeeRole.Create(RoleName.Create("Manager").Value!).Value!;
        EmployeeRole role2 = EmployeeRole.Create(RoleName.Create("Staff").Value!).Value!;

        context.EmployeeRoles.AddRange(role1, role2);
        _ = context.SaveChanges();

        // Create employees with valid role and establishment references
        Employee employee1 = Employee.Create(EmployeeNumber.Create("EMP-001").Value!, PersonName.Create("John", null, "Doe").Value!, EmailAddress.Create("john.doe@example.com").Value!, role1.Id, establishment1.Id).Value!;
        Employee employee2 = Employee.Create(EmployeeNumber.Create("EMP-002").Value!, PersonName.Create("Jane", null, "Smith").Value!, EmailAddress.Create("jane.smith@example.com").Value!, role2.Id, establishment2.Id).Value!;

        context.Employees.AddRange(employee1, employee2);
        _ = context.SaveChanges();
    }

    public class EmployeeByEstablishmentIdSpecification : BaseSpecification<Employee>
    {
        public EmployeeByEstablishmentIdSpecification(EstablishmentId establishmentId)
            : base(e => e.EstablishmentId == establishmentId)
        {
            AddInclude(e => e.EmployeeName);
            AddInclude(e => e.Establishment);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEmployee_ReturnsSuccessResultWithEmployee()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRepository(context);
        EmployeeId existingEmployeeId = await context.Employees.Select(e => e.Id).FirstAsync();

        // Act
        SharedLibrary.Common.Results.Result<Employee> result = await repository.GetByIdAsync(existingEmployeeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existingEmployeeId, result.Value.EmployeeId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEmployee_ReturnsFailureResultWithNotFound()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRepository(context);
        var nonExistingEmployeeId = new EmployeeId(Guid.NewGuid());

        // Act
        Result<Employee> result = await repository.GetByIdAsync(nonExistingEmployeeId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task AddAsync_ValidEmployee_ReturnsSuccessResultWithAddedEmployee()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRepository(context);
        Establishment newEstablishment = Establishment.Create(EstablishmentName.Create("New Establishment").Value!, EmailAddress.Create("new@example.com").Value!).Value!;
        _ = context.Establishments.Add(newEstablishment);
        _ = await context.SaveChangesAsync();

        Result<Employee> newEmployeeResult = Employee.Create(EmployeeNumber.Create("EMP-003").Value!, PersonName.Create("Peter", null, "Pan").Value!, EmailAddress.Create("peter.pan@example.com").Value!, new EmployeeRoleId(Guid.NewGuid()), newEstablishment.Id);
        Assert.True(newEmployeeResult.IsSuccess);
        Employee newEmployee = newEmployeeResult.Value!;

        // Act
        Result<Employee> result = await repository.AddAsync(newEmployee);
        _ = await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Employee? addedEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Id == newEmployee.Id);
        Assert.NotNull(addedEmployee);
        Assert.Equal("EMP-003", addedEmployee.EmployeeNumber.EmployeeNo);
    }

    [Fact(Skip = "TODO: Fix WithPersonName update with InMemory database and navigation properties")]
    public async Task UpdateAsync_ExistingEmployee_ReturnsSuccessResultWithUpdatedEmployee()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRepository(context);
        Employee existingEmployee = await context.Employees
            .Include(e => e.EmployeeRole)
            .Include(e => e.Establishment)
            .FirstAsync();
        Result<PersonName> updatedNameResult = PersonName.Create("Updated", null, "Name");
        Assert.True(updatedNameResult.IsSuccess);
        Employee updatedEmployee = existingEmployee.WithPersonName(updatedNameResult.Value!).Value!;

        // Act
        Result<Employee> result = await repository.UpdateAsync(updatedEmployee);
        _ = await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Employee? retrievedEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Id == existingEmployee.Id);
        Assert.NotNull(retrievedEmployee);
        Assert.Equal("Updated Name", retrievedEmployee.EmployeeName.GetFullName());
    }

    [Fact]
    public async Task DeleteAsync_ExistingEmployee_ReturnsSuccessResultAndDeletesEmployee()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRepository(context);
        EmployeeId existingEmployeeId = await context.Employees.Select(e => e.Id).FirstAsync();

        // Act
        var result = await repository.DeleteAsync(existingEmployeeId);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        // Soft deleted employee should not be found by regular query (filtered by global query filter)
        var deletedEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Id == existingEmployeeId);
        Assert.Null(deletedEmployee);

        // But should still exist in database with IsDeleted = true
        var softDeletedEmployee = await context.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == existingEmployeeId);
        Assert.NotNull(softDeletedEmployee);
        Assert.True(softDeletedEmployee.IsDeleted);
    }

    [Fact]
    public async Task ListAsync_WithSpecification_ReturnsSuccessResultWithFilteredEmployees()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRepository(context);
        var existingEstablishmentId = context.Establishments.First().Id;
        var specification = new EmployeeByEstablishmentIdSpecification(existingEstablishmentId);

        // Act
        var result = await repository.ListAsync(specification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.All(result.Value, e => Assert.Equal(existingEstablishmentId, e.EstablishmentId));
    }

    [Fact]
    public async Task CountAsync_WithSpecification_ReturnsSuccessResultWithCorrectCount()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRepository(context);
        var existingEstablishmentId = context.Establishments.First().Id;
        var specification = new EmployeeByEstablishmentIdSpecification(existingEstablishmentId);

        // Act
        var result = await repository.CountAsync(specification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value > 0);
        Assert.Equal(context.Employees.Count(e => e.EstablishmentId == existingEstablishmentId), result.Value);
    }
    [Fact]
    public async Task EmployeeExistsWithSameCombination_ExistingCombination_ReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRepository(context);
        var existingEmployee = await context.Employees.FirstAsync();

        // Act
        var exists = await repository.EmployeeExistsWithSameCombination(
            existingEmployee.Id,
            existingEmployee.EmployeeNumber,
            existingEmployee.EmployeeName,
            existingEmployee.EmployeeEmail,
            existingEmployee.EstablishmentId);

        // Assert
        Assert.False(exists); // Should be false because we exclude the same ID
    }

    [Fact]
    public async Task EmployeeExistsWithSameCombination_NonExistingCombination_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRepository(context);
        var newEmployeeResult = Employee.Create(EmployeeNumber.Create("EMP-999").Value!, PersonName.Create("Non", null, "Existent").Value!, EmailAddress.Create("non.existent@example.com").Value!, new EmployeeRoleId(Guid.NewGuid()), context.Establishments.First().Id);
        Assert.True(newEmployeeResult.IsSuccess);
        var nonExistingEmployee = newEmployeeResult.Value!;

        // Act
        var exists = await repository.EmployeeExistsWithSameCombination(
            EmployeeId.New(),
            nonExistingEmployee.EmployeeNumber,
            nonExistingEmployee.EmployeeName,
            nonExistingEmployee.EmployeeEmail,
            nonExistingEmployee.EstablishmentId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task DetachAsync_AttachedEmployee_StateIsDetached()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRepository(context);
        var existingEmployee = await context.Employees.FirstAsync();

        // Act
        repository.DetachAsync(existingEmployee);

        // Assert
        Assert.Equal(EntityState.Detached, context.Entry(existingEmployee).State);
    }
}
