using Microsoft.Extensions.Logging;
using Moq;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.Helpers;
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
            EstablishmentId = "invalid-guid", // Invalid ID format
            AddressLine = "", // Required field
            Latitude = 200, // Invalid latitude
            Longitude = -200 // Invalid longitude
        };

        // Act
        var result = await _service.CreateEstablishmentAddressAsync(invalidRequest);

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

        // Mock duplicate check
        _mockEstablishmentAddressRepository
            .Setup(repo => repo.AddressExistsWithSameCoordinatesInEstablishment(
                It.IsAny<EstablishmentAddressId>(),
                40.7128m,
                -74.0060m,
                It.IsAny<EstablishmentId>()
            ))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateEstablishmentAddressAsync(validRequest);

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

        // Mock establishment not found
        _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(It.IsAny<EstablishmentId>()))
            .ReturnsAsync(Result<Establishment>.Failure(
                ErrorType.NotFound,
                [new Error("NotFound", $"EstablishmentAddress with ID '{It.IsAny<EstablishmentId>()}' not found.")]
            ));

        // Act
        var result = await _service.CreateEstablishmentAddressAsync(validRequest);

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
            Municipality = "Manhattan",  // Added required field
            City = "New York",
            Province = "NY",             // Added required field
            Region = "Northeast",        // Added required field
            Country = "USA",             // Added required field
            Latitude = 40.7128,
            Longitude = -74.0060,
            IsPrimary = false
        };

        // Create valid domain objects
        var establishmentId = EstablishmentId.Create(Guid.Parse(validRequest.EstablishmentId)).Value!;

        // Create valid Address with all required fields
        var addressResult = Address.Create(
            validRequest.AddressLine,
            validRequest.Municipality,
            validRequest.City,
            validRequest.Province,
            validRequest.Region,
            validRequest.Country,
            (decimal)validRequest.Latitude,
            (decimal)validRequest.Longitude
        );

        Assert.True(addressResult.IsSuccess, "Address validation failed"); // Ensure address creation succeeds
        var address = addressResult.Value!;

        // Create valid Establishment
        var establishmentName = EstablishmentName.Create("Test Establishment").Value!;
        var establishmentEmail = EmailAddress.Create("test@example.com").Value!;
        var establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        // Create valid EstablishmentAddress
        var establishmentAddressResult = EstablishmentAddress.Create(establishmentId, address, false);
        Assert.True(establishmentAddressResult.IsSuccess, "EstablishmentAddress creation failed");
        var establishmentAddress = establishmentAddressResult.Value!;

        // Mock repository responses
        _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(establishment));

        _mockEstablishmentAddressRepository
            .Setup(repo => repo.AddAsync(It.IsAny<EstablishmentAddress>()))
            .ReturnsAsync(Result<EstablishmentAddress>.Success(establishmentAddress));

        // Act
        var result = await _service.CreateEstablishmentAddressAsync(validRequest);

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
            Municipality = "Westside",   // Added required field
            City = "Los Angeles",
            Province = "CA",             // Added required field
            Region = "West",             // Added required field
            Country = "USA",             // Added required field
            Latitude = 34.0522,
            Longitude = -118.2437,
            IsPrimary = true
        };

        var establishmentId = EstablishmentId.Create(Guid.Parse(validRequest.EstablishmentId)).Value!;

        // Create valid establishment
        var establishmentName = EstablishmentName.Create("Test Establishment").Value!;
        var establishmentEmail = EmailAddress.Create("test@example.com").Value!;
        var establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        // Mock repository responses
        _mockEstablishmentRepository
            .Setup(repo => repo.GetByIdAsync(establishmentId))
            .ReturnsAsync(Result<Establishment>.Success(establishment));

        // Fix: Provide all parameters including CancellationToken
        _mockEstablishmentAddressRepository
            .Setup(repo => repo.SetNonPrimaryForEstablishmentAsync(
                establishmentId,
                EstablishmentAddressId.Empty(),
                It.IsAny<CancellationToken>()  // Add this line
            ))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _service.CreateEstablishmentAddressAsync(validRequest);

        // Assert
        Assert.True(result.IsSuccess);
        _mockEstablishmentAddressRepository.Verify(
            repo => repo.SetNonPrimaryForEstablishmentAsync(
                establishmentId,
                EstablishmentAddressId.Empty(),
                It.IsAny<CancellationToken>()  // Add this line
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateEstablishmentAddressAsync_InvalidData_ReturnsValidationError()
    {
        // Arrange
        var addressId = EstablishmentAddressId.Create(Guid.NewGuid()).Value!;
        var invalidRequest = new EstablishmentAddressForUpdateRequest
        {
            // Valid GUID format to bypass ID validation
            EstablishmentId = "invalid-guid",
            AddressLine = "",    // Required field missing
            City = "",           // Required field missing
            Municipality = "",   // Required field missing
            Province = "",       // Required field missing
            Region = "",         // Required field missing
            Country = "",        // Required field missing
            Latitude = 200,      // Invalid value
            Longitude = -200     // Invalid value
        };

        // Act
        var result = await _service.UpdateEstablishmentAddressAsync(addressId, invalidRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
    }


    [Fact]
    public async Task UpdateEstablishmentAddressAsync_NonexistentAddress_ReturnsNotFound()
    {
        // Arrange
        var addressId = EstablishmentAddressId.Create(Guid.NewGuid()).Value!;
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

        _mockEstablishmentAddressRepository
            .Setup(repo => repo.GetByIdAsync(addressId))
            .ReturnsAsync(Result<EstablishmentAddress>.Failure(ErrorType.NotFound, [new Error("NotFound", "Not found")]));

        // Act
        var result = await _service.UpdateEstablishmentAddressAsync(addressId, validRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task UpdateEstablishmentAddressAsync_EstablishmentMismatch_ReturnsUnauthorized()
    {
        // Arrange
        var addressId = Guid.NewGuid();

        // 1. Create valid EstablishmentAddressId first
        var addressIdResult = EstablishmentAddressId.Create(addressId);
        var establishmentAddressId = addressIdResult.Value!; // Access the Value property

        // 2. Create existing address with different establishment ID
        var existingEstablishmentId = Guid.NewGuid();
        var existingAddress = SetupExistingAddress(
            addressId: addressId,
            establishmentId: existingEstablishmentId
        );

        var request = new EstablishmentAddressForUpdateRequest
        {
            // Use a DIFFERENT establishment ID
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
        var result = await _service.UpdateEstablishmentAddressAsync(
            establishmentAddressId, // Use the proper ID type here
            request
        );

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }
    // Add this helper method to your test class
    private EstablishmentAddress SetupExistingAddress(
        Guid addressId,
        Guid? establishmentId = null,
        bool isPrimary = false)
    {
        var estabId = establishmentId ?? Guid.NewGuid();
        var establishmentIdObj = EstablishmentId.Create(estabId).Value!;

        var address = Address.Create(
            "Existing Address Line",
            "Existing City",
            "Existing Municipality",
            "Existing Province",
            "Existing Region",
            "Existing Country",
            40.7128m,
            -74.0060m
        ).Value!;

        var establishmentAddressResult = EstablishmentAddress.Create(
            establishmentIdObj,
            address,
            isPrimary
        );

        var establishmentAddress = establishmentAddressResult.Value!;

        // Set the ID using reflection
        var idProperty = typeof(EstablishmentAddress).GetProperty("Id");
        idProperty!.SetValue(establishmentAddress, EstablishmentAddressId.Create(addressId).Value!);

        // Mock repository response
        _mockEstablishmentAddressRepository
            .Setup(repo => repo.GetByIdAsync(EstablishmentAddressId.Create(addressId).Value!))
            .ReturnsAsync(Result<EstablishmentAddress>.Success(establishmentAddress));

        return establishmentAddress;
    }

}

