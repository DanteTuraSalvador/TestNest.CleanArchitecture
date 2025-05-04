using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Infrastructure.Persistence;
using TestNest.Admin.Infrastructure.Persistence.Repositories;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.InfrastructureTests.Repositories.Employees;
public class EmployeeRoleRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;

    public EmployeeRoleRepositoryTests()
    {
        // Use an in-memory database for testing
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestEmployeeRoleDatabase")
            .Options;

        // Ensure the database is created and seeded for each test
        using var context = new ApplicationDbContext(_dbContextOptions);
        _ = context.Database.EnsureCreated();
        SeedData(context);
    }

    private ApplicationDbContext CreateContext() => new ApplicationDbContext(_dbContextOptions);

    private static void SeedData(ApplicationDbContext context)
    {
        var role1Result = RoleName.Create("Administrator");
        var role2Result = RoleName.Create("Manager");

        if (role1Result.IsSuccess && role2Result.IsSuccess)
        {
            context.EmployeeRoles.AddRange(
                EmployeeRole.Create(role1Result.Value!).Value!,
                EmployeeRole.Create(role2Result.Value!).Value!
            );
            context.SaveChanges();
        }
    }

    public class AllEmployeeRolesSpecification : BaseSpecification<EmployeeRole>
    {
        public AllEmployeeRolesSpecification()
        {
            AddInclude(r => r.RoleName);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsSuccessResultWithRole()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var existingRoleId = context.EmployeeRoles.First().Id;

        // Act
        var result = await repository.GetByIdAsync(existingRoleId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existingRoleId, result.Value.EmployeeRoleId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingRole_ReturnsFailureResultWithNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var nonExistingRoleId = new EmployeeRoleId(Guid.NewGuid());

        // Act
        var result = await repository.GetByIdAsync(nonExistingRoleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task GetEmployeeRoleByNameAsync_ExistingName_ReturnsSuccessResultWithRole()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var existingRoleName = context.EmployeeRoles.First().RoleName.Name;

        // Act
        var result = await repository.GetEmployeeRoleByNameAsync(existingRoleName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(existingRoleName, result.Value.RoleName.Name);
    }

    [Fact]
    public async Task GetEmployeeRoleByNameAsync_NonExistingName_ReturnsFailureResultWithNotFound()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        const string nonExistingRoleName = "NonExistentRole";

        // Act
        var result = await repository.GetEmployeeRoleByNameAsync(nonExistingRoleName);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task AddAsync_ValidRole_ReturnsSuccessResultWithAddedRole()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var newRoleNameResult = RoleName.Create("Editor");
        Assert.True(newRoleNameResult.IsSuccess);
        var newRole = EmployeeRole.Create(newRoleNameResult.Value!).Value!;

        // Act
        var result = await repository.AddAsync(newRole);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var addedRole = await context.EmployeeRoles.FirstOrDefaultAsync(r => r.Id == newRole.Id);
        Assert.NotNull(addedRole);
        Assert.Equal("Editor", addedRole.RoleName.Name);
    }

    [Fact]
    public async Task DeleteAsync_ExistingRole_ReturnsFailureResultAndRoleIsNotDeleted()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var existingRoleId = context.EmployeeRoles.First().Id;

        // Act
        var result = await repository.DeleteAsync(existingRoleId);
        await context.SaveChangesAsync();

        // Assert
        Assert.False(result.IsSuccess, $"Delete operation should return failure due to ExecuteDeleteAsync issue: {string.Join(", ", result.Errors.Select(e => e.Message))}");
        var notDeletedRole = await context.EmployeeRoles.FindAsync(existingRoleId);
        Assert.NotNull(notDeletedRole);
        Assert.Equal("Administrator", notDeletedRole.RoleName.Name); // Assuming the first role is "Administrator"
    }

    [Fact]
    public async Task RoleIdExists_ExistingId_ReturnsTrue()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var existingRoleId = context.EmployeeRoles.First().Id;

        // Act
        var exists = await repository.RoleIdExists(existingRoleId);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task RoleIdExists_NonExistingId_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var nonExistingRoleId = new EmployeeRoleId(Guid.NewGuid());

        // Act
        var exists = await repository.RoleIdExists(nonExistingRoleId);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ListAsync_WithSpecification_ReturnsSuccessResultWithAllRoles()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var specification = new AllEmployeeRolesSpecification();

        // Act
        var result = await repository.ListAsync(specification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(context.EmployeeRoles.Count(), result.Value.Count());
    }

    [Fact]
    public async Task CountAsync_WithSpecification_ReturnsSuccessResultWithCorrectCount()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var specification = new AllEmployeeRolesSpecification();

        // Act
        var result = await repository.CountAsync(specification);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(context.EmployeeRoles.Count(), result.Value);
    }
    [Fact]
    public async Task UpdateAsync_ExistingRole_SimulatesServiceDetachAndUpdate_Refined()
    {
        // Arrange
        using var context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        var existingRole = await context.EmployeeRoles.FirstAsync();
        var originalRoleId = existingRole.Id;
        var updatedRoleNameResult = RoleName.Create("Senior Manager");
        Assert.True(updatedRoleNameResult.IsSuccess);
        var updatedRoleName = updatedRoleNameResult.Value!;

        // Act
        await repository.DetachAsync(existingRole); // Simulate detaching

        var updatedRoleResult = existingRole.WithRoleName(updatedRoleName);
        Assert.True(updatedRoleResult.IsSuccess);
        var roleToUpdate = updatedRoleResult.Value!;

        var result = await repository.UpdateAsync(roleToUpdate);
        await context.SaveChangesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        var retrievedRole = await context.EmployeeRoles.FindAsync(originalRoleId);
        Assert.NotNull(retrievedRole);
        Assert.Equal("Senior Manager", retrievedRole.RoleName.Name);
    }
    [Fact]
    public async Task DetachAsync_AttachedRole_StateIsDetached()
    {
        // Arrange
        using ApplicationDbContext context = CreateContext();
        var repository = new EmployeeRoleRepository(context);
        EmployeeRole existingRole = await context.EmployeeRoles.FirstAsync();

        // Act
        _ = repository.DetachAsync(existingRole);

        // Assert
        Assert.Equal(EntityState.Detached, context.Entry(existingRole).State);
    }

}
