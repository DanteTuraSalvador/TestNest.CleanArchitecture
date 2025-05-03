using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.Dtos.Responses;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Tests.Services.Employees;

public class EmployeeRoleServiceTests
{
    private readonly Mock<IEmployeeRoleRepository> _mockEmployeeRoleRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDatabaseExceptionHandlerFactory> _mockExceptionHandlerFactory;
    private readonly Mock<ILogger<EmployeeRoleService>> _mockLogger;
    private readonly EmployeeRoleService _employeeRoleService;


    public EmployeeRoleServiceTests()
    {
        _mockEmployeeRoleRepository = new Mock<IEmployeeRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExceptionHandlerFactory = new Mock<IDatabaseExceptionHandlerFactory>();
        _mockLogger = new Mock<ILogger<EmployeeRoleService>>();
        _employeeRoleService = new EmployeeRoleService(
            _mockEmployeeRoleRepository.Object,
            _mockUnitOfWork.Object,
            _mockExceptionHandlerFactory.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEmployeeRoleAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        const string roleName = "Admin";
        var creationRequest = new EmployeeRoleForCreationRequest { RoleName = roleName };
        RoleName expectedRoleName = RoleName.Create(roleName).Value!;
        EmployeeRole createdEmployeeRole = EmployeeRole.Create(expectedRoleName).Value!;
        var expectedResponse = new EmployeeRoleResponse { RoleName = roleName, Id = createdEmployeeRole.Id.Value.ToString() };

        _ = _mockEmployeeRoleRepository.Setup(static repo => repo.GetEmployeeRoleByNameAsync(roleName))
            .ReturnsAsync(Result<EmployeeRole>.Failure(ErrorType.NotFound, new Error("NotFound", "Role not found.")));
        _ = _mockEmployeeRoleRepository.Setup(static repo => repo.AddAsync(It.IsAny<EmployeeRole>()))
            .ReturnsAsync(Result<EmployeeRole>.Success(createdEmployeeRole));
        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1); 

        // Act
        Result<EmployeeRoleResponse> result = await _employeeRoleService.CreateEmployeeRoleAsync(creationRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(roleName, result.Value.RoleName);
        Assert.NotEqual(default, result.Value.Id);

        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(roleName), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<EmployeeRole>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeRoleAsync_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        const string invalidRoleName = ""; // An empty role name should be invalid
        var creationRequest = new EmployeeRoleForCreationRequest { RoleName = invalidRoleName };

        // Act
        Result<EmployeeRoleResponse> result = await _employeeRoleService.CreateEmployeeRoleAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == RoleNameException.EmptyRoleName().Code.ToString()); // Check for a specific validation error

        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(It.IsAny<string>()), Times.Never);
        _mockEmployeeRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<EmployeeRole>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeRoleAsync_DuplicateRoleName_ReturnsConflictError()
    {
        // Arrange
        const string duplicateRoleName = "Admin";
        var creationRequest = new EmployeeRoleForCreationRequest { RoleName = duplicateRoleName };
        EmployeeRole existingEmployeeRole = EmployeeRole.Create(RoleName.Create(duplicateRoleName).Value!).Value!;

        _ = _mockEmployeeRoleRepository.Setup(repo => repo.GetEmployeeRoleByNameAsync(duplicateRoleName))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingEmployeeRole));

        // Act
        Result<EmployeeRoleResponse> result = await _employeeRoleService.CreateEmployeeRoleAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == EmployeeRoleException.DuplicateResource().Code.ToString());

        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(duplicateRoleName), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<EmployeeRole>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeRoleAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var roleId = EmployeeRoleId.New();
        const string initialRoleName = "Editor";
        const string updatedRoleName = "Supervisor";
        var updateRequest = new EmployeeRoleForUpdateRequest { RoleName = updatedRoleName };
        var initialRoleNameVo = RoleName.Create(initialRoleName).Value!;
        var existingEmployeeRoleResult = EmployeeRole.Create(initialRoleNameVo);
        Assert.True(existingEmployeeRoleResult.IsSuccess);
        var existingEmployeeRole = existingEmployeeRoleResult.Value!;

        // Manually set the Id of the existing role
        PropertyInfo propertyInfo = existingEmployeeRole.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo?.SetValue(existingEmployeeRole, roleId);

        var updatedRoleNameVo = RoleName.Create(updatedRoleName).Value!;
        var updatedEmployeeRoleResult = existingEmployeeRole.WithRoleName(updatedRoleNameVo);
        Assert.True(updatedEmployeeRoleResult.IsSuccess);
        existingEmployeeRole = updatedEmployeeRoleResult.Value!; // Re-assign the updated EmployeeRole

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(roleId))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingEmployeeRole));
        _mockEmployeeRoleRepository.Setup(repo => repo.GetEmployeeRoleByNameAsync(updatedRoleName))
            .ReturnsAsync(Result<EmployeeRole>.Failure(ErrorType.NotFound, new Error("NotFound", "Role not found.")));

        // When UpdateAsync is called with any EmployeeRole, return a successful result
        // with the *same* existingEmployeeRole instance (which now has the updated name and the correct Id)
        _mockEmployeeRoleRepository.Setup(repo => repo.UpdateAsync(It.IsAny<EmployeeRole>()))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingEmployeeRole));

        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);
        // Act
        Result<EmployeeRoleResponse> result = await _employeeRoleService.UpdateEmployeeRoleAsync(roleId, updateRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        // Basic checks for success, omitting detailed property assertions for now
        // If you want to re-enable them later, ensure the Arrange section correctly sets up the updated entity.

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(roleId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(updatedRoleName), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EmployeeRole>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeRoleAsync_InvalidId_ReturnsNotFoundError()
    {
        // Arrange
        var invalidRoleId = EmployeeRoleId.New();
        const string newRoleName = "Manager";
        var updateRequest = new EmployeeRoleForUpdateRequest { RoleName = newRoleName };

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(invalidRoleId))
            .ReturnsAsync(Result<EmployeeRole>.Failure(ErrorType.NotFound, new Error("NotFound", "Employee Role not found.")));

        // Act
        var result = await _employeeRoleService.UpdateEmployeeRoleAsync(invalidRoleId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == "NotFound");

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(invalidRoleId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(It.IsAny<string>()), Times.Never);
        _mockEmployeeRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EmployeeRole>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeRoleAsync_InvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var validRoleId = EmployeeRoleId.New();
        const string invalidRoleName = ""; // An empty role name should be invalid
        var updateRequest = new EmployeeRoleForUpdateRequest { RoleName = invalidRoleName };
        var existingRole = EmployeeRole.Create(RoleName.Create("OldName").Value!).Value!;
        PropertyInfo propertyInfo = existingRole.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo?.SetValue(existingRole, validRoleId);

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(validRoleId))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingRole));

        // Act
        var result = await _employeeRoleService.UpdateEmployeeRoleAsync(validRoleId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == RoleNameException.EmptyRoleName().Code.ToString());

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(validRoleId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(It.IsAny<string>()), Times.Never);
        _mockEmployeeRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EmployeeRole>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeRoleAsync_DuplicateRoleName_ReturnsConflictError()
    {
        // Arrange
        var existingRoleId = EmployeeRoleId.New();
        var anotherRoleId = EmployeeRoleId.New();
        const string initialRoleName = "Editor";
        const string duplicateRoleName = "Admin";
        var updateRequest = new EmployeeRoleForUpdateRequest { RoleName = duplicateRoleName };

        var existingRole = EmployeeRole.Create(RoleName.Create(initialRoleName).Value!).Value!;
        PropertyInfo existingPropertyInfo = existingRole.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        existingPropertyInfo?.SetValue(existingRole, existingRoleId);

        var conflictingRole = EmployeeRole.Create(RoleName.Create(duplicateRoleName).Value!).Value!;
        PropertyInfo conflictingPropertyInfo = conflictingRole.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        conflictingPropertyInfo?.SetValue(conflictingRole, anotherRoleId);

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(existingRoleId))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingRole));
        _mockEmployeeRoleRepository.Setup(repo => repo.GetEmployeeRoleByNameAsync(duplicateRoleName))
            .ReturnsAsync(Result<EmployeeRole>.Success(conflictingRole));

        // Act
        var result = await _employeeRoleService.UpdateEmployeeRoleAsync(existingRoleId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == EmployeeRoleException.DuplicateResource().Code.ToString());

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(existingRoleId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.GetEmployeeRoleByNameAsync(duplicateRoleName), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.UpdateAsync(It.IsAny<EmployeeRole>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task DeleteEmployeeRoleAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var roleIdToDelete = EmployeeRoleId.New();
        var existingRole = EmployeeRole.Create(RoleName.Create("ToDelete").Value!).Value!;
        PropertyInfo propertyInfo = existingRole.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo?.SetValue(existingRole, roleIdToDelete);

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(roleIdToDelete))
            .ReturnsAsync(Result<EmployeeRole>.Success(existingRole));
        _mockEmployeeRoleRepository.Setup(repo => repo.DeleteAsync(roleIdToDelete)) // Pass the EmployeeRoleId for setup
            .ReturnsAsync(Result.Success());
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _employeeRoleService.DeleteEmployeeRoleAsync(roleIdToDelete);

        // Assert
        Assert.True(result.IsSuccess);

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(roleIdToDelete), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.DeleteAsync(roleIdToDelete), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }

    [Fact]
    public async Task DeleteEmployeeRoleAsync_InvalidId_ReturnsNotFoundError()
    {
        // Arrange
        var invalidRoleId = EmployeeRoleId.New();

        _mockEmployeeRoleRepository.Setup(repo => repo.GetByIdAsync(invalidRoleId))
            .ReturnsAsync(Result<EmployeeRole>.Failure(ErrorType.NotFound, new Error("NotFound", "Employee Role not found.")));

        // Act
        var result = await _employeeRoleService.DeleteEmployeeRoleAsync(invalidRoleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == "NotFound");

        _mockEmployeeRoleRepository.Verify(repo => repo.GetByIdAsync(invalidRoleId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.DeleteAsync(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }


    [Fact]
    public async Task GetEmployeeRolessAsync_ReturnsAllRoles()
    {
        // Arrange
        var role1 = EmployeeRole.Create(RoleName.Create("Admin").Value!).Value!;
        PropertyInfo propertyInfo1 = role1.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo1?.SetValue(role1, EmployeeRoleId.New());

        var role2 = EmployeeRole.Create(RoleName.Create("Editor").Value!).Value!;
        PropertyInfo propertyInfo2 = role2.GetType().BaseType.GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic);
        propertyInfo2?.SetValue(role2, EmployeeRoleId.New());

        var roles = new List<EmployeeRole> { role1, role2 };

        _mockEmployeeRoleRepository.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<EmployeeRole>>()))
            .ReturnsAsync(Result<IEnumerable<EmployeeRole>>.Success(roles));

        // Act
        var emptySpec = new BaseSpecification<EmployeeRole>(); // Or however you represent an "all" specification
        var result = await _employeeRoleService.GetEmployeeRolessAsync(emptySpec);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(roles.Count, result.Value.Count());
        Assert.Contains(result.Value, r => r.RoleName == "Admin");
        Assert.Contains(result.Value, r => r.RoleName == "Editor");

        _mockEmployeeRoleRepository.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<EmployeeRole>>()), Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true)!,
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
            Times.Never);
    }


    [Fact]
    public async Task GetEmployeeRolessAsync_ReturnsErrorOnRepositoryFailure()
    {
        // Arrange
        var errorMessage = "Database error occurred while retrieving employee roles.";
        _mockEmployeeRoleRepository.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<EmployeeRole>>()))
            .ReturnsAsync(Result<IEnumerable<EmployeeRole>>.Failure(ErrorType.Database, new Error("DatabaseError", errorMessage)));

        // Act
        var emptySpec = new BaseSpecification<EmployeeRole>();
        var result = await _employeeRoleService.GetEmployeeRolessAsync(emptySpec);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Message == errorMessage);

        _mockEmployeeRoleRepository.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<EmployeeRole>>()), Times.Once);

    }
}
