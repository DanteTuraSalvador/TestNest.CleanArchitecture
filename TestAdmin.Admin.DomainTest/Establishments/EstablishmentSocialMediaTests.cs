using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Establishments;
public class EstablishmentSocialMediaTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var socialMediaId = SocialMediaId.New();
        SocialMediaAccountName accountName = SocialMediaAccountName.Create("valid_account").Value!;

        // Act
        Result<EstablishmentSocialMedia> result = EstablishmentSocialMedia.Create(
            establishmentId,
            socialMediaId,
            accountName
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(EstablishmentSocialMediaId.Empty(), result.Value.EstablishmentSocialMediaId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(socialMediaId, result.Value.SocialMediaId);
        Assert.Equal(accountName, result.Value.SocialMediaAccountName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithSocialMediaId_ValidSocialMediaId_ReturnsNewEstablishmentSocialMedia()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var initialSocialMediaId = SocialMediaId.New();
        SocialMediaAccountName accountName = SocialMediaAccountName.Create("valid_account").Value!;

        Result<EstablishmentSocialMedia> establishmentSocialMediaResult = EstablishmentSocialMedia.Create(
            establishmentId,
            initialSocialMediaId,
            accountName
        );
        Assert.True(establishmentSocialMediaResult.IsSuccess);
        EstablishmentSocialMedia establishmentSocialMedia = establishmentSocialMediaResult.Value!;

        var newSocialMediaId = SocialMediaId.New();

        // Act
        Result<EstablishmentSocialMedia> result = establishmentSocialMedia.WithSocialMediaId(newSocialMediaId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentSocialMedia.EstablishmentSocialMediaId, result.Value.EstablishmentSocialMediaId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(newSocialMediaId, result.Value.SocialMediaId);
        Assert.Equal(accountName, result.Value.SocialMediaAccountName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithSocialMediaAccountName_ValidAccountName_ReturnsNewEstablishmentSocialMedia()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var socialMediaId = SocialMediaId.New();
        SocialMediaAccountName initialAccountName = SocialMediaAccountName.Create("initial_account").Value!;

        Result<EstablishmentSocialMedia> establishmentSocialMediaResult = EstablishmentSocialMedia.Create(
            establishmentId,
            socialMediaId,
            initialAccountName
        );
        Assert.True(establishmentSocialMediaResult.IsSuccess);
        EstablishmentSocialMedia establishmentSocialMedia = establishmentSocialMediaResult.Value!;
        SocialMediaAccountName newAccountName = SocialMediaAccountName.Create("new.account").Value!;

        // Act
        Result<EstablishmentSocialMedia> result = establishmentSocialMedia.WithSocialMediaAccountName(newAccountName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentSocialMedia.EstablishmentSocialMediaId, result.Value.EstablishmentSocialMediaId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(socialMediaId, result.Value.SocialMediaId);
        Assert.Equal(newAccountName, result.Value.SocialMediaAccountName);
        Assert.Empty(result.Errors);
    }


    [Fact]
    public void EstablishmentSocialMediaId_ReturnsId()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var socialMediaId = SocialMediaId.New();
        SocialMediaAccountName accountName = SocialMediaAccountName.Create("valid_account").Value!;

        EstablishmentSocialMedia establishmentSocialMedia = EstablishmentSocialMedia.Create(
            establishmentId,
            socialMediaId,
            accountName
        ).Value!;

        // Act
        EstablishmentSocialMediaId actualId = establishmentSocialMedia.EstablishmentSocialMediaId;

        // Assert
        Assert.Equal(establishmentSocialMedia.Id, actualId);
    }

    [Fact]
    public void Empty_ReturnsEmptyInstance()
    {
        // Act
        var emptySocialMedia = EstablishmentSocialMedia.Empty();

        // Assert
        Assert.NotNull(emptySocialMedia);
        Assert.True(emptySocialMedia.IsEmpty());
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var socialMediaId = SocialMediaId.New();
        SocialMediaAccountName accountName = SocialMediaAccountName.Create("valid_account").Value!;

        EstablishmentSocialMedia establishmentSocialMedia = EstablishmentSocialMedia.Create(
            establishmentId,
            socialMediaId,
            accountName
        ).Value!;

        // Act
        bool isEmpty = establishmentSocialMedia.IsEmpty();

        // Assert
        Assert.False(isEmpty);
    }

  
}

