using Microsoft.Extensions.Logging;
using Moq;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Establishments;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestNest.Admin.Application.Tests.Services.Establishments;
public class EstablishmentServiceTests
{
    private readonly Mock<IEstablishmentRepository> _mockEstablishmentRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDatabaseExceptionHandlerFactory> _mockExceptionHandlerFactory;
    private readonly Mock<ILogger<EstablishmentService>> _mockLogger;
    private readonly EstablishmentService _establishmentService;

    public EstablishmentServiceTests()
    {
        _mockEstablishmentRepository = new Mock<IEstablishmentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExceptionHandlerFactory = new Mock<IDatabaseExceptionHandlerFactory>();
        _mockLogger = new Mock<ILogger<EstablishmentService>>();
        _establishmentService = new EstablishmentService(
            _mockEstablishmentRepository.Object,
            _mockUnitOfWork.Object,
            _mockExceptionHandlerFactory.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_ValidInput_ReturnsSuccessAndEstablishmentResponse()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "Test Establishment",
            EstablishmentEmail = "test@example.com"
        };

        EstablishmentName establishmentName = EstablishmentName.Create(creationRequest.EstablishmentName).Value!;
        EmailAddress establishmentEmail = EmailAddress.Create(creationRequest.EstablishmentEmail).Value!;
        Result<Establishment> newEstablishmentResult = Establishment.Create(establishmentName, establishmentEmail);
        Assert.True(newEstablishmentResult.IsSuccess);
        Establishment newEstablishment = newEstablishmentResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>())).ReturnsAsync(false);
        _ = _mockEstablishmentRepository.Setup(repo => repo.AddAsync(It.IsAny<Establishment>())).ReturnsAsync(Result<Establishment>.Success(newEstablishment));
        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _ = Assert.IsType<EstablishmentResponse>(result.Value);
        Assert.Equal(creationRequest.EstablishmentName, result.Value!.EstablishmentName);
        Assert.Equal(creationRequest.EstablishmentEmail, result.Value!.EstablishmentEmail);

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_InvalidEmptyName_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "", // Empty name
            EstablishmentEmail = "test@example.com"
        };

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == EstablishmentNameException.ErrorCode.EmptyName.ToString());

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(It.IsAny<EstablishmentName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId?>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_InvalidEmailFormat_ReturnsFailureWithValidationErrors()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "Test Establishment",
            EstablishmentEmail = "invalid-email" // Invalid email format
        };

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(It.IsAny<EstablishmentName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId?>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_DuplicateNameAndEmail_ReturnsFailureWithConflictError()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "Existing Establishment",
            EstablishmentEmail = "existing@example.com"
        };

        var establishmentName = EstablishmentName.Create(creationRequest.EstablishmentName).Value!;
        var establishmentEmail = EmailAddress.Create(creationRequest.EstablishmentEmail).Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>())).ReturnsAsync(true); // Simulate existing establishment

        // Act
        var result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == EstablishmentException.ErrorCode.DuplicateResource.ToString());

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_RepositoryAddFails_ReturnsFailure_WithCommitException()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "Test Establishment",
            EstablishmentEmail = "test@example.com"
        };

        EstablishmentName establishmentName = EstablishmentName.Create(creationRequest.EstablishmentName).Value!;
        EmailAddress establishmentEmail = EmailAddress.Create(creationRequest.EstablishmentEmail).Value!;
        Result<Establishment> newEstablishmentResult = Establishment.Create(establishmentName, establishmentEmail);
        Assert.True(newEstablishmentResult.IsSuccess);
        Establishment newEstablishment = newEstablishmentResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, 
                It.IsAny<EstablishmentId?>())).ReturnsAsync(false);
        _ = _mockEstablishmentRepository.Setup(repo => repo.AddAsync(
            It.IsAny<Establishment>()))
                .ReturnsAsync(Result<Establishment>.Failure(ErrorType.Database, new Error("DatabaseError", "Failed to add establishment.")));

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("Error saving changes."));

        _ = _mockExceptionHandlerFactory.Setup(factory => factory.HandleDbUpdateException(It.IsAny<DbUpdateException>()))
            .Returns(Result.Failure(ErrorType.Database, new Error("DbUpdateError", "Error saving changes.")));

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_SuccessfulAdd_FailedCommit_ReturnsFailure()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "Test Establishment",
            EstablishmentEmail = "test@example.com"
        };

        EstablishmentName establishmentName = EstablishmentName.Create(creationRequest.EstablishmentName).Value!;
        EmailAddress establishmentEmail = EmailAddress.Create(creationRequest.EstablishmentEmail).Value!;
        Result<Establishment> newEstablishmentResult = Establishment.Create(establishmentName, establishmentEmail);
        Assert.True(newEstablishmentResult.IsSuccess);
        Establishment newEstablishment = newEstablishmentResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>())).ReturnsAsync(false);
        _ = _mockEstablishmentRepository.Setup(repo => repo.AddAsync(It.IsAny<Establishment>())).ReturnsAsync(Result<Establishment>.Success(newEstablishment));

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("Error during commit."));

        _ = _mockExceptionHandlerFactory.Setup(factory => factory.HandleDbUpdateException(It.IsAny<DbUpdateException>()))
            .Returns(Result.Failure(ErrorType.Database, new Error("DbUpdateError", "Error during commit.")));

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == "DbUpdateError" && error.Message == "Error during commit.");

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(establishmentName, establishmentEmail, It.IsAny<EstablishmentId?>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateEstablishmentAsync_InvalidNameAndEmail_ReturnsFailureWithMultipleValidationErrors()
    {
        // Arrange
        var creationRequest = new EstablishmentForCreationRequest
        {
            EstablishmentName = "",
            EstablishmentEmail = "invalid-email"
        };

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.CreateEstablishmentAsync(creationRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);

        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(It.IsAny<EstablishmentName>(), It.IsAny<EmailAddress>(), It.IsAny<EstablishmentId?>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.AddAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }
    [Fact]
    public async Task GetEstablishmentByIdAsync_ExistingId_ReturnsSuccessWithEstablishmentResponse_Simplified()
    {
        // Arrange
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Establishment").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId)).ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.GetEstablishmentByIdAsync(establishmentId);

        // Assert
        Assert.True(result.IsSuccess);
        _ = Assert.IsType<EstablishmentResponse>(result.Value);
        Assert.NotNull(result.Value!.EstablishmentId);
        Assert.NotNull(result.Value!.EstablishmentName);
        Assert.NotNull(result.Value!.EstablishmentEmail);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentByIdAsync_NonExistingId_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        var nonExistingId = new EstablishmentId();
        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(nonExistingId)).ReturnsAsync(Result<Establishment>.Failure(ErrorType.NotFound, new Error("NotFound", "Establishment not found.")));

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.GetEstablishmentByIdAsync(nonExistingId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == "NotFound");

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentsAsync_NoEstablishments_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var emptyList = new List<Establishment>();
        _ = _mockEstablishmentRepository.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>())).ReturnsAsync(Result<IEnumerable<Establishment>>.Success(emptyList));

        // Act
        Result<IEnumerable<EstablishmentResponse>> result = await _establishmentService.GetEstablishmentsAsync(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);

        _mockEstablishmentRepository.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>()), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentsAsync_SomeEstablishments_ReturnsSuccessWithMappedList()
    {
        // Arrange
        Result<Establishment> establishment1Result = Establishment.Create(EstablishmentName.Create("Test 1").Value!, EmailAddress.Create("test1@example.com").Value!);
        Assert.True(establishment1Result.IsSuccess);
        Establishment establishment1 = establishment1Result.Value!;

        Result<Establishment> establishment2Result = Establishment.Create(EstablishmentName.Create("Test 2").Value!, EmailAddress.Create("test2@example.com").Value!);
        Assert.True(establishment2Result.IsSuccess);
        Establishment establishment2 = establishment2Result.Value!;

        var establishments = new List<Establishment> { establishment1, establishment2 };

        _ = _mockEstablishmentRepository.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>())).ReturnsAsync(Result<IEnumerable<Establishment>>.Success(establishments));

        // Act
        Result<IEnumerable<EstablishmentResponse>> result = await _establishmentService.GetEstablishmentsAsync(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
        Assert.Equal(establishments.Count, result.Value!.Count());
        Assert.Collection(result.Value!,
            response1 =>
            {
                Assert.Equal(establishment1.Id.Value.ToString(), response1.EstablishmentId);
                Assert.Equal(establishment1.EstablishmentName.Name, response1.EstablishmentName);
                Assert.Equal(establishment1.EstablishmentEmail.Email, response1.EstablishmentEmail);
            },
            response2 =>
            {
                Assert.Equal(establishment2.Id.Value.ToString(), response2.EstablishmentId);
                Assert.Equal(establishment2.EstablishmentName.Name, response2.EstablishmentName);
                Assert.Equal(establishment2.EstablishmentEmail.Email, response2.EstablishmentEmail);
            });

        _mockEstablishmentRepository.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>()), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentsAsync_RepositoryListFails_ReturnsFailure()
    {
        // Arrange
        var databaseError = new Error("DatabaseError", "Failed to retrieve establishments.");
        _ = _mockEstablishmentRepository.Setup(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>())).ReturnsAsync(Result<IEnumerable<Establishment>>.Failure(ErrorType.Database, new List<Error> { databaseError }));

        // Act
        Result<IEnumerable<EstablishmentResponse>> result = await _establishmentService.GetEstablishmentsAsync(null);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == databaseError.Code && error.Message == databaseError.Message);

        _mockEstablishmentRepository.Verify(repo => repo.ListAsync(It.IsAny<ISpecification<Establishment>>()), Times.Once);
    }


    [Fact]
    public async Task UpdateEstablishmentAsync_ValidInput_ReturnsSuccessWithUpdatedResponse()
    {
        // Arrange
        Result<Establishment> creationResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(creationResult.IsSuccess);
        Establishment existingEstablishment = creationResult.Value!;
        EstablishmentId establishmentId = existingEstablishment.Id;

        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "updated@example.com",
            EstablishmentStatusId = 2
        };

        Result<EstablishmentName> updatedNameResult = EstablishmentName.Create(updateRequest.EstablishmentName);
        Assert.True(updatedNameResult.IsSuccess);
        EstablishmentName updatedName = updatedNameResult.Value!;

        Result<EmailAddress> updatedEmailResult = EmailAddress.Create(updateRequest.EstablishmentEmail);
        Assert.True(updatedEmailResult.IsSuccess);
        EmailAddress updatedEmail = updatedEmailResult.Value!;

        Result<EstablishmentStatus> updatedStatusResult = EstablishmentStatus.FromId(updateRequest.EstablishmentStatusId);
        Assert.True(updatedStatusResult.IsSuccess);
        EstablishmentStatus updatedStatus = updatedStatusResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId)).ReturnsAsync(Result<Establishment>.Success(existingEstablishment));
        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId)).ReturnsAsync(false); 
        _ = _mockEstablishmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Establishment>())).ReturnsAsync(Result<Establishment>.Success(existingEstablishment));
        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(establishmentId, updateRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _ = Assert.IsType<EstablishmentResponse>(result.Value);
        Assert.Equal(establishmentId.Value.ToString(), result.Value!.EstablishmentId);
        Assert.Equal(updateRequest.EstablishmentName, result.Value!.EstablishmentName);
        Assert.Equal(updateRequest.EstablishmentEmail, result.Value!.EstablishmentEmail);
        Assert.Equal(updateRequest.EstablishmentStatusId, result.Value!.EstablishmentStatusId);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEstablishmentAsync_NonExistingId_ReturnsFailureWithNotFoundError()
    {
        // Arrange
        var nonExistingId = new EstablishmentId();
        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "updated@example.com",
            EstablishmentStatusId = 2
        };

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(nonExistingId)).ReturnsAsync(Result<Establishment>.Failure(ErrorType.NotFound, new Error("NotFound", "Establishment not found.")));

        // Act
        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(nonExistingId, updateRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Contains(result.Errors, error => error.Code == "NotFound");

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(nonExistingId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(It.IsAny<EstablishmentName>(), It.IsAny<EmailAddress>(), nonExistingId), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEstablishmentAsync_InvalidEmail_ReturnsFailureWithValidationErrors_Simplified()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "invalid-email",
            EstablishmentStatusId = 2
        };

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId)).ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(establishmentId, updateRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(It.IsAny<EstablishmentName>(), It.IsAny<EmailAddress>(), establishmentId), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEstablishmentAsync_DuplicateEmail_ReturnsFailureWithConflictError()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "duplicate@example.com",
            EstablishmentStatusId = 2
        };

        Result<EstablishmentName> updatedNameResult = EstablishmentName.Create(updateRequest.EstablishmentName);
        Assert.True(updatedNameResult.IsSuccess);
        EstablishmentName updatedName = updatedNameResult.Value!;

        Result<EmailAddress> updatedEmailResult = EmailAddress.Create(updateRequest.EstablishmentEmail);
        Assert.True(updatedEmailResult.IsSuccess);
        EmailAddress updatedEmail = updatedEmailResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId)).ReturnsAsync(Result<Establishment>.Success(existingEstablishment));
        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId)).ReturnsAsync(true);

        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(establishmentId, updateRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEstablishmentAsync_RepositoryUpdateFails_ReturnsFailure()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "updated@example.com",
            EstablishmentStatusId = 2
        };

        Result<EstablishmentName> updatedNameResult = EstablishmentName.Create(updateRequest.EstablishmentName);
        Assert.True(updatedNameResult.IsSuccess);
        EstablishmentName updatedName = updatedNameResult.Value!;

        Result<EmailAddress> updatedEmailResult = EmailAddress.Create(updateRequest.EstablishmentEmail);
        Assert.True(updatedEmailResult.IsSuccess);
        EmailAddress updatedEmail = updatedEmailResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));
        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId))
            .ReturnsAsync(false);
        _ = _mockEstablishmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Establishment>()))
            .ReturnsAsync(Result<Establishment>.Failure(ErrorType.Database, new Error("DatabaseError", "Failed to update establishment.")));

        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(establishmentId, updateRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Database, result.ErrorType);
        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateEstablishmentAsync_FailedCommit_ReturnsFailure_Simplified()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var updateRequest = new EstablishmentForUpdateRequest
        {
            EstablishmentName = "Updated Name",
            EstablishmentEmail = "updated@example.com",
            EstablishmentStatusId = 2
        };

        Result<EstablishmentName> updatedNameResult = EstablishmentName.Create(updateRequest.EstablishmentName);
        Assert.True(updatedNameResult.IsSuccess);
        EstablishmentName updatedName = updatedNameResult.Value!;

        Result<EmailAddress> updatedEmailResult = EmailAddress.Create(updateRequest.EstablishmentEmail);
        Assert.True(updatedEmailResult.IsSuccess);
        EmailAddress updatedEmail = updatedEmailResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));
        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId))
            .ReturnsAsync(false);
        _ = _mockEstablishmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Establishment>()))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ThrowsAsync(new DbUpdateException("Error saving to the database."));

        _ = _mockExceptionHandlerFactory.Setup(factory => factory.HandleDbUpdateException(It.IsAny<DbUpdateException>()))
            .Returns(Result.Failure(ErrorType.Database, new Error("DbUpdateError", "Error during commit.")));

        Result<EstablishmentResponse> result = await _establishmentService.UpdateEstablishmentAsync(establishmentId, updateRequest);

        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(updatedName, updatedEmail, establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task PatchEstablishmentAsync_SuccessfulPatch_ReturnsSuccessWithUpdatedData()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var patchRequest = new EstablishmentPatchRequest
        {
            EstablishmentName = "Updated Name",
            EmailAddress = "updated@example.com",
            EstablishmentStatus = 1
        };

        Result<EstablishmentName> updatedNameResult = EstablishmentName.Create("Updated Name");
        Assert.True(updatedNameResult.IsSuccess);
        EstablishmentName updatedName = updatedNameResult.Value!;

        Result<EmailAddress> updatedEmailResult = EmailAddress.Create("updated@example.com");
        Assert.True(updatedEmailResult.IsSuccess);
        EmailAddress updatedEmail = updatedEmailResult.Value!;

        Result<EstablishmentStatus> updatedStatusResult = EstablishmentStatus.FromId(1);
        Assert.True(updatedStatusResult.IsSuccess);
        EstablishmentStatus updatedStatus = updatedStatusResult.Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        _ = _mockEstablishmentRepository.Setup(repo => repo.DetachAsync(It.IsAny<Establishment>()))
            .Returns(Task.CompletedTask);

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(
            updatedName,
            updatedEmail,
            establishmentId)).ReturnsAsync(false);

        _ = _mockEstablishmentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Establishment>()))
            .ReturnsAsync(() => Result<Establishment>.Success(existingEstablishment
                .WithName(updatedName).Value!
                .WithEmail(updatedEmail).Value!
                .WithStatus(updatedStatus).Value!));

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _establishmentService.PatchEstablishmentAsync(establishmentId, patchRequest);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value.EstablishmentName);
        Assert.Equal("updated@example.com", result.Value.EstablishmentEmail);
        Assert.Equal(1, result.Value.EstablishmentStatusId);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.DetachAsync(It.IsAny<Establishment>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(
            updatedName,
            updatedEmail,
            establishmentId
        ), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task PatchEstablishmentAsync_DuplicateName_ReturnsConflictError()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var patchRequest = new EstablishmentPatchRequest
        {
            EstablishmentName = "Duplicate Name",
            EmailAddress = "existing@example.com",
            EstablishmentStatus = 1
        };

        Result<EstablishmentName> duplicateNameResult = EstablishmentName.Create("Duplicate Name");
        Assert.True(duplicateNameResult.IsSuccess);
        EstablishmentName duplicateName = duplicateNameResult.Value!;

        EmailAddress existingEmail = EmailAddress.Create("existing@example.com").Value!;

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        _ = _mockEstablishmentRepository.Setup(repo => repo.DetachAsync(It.IsAny<Establishment>()))
            .Returns(Task.CompletedTask);

        _ = _mockEstablishmentRepository.Setup(repo => repo.ExistsWithNameAndEmailAsync(
            duplicateName,
            existingEmail,
            establishmentId
        )).ReturnsAsync(true);

        Result<EstablishmentResponse> result = await _establishmentService.PatchEstablishmentAsync(establishmentId, patchRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Code == "DuplicateResource" || e.Code == "1");
        Assert.Null(result.Value);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.DetachAsync(It.IsAny<Establishment>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(
            duplicateName,
            existingEmail,
            establishmentId
        ), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }


    [Fact]
    public async Task PatchEstablishmentAsync_IdNotFound_ReturnsNotFoundError()
    {
        var nonExistentEstablishmentId = new EstablishmentId();
        var patchRequest = new EstablishmentPatchRequest
        {
            EstablishmentName = "Updated Name",
            EmailAddress = "updated@example.com",
            EstablishmentStatus = 1
        };
        var notFoundError = new Error("NotFound", "Establishment with this ID not found.");

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(nonExistentEstablishmentId))
            .ReturnsAsync(Result<Establishment>.Failure(ErrorType.NotFound, notFoundError));

        Result<EstablishmentResponse> result = await _establishmentService.PatchEstablishmentAsync(nonExistentEstablishmentId, patchRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Equal("NotFound", result.Errors[0].Code);
        Assert.Equal("Establishment with this ID not found.", result.Errors[0].Message);
        Assert.Null(result.Value);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(nonExistentEstablishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.DetachAsync(It.IsAny<Establishment>()), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(
            It.IsAny<EstablishmentName>(),
            It.IsAny<EmailAddress>(),
            nonExistentEstablishmentId
        ), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task PatchEstablishmentAsync_InvalidStatusTransition_ReturnsValidationError()
    {
        var establishmentId = new EstablishmentId();
        Result<Establishment> existingEstablishmentResult = Establishment.Create(
            EstablishmentName.Create("Existing Name").Value!,
            EmailAddress.Create("existing@example.com").Value!
        );
        Assert.True(existingEstablishmentResult.IsSuccess);
        Establishment existingEstablishment = existingEstablishmentResult.Value!;

        var patchRequest = new EstablishmentPatchRequest
        {
            EstablishmentStatus = 3
        };

        _ = _mockEstablishmentRepository.Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(existingEstablishment));

        _ = _mockEstablishmentRepository.Setup(repo => repo.DetachAsync(It.IsAny<Establishment>()))
            .Returns(Task.CompletedTask);

        Result<EstablishmentResponse> result = await _establishmentService.PatchEstablishmentAsync(establishmentId, patchRequest);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Null(result.Value);

        _mockEstablishmentRepository.Verify(repo => repo.GetByIdAsync(establishmentId), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.DetachAsync(It.IsAny<Establishment>()), Times.Once);
        _mockEstablishmentRepository.Verify(repo => repo.ExistsWithNameAndEmailAsync(
            It.IsAny<EstablishmentName>(),
            It.IsAny<EmailAddress>(),
            establishmentId
        ), Times.Never);
        _mockEstablishmentRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Establishment>()), Times.Never);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }


    [Fact]
    public async Task DeleteEstablishmentAsync_SuccessfulDeletion_ReturnsSuccess()
    {
        var establishmentIdToDelete = new EstablishmentId();

        _ = _mockEstablishmentRepository.Setup(repo => repo.DeleteAsync(establishmentIdToDelete))
            .ReturnsAsync(Result.Success());

        _ = _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        Result result = await _establishmentService.DeleteEstablishmentAsync(establishmentIdToDelete);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result);
        Assert.Empty(result.Errors);

        _mockEstablishmentRepository.Verify(repo => repo.DeleteAsync(establishmentIdToDelete), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteEstablishmentAsync_IdNotFound_ReturnsNotFoundError()
    {
        var nonExistentEstablishmentId = new EstablishmentId();
        var notFoundError = new Error("NotFound", "Establishment not found.");

        _ = _mockEstablishmentRepository.Setup(repo => repo.DeleteAsync(nonExistentEstablishmentId))
            .ReturnsAsync(Result.Failure(ErrorType.NotFound, notFoundError));

        Result result = await _establishmentService.DeleteEstablishmentAsync(nonExistentEstablishmentId);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotNull(result.Errors);
        Assert.NotEmpty(result.Errors);
        Assert.Equal("NotFound", result.Errors[0].Code);
        Assert.Equal("Establishment not found.", result.Errors[0].Message);

        _mockEstablishmentRepository.Verify(repo => repo.DeleteAsync(nonExistentEstablishmentId), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.SaveChangesAsync(), Times.Never);
    }
}
