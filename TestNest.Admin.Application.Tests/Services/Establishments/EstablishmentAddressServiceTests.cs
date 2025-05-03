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
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Tests.Services.Establishments;
public class EstablishmentAddressServiceTests
{
    private readonly Mock<IEstablishmentAddressRepository> _mockEstablishmentAddressRepository;
    private readonly Mock<IEstablishmentRepository> _mockEstablishmentRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDatabaseExceptionHandlerFactory> _mockExceptionHandlerFactory;
    private readonly Mock<ILogger<EstablishmentAddressService>> _mockLogger;
    private readonly EstablishmentAddressService _service;

    public EstablishmentAddressServiceTests()
    {
        _mockEstablishmentAddressRepository = new Mock<IEstablishmentAddressRepository>();
        _mockEstablishmentRepository = new Mock<IEstablishmentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExceptionHandlerFactory = new Mock<IDatabaseExceptionHandlerFactory>();
        _mockLogger = new Mock<ILogger<EstablishmentAddressService>>();

        _service = new EstablishmentAddressService(
            _mockEstablishmentAddressRepository.Object,
            _mockEstablishmentRepository.Object,
            _mockUnitOfWork.Object,
            _mockExceptionHandlerFactory.Object,
            _mockLogger.Object
        );
    }



    [Fact]
    public async Task CreateEstablishmentAddressAsync_InvalidData_ReturnsValidationError()
    {
        // Arrange
        var invalidRequest = new EstablishmentAddressForCreationRequest
        {
            EstablishmentId = "invalid-guid",
            AddressLine = "",
            Latitude = 200,
            Longitude = -200
        };

        // Act
        Result<EstablishmentAddressResponse> result = await _service.CreateEstablishmentAddressAsync(invalidRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CreateEstablishmentAddressAsync_DuplicateCoordinates_ReturnsConflict()
    {
        // Arrange
        var validRequest = new EstablishmentAddressForCreationRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "123 Main St",
            City = "New York",
            Latitude = 40.7128,
            Longitude = -74.0060,
            IsPrimary = true
        };

        _ = _mockEstablishmentAddressRepository
            .Setup(repo => repo.AddressExistsWithSameCoordinatesInEstablishment(
                It.IsAny<EstablishmentAddressId>(),
                40.7128m,
                -74.0060m,
                It.IsAny<EstablishmentId>()
            ))
            .ReturnsAsync(true);

        // Act
        Result<EstablishmentAddressResponse> result = await _service.CreateEstablishmentAddressAsync(validRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task CreateEstablishmentAddressAsync_NonexistentEstablishment_ReturnsNotFound()
    {
        // Arrange
        var validRequest = new EstablishmentAddressForCreationRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "123 Main St",
            City = "New York",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        _ = _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<EstablishmentId>()))
            .ReturnsAsync(Result<Establishment>.Failure(
                ErrorType.NotFound,
                [new Error("NotFound", $"EstablishmentAddress with ID '{It.IsAny<EstablishmentId>()}' not found.")]
            ));

        // Act
        Result<EstablishmentAddressResponse> result = await _service.CreateEstablishmentAddressAsync(validRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }

    [Fact]
    public async Task CreateEstablishmentAddressAsync_ValidRequest_ReturnsCreatedAddress()
    {
        // Arrange
        var validRequest = new EstablishmentAddressForCreationRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "123 Main St",
            Municipality = "Manhattan",
            City = "New York",
            Province = "NY",
            Region = "Northeast",
            Country = "USA",
            Latitude = 40.7128,
            Longitude = -74.0060,
            IsPrimary = false
        };

        EstablishmentId establishmentId = EstablishmentId.Create(Guid.Parse(validRequest.EstablishmentId)).Value!;

        Result<Address> addressResult = Address.Create(
            validRequest.AddressLine,
            validRequest.Municipality,
            validRequest.City,
            validRequest.Province,
            validRequest.Region,
            validRequest.Country,
            (decimal)validRequest.Latitude,
            (decimal)validRequest.Longitude
        );

        Assert.True(addressResult.IsSuccess, "Address validation failed");
        Address address = addressResult.Value!;

        EstablishmentName establishmentName = EstablishmentName.Create("Test Establishment").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@example.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, address, false);
        Assert.True(establishmentAddressResult.IsSuccess, "EstablishmentAddress creation failed");
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;

        _ = _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(establishment));

        _ = _mockEstablishmentAddressRepository
            .Setup(repo => repo.AddAsync(It.IsAny<EstablishmentAddress>()))
            .ReturnsAsync(Result<EstablishmentAddress>.Success(establishmentAddress));

