using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.Application.Specifications.EmployeeSpecifications;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.Dtos.Responses;

namespace TestNest.Admin.Application.Tests.Services.Employees;
public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _mockEmployeeRepository;
    private readonly Mock<IEmployeeRoleRepository> _mockEmployeeRoleRepository;
    private readonly Mock<IEstablishmentRepository> _mockEstablishmentRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<EmployeeService>> _mockLogger; 
    private readonly EmployeeService _employeeService;

    public EmployeeServiceTests()
    {
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _mockEmployeeRoleRepository = new Mock<IEmployeeRoleRepository>();
        _mockEstablishmentRepository = new Mock<IEstablishmentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<EmployeeService>>();
        _employeeService = new EmployeeService(
            _mockEmployeeRepository.Object,
            _mockEmployeeRoleRepository.Object,
            _mockEstablishmentRepository.Object,
            _mockUnitOfWork.Object,
            null!,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateEmployeeAsync_ValidInput_ReturnsSuccessAndEmployeeResponse()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        Result<EmployeeNumber> employeeNumberResult = EmployeeNumber.Create(creationRequest.EmployeeNumber);
        if (!employeeNumberResult.IsSuccess)
        {
            throw new Exception($"EmployeeNumber creation failed: {string.Join(", ", employeeNumberResult.Errors.Select(e => e.Message))}");
        }

        EmployeeNumber employeeNumber = employeeNumberResult.Value!;

        Result<PersonName> personNameResult = PersonName.Create(creationRequest.FirstName, creationRequest.MiddleName, creationRequest.LastName);
        if (!personNameResult.IsSuccess)
        {
            throw new Exception($"PersonName creation failed: {string.Join(", ", personNameResult.Errors.Select(e => e.Message))}");
        }

        PersonName personName = personNameResult.Value!;

        Result<EmailAddress> emailAddressResult = EmailAddress.Create(creationRequest.EmailAddress);
        if (!emailAddressResult.IsSuccess)
        {
            throw new Exception($"EmailAddress creation failed: {string.Join(", ", emailAddressResult.Errors.Select(e => e.Message))}");
        }

        EmailAddress emailAddress = emailAddressResult.Value!;

        Result<EmployeeRoleId> employeeRoleIdResult = EmployeeRoleId.Create(creationRequest.EmployeeRoleId);
        if (!employeeRoleIdResult.IsSuccess)
        {
            throw new Exception($"EmployeeRoleId creation failed: {string.Join(", ", employeeRoleIdResult.Errors.Select(e => e.Message))}");
        }

        EmployeeRoleId employeeRoleId = employeeRoleIdResult.Value!;

        Result<EstablishmentId> establishmentIdResult = EstablishmentId.Create(creationRequest.EstablishmentId);
        if (!establishmentIdResult.IsSuccess)
        {
            throw new Exception($"EstablishmentId creation failed: {string.Join(", ", establishmentIdResult.Errors.Select(e => e.Message))}");
        }

        EstablishmentId establishmentId = establishmentIdResult.Value!;

        Employee createdEmployee = Employee.Create(
            employeeNumber,
            personName,
            emailAddress,
            employeeRoleId,
            establishmentId
        ).Value!;

        _ = _mockEmployeeRepository.Setup(repo => repo.EmployeeExistsWithSameCombination(
                It.IsAny<EmployeeId>(),
                employeeNumber,
                personName,
                emailAddress,
                establishmentId))
            .ReturnsAsync(false);

        _ = _mockEmployeeRoleRepository.Setup(repo => repo.RoleIdExists(employeeRoleId))
            .ReturnsAsync(true);

        _ = _mockEstablishmentRepository.Setup(repo => repo.EstablishmentIdExists(establishmentId))
            .ReturnsAsync(true);

        _ = _mockEmployeeRepository.Setup(repo => repo.AddAsync(It.IsAny<Employee>()))
            .ReturnsAsync(createdEmployee);

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        IInvocationList invocations = _mockEmployeeRoleRepository.Invocations;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(creationRequest.EmployeeNumber, result.Value.EmployeeNumber);
        Assert.Equal(creationRequest.FirstName, result.Value.FirstName);
        Assert.Equal(creationRequest.MiddleName, result.Value.MiddleName);
        Assert.Equal(creationRequest.LastName, result.Value.LastName);
        Assert.Equal(creationRequest.EmailAddress, result.Value.Email);
        Assert.Equal(employeeRoleIdGuid.ToString(), result.Value.RoleId);
        Assert.Equal(establishmentIdGuid.ToString(), result.Value.EstablishmentId);

        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
                It.IsAny<EmployeeId>(),
                employeeNumber,
                personName,
                emailAddress,
                establishmentId), Times.Once);
        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(employeeRoleId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(establishmentId), Times.Once);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_NullEmployeeNumber_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = null!, 
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<SharedLibrary.Dtos.Responses.EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmptyLastName_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "", // Invalid: Empty LastName
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors); 

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_InvalidEmailAddress_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "invalid-email", 
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors); // Just assert that there is at least one error

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmptyFirstName_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmptyEmployeeNumber_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_InvalidEmployeeNumberFormat_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP#01",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmployeeNumberTooShort_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EM",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmployeeNumberTooLong_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP00123456789",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmptyEmployeeRoleId_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var establishmentIdGuid = Guid.NewGuid();
        Guid emptyRoleIdGuid = Guid.Empty;

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = emptyRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        // Act
        var result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidInput_EmptyEstablishmentId_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        Guid emptyEstablishmentIdGuid = Guid.Empty;

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = emptyEstablishmentIdGuid
        };

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRoleRepository.Verify(repo => repo.RoleIdExists(It.IsAny<EmployeeRoleId>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.EstablishmentIdExists(It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.EmployeeExistsWithSameCombination(
            It.IsAny<EmployeeId>(), It.IsAny<EmployeeNumber>(), It.IsAny<PersonName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_DuplicateEmployee_ReturnsFailureWithDuplicateError()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var establishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        EmployeeNumber employeeNumber = EmployeeNumber.Create(creationRequest.EmployeeNumber).Value!;
        PersonName personName = PersonName.Create(creationRequest.FirstName, creationRequest.MiddleName, creationRequest.LastName).Value!;
        EmailAddress emailAddress = EmailAddress.Create(creationRequest.EmailAddress).Value!;
        EmployeeRoleId employeeRoleId = EmployeeRoleId.Create(creationRequest.EmployeeRoleId).Value!;
        EstablishmentId establishmentId = EstablishmentId.Create(creationRequest.EstablishmentId).Value!;

        _ = _mockEmployeeRoleRepository.Setup(repo => repo.RoleIdExists(employeeRoleId))
            .ReturnsAsync(true);

        _ = _mockEstablishmentRepository.Setup(repo => repo.EstablishmentIdExists(establishmentId))
            .ReturnsAsync(true);

        _ = _mockEmployeeRepository.Setup(repo => repo.EmployeeExistsWithSameCombination(
                It.IsAny<EmployeeId>(),
                employeeNumber,
                personName,
                emailAddress,
                establishmentId))
            .ReturnsAsync(true);

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidForeignKey_InvalidEmployeeRoleId_ReturnsFailureWithNotFound()
    {
        // Arrange
        var establishmentIdGuid = Guid.NewGuid();
        var invalidRoleIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = invalidRoleIdGuid,
            EstablishmentId = establishmentIdGuid
        };

        EmployeeRoleId invalidEmployeeRoleId = EmployeeRoleId.Create(invalidRoleIdGuid).Value!;
        EstablishmentId establishmentId = EstablishmentId.Create(establishmentIdGuid).Value!;

        _ = _mockEmployeeRoleRepository.Setup(repo => repo.RoleIdExists(invalidEmployeeRoleId))
            .ReturnsAsync(false);

        _ = _mockEstablishmentRepository.Setup(repo => repo.EstablishmentIdExists(establishmentId))
            .ReturnsAsync(true);

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEmployeeAsync_InvalidForeignKey_InvalidEstablishmentId_ReturnsFailureWithNotFound()
    {
        // Arrange
        var employeeRoleIdGuid = Guid.NewGuid();
        var invalidEstablishmentIdGuid = Guid.NewGuid();

        var creationRequest = new EmployeeForCreationRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            MiddleName = "Michael",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com",
            EmployeeRoleId = employeeRoleIdGuid,
            EstablishmentId = invalidEstablishmentIdGuid
        };

        EmployeeNumber employeeNumber = EmployeeNumber.Create(creationRequest.EmployeeNumber).Value!;
        PersonName personName = PersonName.Create(creationRequest.FirstName, creationRequest.MiddleName, creationRequest.LastName).Value!;
        EmailAddress emailAddress = EmailAddress.Create(creationRequest.EmailAddress).Value!;
        EmployeeRoleId employeeRoleId = EmployeeRoleId.Create(creationRequest.EmployeeRoleId).Value!;
        EstablishmentId invalidEstablishmentId = EstablishmentId.Create(invalidEstablishmentIdGuid).Value!;

        _ = _mockEmployeeRoleRepository.Setup(repo => repo.RoleIdExists(employeeRoleId))
            .ReturnsAsync(true);

        _ = _mockEstablishmentRepository.Setup(repo => repo.EstablishmentIdExists(invalidEstablishmentId))
            .ReturnsAsync(false);

        // Act
        Result<EmployeeResponse> result = await _employeeService.CreateEmployeeAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.AddAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_EmployeeNotFound_ReturnsFailureWithNotFound()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP002",
            FirstName = "Jane",
            MiddleName = "Marie",
            LastName = "Doe",
            EmailAddress = "jane.doe@example.com",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Failure(ErrorType.NotFound, new Error(EmployeeException.NotFound().Code.ToString(), EmployeeException.NotFound().Message)));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.DetachAsync(It.IsAny<Employee>()), Times.Never);
        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidInput_NullFirstName_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        Employee existingEmployee = Employee.Create(
            EmployeeNumber.Create("EMP001").Value!,
            PersonName.Create("Old", null, "Name").Value!,
            EmailAddress.Create("old@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!;

        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP002",
            FirstName = null!,
            MiddleName = "Marie",
            LastName = "Doe",
            EmailAddress = "jane.doe@example.com",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Success(existingEmployee));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidInput_EmptyLastName_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        Employee existingEmployee = Employee.Create(
            EmployeeNumber.Create("EMP001").Value!,
            PersonName.Create("Old", null, "Name").Value!,
            EmailAddress.Create("old@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!;

        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP002",
            FirstName = "Jane",
            MiddleName = "Marie",
            LastName = "",
            EmailAddress = "jane.doe@example.com",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Success(existingEmployee));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidInput_InvalidEmailAddress_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        Employee existingEmployee = Employee.Create(
            EmployeeNumber.Create("EMP001").Value!,
            PersonName.Create("Old", null, "Name").Value!,
            EmailAddress.Create("old@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!;

        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP002",
            FirstName = "Jane",
            MiddleName = "Marie",
            LastName = "Doe",
            EmailAddress = "invalid-email",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Success(existingEmployee));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEmployeeAsync_InvalidInput_InvalidEmployeeNumberFormat_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        Employee existingEmployee = Employee.Create(
            EmployeeNumber.Create("EMP001").Value!,
            PersonName.Create("Old", null, "Name").Value!,
            EmailAddress.Create("old@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!;

        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP#02",
            FirstName = "Jane",
            MiddleName = "Marie",
            LastName = "Doe",
            EmailAddress = "jane.doe@example.com",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Success(existingEmployee));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task UpdateEmployeeAsync_DuplicateEmployeeOnUpdateRequest_ReturnsFailureWithConflictError()
    {
        // Arrange
        EmployeeId employeeIdToUpdate = EmployeeId.Create(Guid.NewGuid()).Value!;

        EmployeeRoleId validEmployeeRoleId = EmployeeRoleId.Create(Guid.NewGuid()).Value!;
        EstablishmentId validEstablishmentId = EstablishmentId.Create(Guid.NewGuid()).Value!;
        EmployeeNumber validEmployeeNumber = EmployeeNumber.Create("EMP-12345").Value!;
        PersonName validPersonName = PersonName.Create("John", null, "Doe").Value!;
        EmailAddress validEmail = EmailAddress.Create("john.doe@example.com").Value!;

        Employee existingEmployee = Employee.Create(
            validEmployeeNumber,
            validPersonName,
            validEmail,
            validEmployeeRoleId,
            validEstablishmentId
        ).Value!;

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeIdToUpdate))
            .ReturnsAsync(Result<Employee>.Success(existingEmployee));

        var updateRequest = new EmployeeForUpdateRequest
        {
            EmployeeNumber = "EMP-67890",
            FirstName = "Jane",
            MiddleName = null,
            LastName = "Smith",
            EmailAddress = "jane.smith@example.com",
            EmployeeRoleId = Guid.NewGuid(),
            EstablishmentId = Guid.NewGuid(),
            EmployeeStatusId = 1
        };

        _ = _mockEmployeeRepository.Setup(repo => repo.EmployeeExistsWithSameCombination(
                employeeIdToUpdate,
                EmployeeNumber.Create(updateRequest.EmployeeNumber).Value!,
                PersonName.Create(updateRequest.FirstName, updateRequest.MiddleName, updateRequest.LastName).Value!,
                EmailAddress.Create(updateRequest.EmailAddress).Value!,
                EstablishmentId.Create(updateRequest.EstablishmentId).Value!
            ))
            .ReturnsAsync(true);

        _ = _mockEmployeeRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Employee>()))
            .ReturnsAsync(Result<Employee>.Failure(ErrorType.Conflict, [new Error("Test", "Error")]));

        // Act
        Result<EmployeeResponse> result = await _employeeService.UpdateEmployeeAsync(employeeIdToUpdate, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEmployeeRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Employee>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_ValidId_ReturnsSuccessAndDeletes()
    {
        // Arrange
        EmployeeId employeeIdToDelete = EmployeeId.Create(Guid.NewGuid()).Value!;
        _ = _mockEmployeeRepository.Setup(repo => repo.DeleteAsync(employeeIdToDelete))
            .ReturnsAsync(Result.Success());
        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        Result result = await _employeeService.DeleteEmployeeAsync(employeeIdToDelete);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEmployeeRepository.Verify(repo => repo.DeleteAsync(employeeIdToDelete), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployeeAsync_RepositoryDeleteFails_ReturnsFailureAndDoesNotSaveChanges()
    {
        // Arrange
        EmployeeId employeeIdToDelete = EmployeeId.Create(Guid.NewGuid()).Value!;
        var repositoryError = Result.Failure(ErrorType.Database, [new Error("DB-001", "Database error during delete")]);
        _ = _mockEmployeeRepository.Setup(repo => repo.DeleteAsync(employeeIdToDelete))
            .ReturnsAsync(repositoryError);

        // Act
        Result result = await _employeeService.DeleteEmployeeAsync(employeeIdToDelete);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        _mockEmployeeRepository.Verify(repo => repo.DeleteAsync(employeeIdToDelete), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }


    [Fact]
    public async Task DeleteEmployeeAsync_SaveChangesFails_ReturnsFailure()
    {
        // Arrange
        EmployeeId employeeIdToDelete = EmployeeId.Create(Guid.NewGuid()).Value!;

        var mockEmployeeRepository = new Mock<IEmployeeRepository>();
        var mockEmployeeRoleRepository = new Mock<IEmployeeRoleRepository>();
        var mockEstablishmentRepository = new Mock<IEstablishmentRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<EmployeeService>>();
        var mockExceptionHandlerFactory = new Mock<IDatabaseExceptionHandlerFactory>();

        _ = mockExceptionHandlerFactory
            .Setup(f => f.HandleDbUpdateException(It.IsAny<DbUpdateException>()))
            .Returns(Result.Failure(ErrorType.Database, [new Error("DB_ERROR", "Database error")]));

        var employeeService = new EmployeeService(
            mockEmployeeRepository.Object,
            mockEmployeeRoleRepository.Object,
            mockEstablishmentRepository.Object,
            mockUnitOfWork.Object,
            mockExceptionHandlerFactory.Object,
            mockLogger.Object 
        );

        _ = mockEmployeeRepository.Setup(repo => repo.DeleteAsync(employeeIdToDelete))
            .ReturnsAsync(Result.Success());

        _ = mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("Simulated database failure"));

        // Act
        Result result = await employeeService.DeleteEmployeeAsync(employeeIdToDelete);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        mockEmployeeRepository.Verify(repo => repo.DeleteAsync(employeeIdToDelete), Times.Once);
        mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_EmployeeExists_ReturnsEmployeeResponse()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        Employee employee = Employee.Create(
            EmployeeNumber.Create("EMP-001").Value!,
            PersonName.Create("John", null, "Doe").Value!,
            EmailAddress.Create("john.doe@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!;

        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Success(employee));

        // Act
        Result<EmployeeResponse> result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(employee.EmployeeNumber.EmployeeNo, result.Value!.EmployeeNumber);
        _mockEmployeeRepository.Verify(repo => repo.GetByIdAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task GetEmployeeByIdAsync_EmployeeNotFound_ReturnsNotFoundError()
    {
        // Arrange
        EmployeeId employeeId = EmployeeId.Create(Guid.NewGuid()).Value!;
        _ = _mockEmployeeRepository.Setup(repo => repo.GetByIdAsync(employeeId))
            .ReturnsAsync(Result<Employee>.Failure(ErrorType.NotFound, [new Error("NotFound", "Employee not found")]));

        // Act
        Result<EmployeeResponse> result = await _employeeService.GetEmployeeByIdAsync(employeeId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        _mockEmployeeRepository.Verify(repo => repo.GetByIdAsync(employeeId), Times.Once);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_EmployeesExist_ReturnsEmployeeResponses()
    {
        // Arrange
        var spec = new EmployeeSpecification();
        var employees = new List<Employee>
    {
        Employee.Create(
            EmployeeNumber.Create("EMP-001").Value!,
            PersonName.Create("Alice", null, "Smith").Value!,
            EmailAddress.Create("alice@example.com").Value!,
            EmployeeRoleId.Create(Guid.NewGuid()).Value!,
            EstablishmentId.Create(Guid.NewGuid()).Value!
        ).Value!,
            Employee.Create(
                EmployeeNumber.Create("EMP-002").Value!,
                PersonName.Create("Bob", null, "Johnson").Value!,
                EmailAddress.Create("bob@example.com").Value!,
                EmployeeRoleId.Create(Guid.NewGuid()).Value!,
                EstablishmentId.Create(Guid.NewGuid()).Value!
                ).Value!};

        _ = _mockEmployeeRepository.Setup(repo => repo.ListAsync(spec))
            .ReturnsAsync(Result<IEnumerable<Employee>>.Success(employees));

        // Act
        Result<IEnumerable<EmployeeResponse>> result = await _employeeService.GetAllEmployeesAsync(spec);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count());
        _mockEmployeeRepository.Verify(repo => repo.ListAsync(spec), Times.Once);
    }

    [Fact]
    public async Task GetAllEmployeesAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var spec = new EmployeeSpecification();
        _ = _mockEmployeeRepository.Setup(repo => repo.ListAsync(spec))
            .ReturnsAsync(Result<IEnumerable<Employee>>.Failure(ErrorType.Database, [new Error("DB_ERROR", "Database error")]));

        // Act
        Result<IEnumerable<EmployeeResponse>> result = await _employeeService.GetAllEmployeesAsync(spec);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
    }

    [Fact]
    public async Task CountAsync_ValidSpecification_ReturnsCount()
    {
        // Arrange
        var spec = new EmployeeSpecification();
        const int expectedCount = 5;
        _ = _mockEmployeeRepository.Setup(repo => repo.CountAsync(spec))
            .ReturnsAsync(Result<int>.Success(expectedCount));

        // Act
        Result<int> result = await _employeeService.CountAsync(spec);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCount, result.Value);
    }

    [Fact]
    public async Task CountAsync_DatabaseError_ReturnsFailure()
    {
        // Arrange
        var spec = new EmployeeSpecification();
        _ = _mockEmployeeRepository.Setup(repo => repo.CountAsync(spec))
            .ReturnsAsync(Result<int>.Failure(ErrorType.Database, [new Error("DB_ERROR", "Database error")]));

        // Act
        Result<int> result = await _employeeService.CountAsync(spec);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
    }
}
