using Microsoft.Extensions.Logging;
using Moq;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Interfaces;
using TestNest.Admin.Application.Services;
using TestNest.Admin.Application.Specifications.SoicalMediaPlatfomrSpecifications;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.SocialMediaPlatform;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using Xunit.Sdk;

namespace TestNest.Admin.Application.Tests.Services.SocialMediaPlatforms;
public class SocialMediaPlatformServiceTests
{
    private readonly Mock<ISocialMediaPlatformRepository> _mockSocialMediaRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IDatabaseExceptionHandlerFactory> _mockExceptionHandlerFactory;
    private readonly Mock<ILogger<SocialMediaPlatformService>> _mockLogger;
    private readonly SocialMediaPlatformService _service;

    public SocialMediaPlatformServiceTests()
    {
        _mockSocialMediaRepository = new Mock<ISocialMediaPlatformRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockExceptionHandlerFactory = new Mock<IDatabaseExceptionHandlerFactory>();
        _mockLogger = new Mock<ILogger<SocialMediaPlatformService>>();

        _service = new SocialMediaPlatformService(
            _mockSocialMediaRepository.Object,
            _mockUnitOfWork.Object,
            _mockExceptionHandlerFactory.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateSocialMediaPlatformAsync_InvalidName_ReturnsValidationError()
    {
        // Arrange
        var invalidRequest = new SocialMediaPlatformForCreationRequest
        {
            Name = "", // Invalid name
            PlatformURL = "invalid-url" // Invalid URL
        };

        // Act
        var result = await _service.CreateSocialMediaPlatformAsync(invalidRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
        Assert.NotEmpty(result.Errors);
        
        _mockSocialMediaRepository.Verify(repo => repo.AddAsync(It.IsAny<SocialMediaPlatform>()), Times.Never);
    }

    [Fact]
    public async Task CreateSocialMediaPlatformAsync_DuplicateName_ReturnsConflictError()
    {
        // Arrange
        var validRequest = new SocialMediaPlatformForCreationRequest
        {
            Name = "Twitter",
            PlatformURL = "https://twitter.com"
        };

        var existingPlatform = SocialMediaPlatform.Create(
            SocialMediaName.Create("Twitter", "https://twitter.com").Value!
        ).Value!;

        _mockSocialMediaRepository.Setup(repo => repo.GetSocialMediaPlatformByNameAsync("Twitter"))
            .ReturnsAsync(Result<SocialMediaPlatform>.Success(existingPlatform));

        // Act
        var result = await _service.CreateSocialMediaPlatformAsync(validRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.NotEmpty(result.Errors);
    }


    //[Fact]
    //public async Task CreateSocialMediaPlatformAsync_ValidRequest_ReturnsSuccess()
    //{
    //    // Arrange
    //    var validRequest = new SocialMediaPlatformForCreationRequest
    //    {
    //        Name = "Twitter",
    //        PlatformURL = "https://twitter.com"
    //    };

    //    // Mock repository to return "not found" for duplicate check
    //    _mockSocialMediaRepository.Setup(repo => repo.GetSocialMediaPlatformByNameAsync(It.IsAny<string>()))
    //        .ReturnsAsync(Result<SocialMediaPlatform>.Failure(ErrorType.NotFound, [new Error("NotFound", "Dummy error")]));

    //    // Mock successful creation
    //    _mockSocialMediaRepository.Setup(repo => repo.AddAsync(It.IsAny<SocialMediaPlatform>()))
    //        .ReturnsAsync(Result<SocialMediaPlatform>.Success(new SocialMediaPlatform()));

    //    // Act
    //    var result = await _service.CreateSocialMediaPlatformAsync(validRequest);

    //    // Assert
    //    Assert.True(result.IsSuccess); // Simplified assertion
    //}

    [Fact]
    public async Task CreateSocialMediaPlatformAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var validRequest = new SocialMediaPlatformForCreationRequest
        {
            Name = "Twitter",
            PlatformURL = "https://twitter.com"
        };

        // 1. Create a valid SocialMediaName
        var socialMediaName = SocialMediaName.Create("Twitter", "https://twitter.com").Value!;

        // 2. Create a valid SocialMediaPlatform using the factory method
        var newPlatform = SocialMediaPlatform.Create(socialMediaName).Value!;

        // Mock repository responses
        _mockSocialMediaRepository.Setup(repo => repo.GetSocialMediaPlatformByNameAsync("Twitter"))
            .ReturnsAsync(Result<SocialMediaPlatform>.Failure(ErrorType.NotFound, [new Error("NotFound", "Dummy error")]));

        _mockSocialMediaRepository.Setup(repo => repo.AddAsync(It.IsAny<SocialMediaPlatform>()))
            .ReturnsAsync(Result<SocialMediaPlatform>.Success(newPlatform));

        // Act
        var result = await _service.CreateSocialMediaPlatformAsync(validRequest);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CreateSocialMediaPlatformAsync_ValidRequest_ReturnsCreatedPlatform()
    {
        // Arrange
        var validRequest = new SocialMediaPlatformForCreationRequest
        {
            Name = "Twitter",
            PlatformURL = "https://twitter.com"
        };

        var newPlatform = SocialMediaPlatform.Create(
            SocialMediaName.Create("Twitter", "https://twitter.com").Value!
        ).Value!;

        // Add at least one error to the failure result
        _mockSocialMediaRepository.Setup(repo => repo.GetSocialMediaPlatformByNameAsync("Twitter"))
            .ReturnsAsync(Result<SocialMediaPlatform>.Failure(
                ErrorType.NotFound,
                [new Error("NotFound", "Dummy error")] // Add dummy error
            ));

        _mockSocialMediaRepository.Setup(repo => repo.AddAsync(It.IsAny<SocialMediaPlatform>()))
            .ReturnsAsync(Result<SocialMediaPlatform>.Success(newPlatform));

        // Act
        var result = await _service.CreateSocialMediaPlatformAsync(validRequest);

        // Assert
        Assert.True(result.IsSuccess);
    }



    [Fact]
    public async Task UpdateSocialMediaPlatformAsync_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = SocialMediaId.Create(Guid.NewGuid()).Value!;
        var request = new SocialMediaPlatformForUpdateRequest
        {
            Name = "Twitter",
            PlatformURL = "https://twitter.com"
        };

        // Add at least one error to the failure result
        _mockSocialMediaRepository.Setup(repo => repo.GetByIdAsync(invalidId))
            .ReturnsAsync(Result<SocialMediaPlatform>.Failure(
                ErrorType.NotFound,
                [new Error("NotFound", "Social media platform not found")] // Add error
            ));

        // Act
        var result = await _service.UpdateSocialMediaPlatformAsync(invalidId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.NotEmpty(result.Errors); // Verify errors are propagated
    }
    [Fact]
    public async Task UpdateSocialMediaPlatformAsync_InvalidName_ReturnsValidationError()
    {
        // Arrange
        var validId = SocialMediaId.Create(Guid.NewGuid()).Value!;
        var invalidRequest = new SocialMediaPlatformForUpdateRequest
        {
            Name = "", // Invalid
            PlatformURL = "invalid-url"
        };

        // Create a valid platform instance for mocking
        var existingPlatform = SocialMediaPlatform.Create(
            SocialMediaName.Create("ValidName", "https://valid.url").Value!
        ).Value!;

        _mockSocialMediaRepository.Setup(repo => repo.GetByIdAsync(validId))
            .ReturnsAsync(Result<SocialMediaPlatform>.Success(existingPlatform));

        // Act
        var result = await _service.UpdateSocialMediaPlatformAsync(validId, invalidRequest);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.ErrorType);
    }


    [Fact]
    public async Task DeleteSocialMediaPlatformAsync_ValidId_ReturnsSuccess()
    {
        // Arrange
        var validId = SocialMediaId.Create(Guid.NewGuid()).Value!;
        _mockSocialMediaRepository.Setup(repo => repo.DeleteAsync(validId))
            .ReturnsAsync(Result.Success());
        _mockUnitOfWork.Setup(uow => uow.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteSocialMediaPlatformAsync(validId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockSocialMediaRepository.Verify(repo => repo.DeleteAsync(validId), Times.Once);
    }

    [Fact]
    public async Task GetSocialMediaPlatformByIdAsync_ValidId_ReturnsPlatform()
    {
        // Arrange
        var validId = SocialMediaId.Create(Guid.NewGuid()).Value!;

        // Create a valid platform
        var socialMediaName = SocialMediaName.Create("Twitter", "https://twitter.com").Value!;
        var expectedPlatform = SocialMediaPlatform.Create(socialMediaName).Value!;

        _mockSocialMediaRepository.Setup(repo => repo.GetByIdAsync(validId))
            .ReturnsAsync(Result<SocialMediaPlatform>.Success(expectedPlatform));

        // Act
        var result = await _service.GetSocialMediaPlatformByIdAsync(validId);

        // Assert
        Assert.True(result.IsSuccess);

        // Verify the DTO mapping
        Assert.Equal(expectedPlatform.Id.Value.ToString(), result.Value!.Id); // ID format check
        Assert.Equal("Twitter", result.Value.Name);
        Assert.Equal("https://twitter.com", result.Value.PlatformURL);
    }

    //[Fact]
    //public async Task GetSocialMediaPlatformByIdAsync_ValidId_ReturnsPlatform()
    //{
    //    // Arrange
    //    var validId = SocialMediaId.Create(Guid.NewGuid()).Value!;

    //    // Create a valid platform with SocialMediaName
    //    var socialMediaName = SocialMediaName.Create("Twitter", "https://twitter.com").Value!;
    //    var expectedPlatform = SocialMediaPlatform.Create(socialMediaName).Value!;

    //    _mockSocialMediaRepository.Setup(repo => repo.GetByIdAsync(validId))
    //        .ReturnsAsync(Result<SocialMediaPlatform>.Success(expectedPlatform));

    //    // Act
    //    var result = await _service.GetSocialMediaPlatformByIdAsync(validId);

    //    // Assert
    //    Assert.True(result.IsSuccess);

    //    // Access properties from the response DTO, not the domain object
    //    Assert.Equal("Twitter", result.Value!.Name); // Assuming SocialMediaPlatformResponse has a 'Name' property
    //    Assert.Equal("https://twitter.com", result.Value!.PlatformURL); // And a 'PlatformURL' property
    //}

    [Fact]
    public async Task GetAllSocialMediaPlatformsAsync_ReturnsPlatforms()
    {
        // Arrange
        var spec = new SocialMediaPlatformSpecification();

        // Create valid platforms
        var platforms = new List<SocialMediaPlatform>
    {
        SocialMediaPlatform.Create(
            SocialMediaName.Create("Twitter", "https://twitter.com").Value!
        ).Value!,
        SocialMediaPlatform.Create(
            SocialMediaName.Create("Facebook", "https://facebook.com").Value!
        ).Value!
    };

        _mockSocialMediaRepository.Setup(repo => repo.ListAsync(spec))
            .ReturnsAsync(Result<IEnumerable<SocialMediaPlatform>>.Success(platforms));

        // Act
        var result = await _service.GetAllSocialMediaPlatformsAsync(spec);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count());
    }

}