        // Act
        Result<EstablishmentAddressResponse> result = await _service.CreateEstablishmentAddressAsync(validRequest);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(validRequest.AddressLine, result.Value!.AddressLine);
        _mockEstablishmentAddressRepository.Verify(
            repo => repo.AddAsync(It.IsAny<EstablishmentAddress>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateEstablishmentAddressAsync_SetPrimary_UpdatesExistingPrimary()
    {
        // Arrange
        var validRequest = new EstablishmentAddressForCreationRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "456 Oak Ave",
            Municipality = "Westside",
            City = "Los Angeles",
            Province = "CA",
            Region = "West",
            Country = "USA",
            Latitude = 34.0522,
            Longitude = -118.2437,
            IsPrimary = true
        };

        EstablishmentId establishmentId = EstablishmentId.Create(Guid.Parse(validRequest.EstablishmentId)).Value!;

        EstablishmentName establishmentName = EstablishmentName.Create("Test Establishment").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@example.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        _ = _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(establishment));

        _ = _mockEstablishmentAddressRepository
            .Setup(repo => repo.SetNonPrimaryForEstablishmentAsync(
                establishmentId,
                EstablishmentAddressId.Empty(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Success());

        // Act
        Result<EstablishmentAddressResponse> result = await _service.CreateEstablishmentAddressAsync(validRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEstablishmentAddressRepository.Verify(
            repo => repo.SetNonPrimaryForEstablishmentAsync(
                establishmentId,
                EstablishmentAddressId.Empty(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateEstablishmentAddressAsync_InvalidData_ReturnsValidationError()
    {
        // Arrange
        EstablishmentAddressId addressId = EstablishmentAddressId.Create(Guid.NewGuid()).Value!;
        var invalidRequest = new EstablishmentAddressForUpdateRequest
        {
            EstablishmentId = "invalid-guid",
            AddressLine = "",
            City = "",
            Municipality = "",
            Province = "",
            Region = "",
            Country = "",
            Latitude = 200,
            Longitude = -200
        };

        // Act
        Result<EstablishmentAddressResponse> result = await _service.UpdateEstablishmentAddressAsync(addressId, invalidRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
    }


    [Fact]
    public async Task UpdateEstablishmentAddressAsync_NonexistentAddress_ReturnsNotFound()
    {
        // Arrange
        EstablishmentAddressId addressId = EstablishmentAddressId.Create(Guid.NewGuid()).Value!;
        var validRequest = new EstablishmentAddressForUpdateRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "Valid Line",
            City = "City",
            Municipality = "Municipality",
            Province = "Province",
            Region = "Region",
            Country = "Country",
            Latitude = 40.7128,
            Longitude = -74.0060
        };

        _ = _mockEstablishmentAddressRepository
            .Setup(repo => repo.GetByIdAsync(addressId))
            .ReturnsAsync(Result<EstablishmentAddress>.Failure(ErrorType.NotFound, [new Error("NotFound", "Not found")]));

        // Act
        Result<EstablishmentAddressResponse> result = await _service.UpdateEstablishmentAddressAsync(addressId, validRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task UpdateEstablishmentAddressAsync_EstablishmentMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        Result<EstablishmentAddressId> addressIdResult = EstablishmentAddressId.Create(addressId);
        EstablishmentAddressId establishmentAddressId = addressIdResult.Value!;

        var existingEstablishmentId = Guid.NewGuid();
        EstablishmentAddress existingAddress = SetupExistingAddress(
            addressId: addressId,
            establishmentId: existingEstablishmentId
        );

        var request = new EstablishmentAddressForUpdateRequest
        {
            EstablishmentId = Guid.NewGuid().ToString(),
            AddressLine = "New Line",
            City = existingAddress.Address.City,
            Municipality = existingAddress.Address.Municipality,
            Province = existingAddress.Address.Province,
            Region = existingAddress.Address.Region,
            Country = existingAddress.Address.Country,
            Latitude = (double)existingAddress.Address.Latitude,
            Longitude = (double)existingAddress.Address.Longitude
        };

        // Act
        Result<EstablishmentAddressResponse> result = await _service.UpdateEstablishmentAddressAsync(
            establishmentAddressId,
            request
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    private EstablishmentAddress SetupExistingAddress(
        Guid addressId,
        Guid? establishmentId = null,
        bool isPrimary = false)
    {
        Guid estabId = establishmentId ?? Guid.NewGuid();
        EstablishmentId establishmentIdObj = EstablishmentId.Create(estabId).Value!;

        Address address = Address.Create(
            "Existing Address Line",
            "Existing City",
            "Existing Municipality",
            "Existing Province",
            "Existing Region",
            "Existing Country",
            40.7128m,
            -74.0060m
        ).Value!;

        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(
            establishmentIdObj,
            address,
            isPrimary
        );

        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;

        System.Reflection.PropertyInfo? idProperty = typeof(EstablishmentAddress).GetProperty("Id");
        idProperty!.SetValue(establishmentAddress, EstablishmentAddressId.Create(addressId).Value!);

        _ = _mockEstablishmentAddressRepository
            .Setup(repo => repo.GetByIdAsync(EstablishmentAddressId.Create(addressId).Value!))
            .ReturnsAsync(Result<EstablishmentAddress>.Success(establishmentAddress));

        return establishmentAddress;
    }

}

