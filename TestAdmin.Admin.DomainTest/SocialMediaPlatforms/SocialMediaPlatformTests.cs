using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.SocialMediaPlatforms;

public class SocialMediaPlatformTests
{
    [Fact]
    public void Create_ValidSocialMediaName_ReturnsSuccess()
    {
        SocialMediaName socialMediaName = SocialMediaName.Create("YouTube", "https://www.youtube.com").Value!;

        Result<SocialMediaPlatform> result = SocialMediaPlatform.Create(socialMediaName);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotNull(result.Value.SocialMediaId);
        Assert.NotEqual(SocialMediaId.Empty(), result.Value.SocialMediaId);
        Assert.Equal(socialMediaName, result.Value.SocialMediaName);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Create_ReceivesInvalidSocialMediaNameResult_ReturnsFailure()
    {
        Result<SocialMediaName> invalidSocialMediaNameResult = SocialMediaName.Create("Inv@lid", "not a url");

        if (!invalidSocialMediaNameResult.IsSuccess)
        {
            var platformResult = Result<SocialMediaPlatform>.Failure(
                ErrorType.Validation,
                invalidSocialMediaNameResult.Errors
            );

            Assert.False(platformResult.IsSuccess);
            Assert.Null(platformResult.Value);
            Assert.NotEmpty(platformResult.Errors);

            string[] actualErrorCodes = [.. platformResult.Errors.Select(error => error.Code)];
            string[] expectedErrorCodes =
                [SocialMediaNameException.ErrorCode.InvalidCharacters.ToString(), SocialMediaNameException.ErrorCode.InvalidPlatformURLFormat.ToString()];

            Assert.Equal(expectedErrorCodes, actualErrorCodes);
        }
    }

    [Fact]
    public void WithSocialMediaName_ValidNewName_ReturnsNewSocialMediaPlatform()
    {
        SocialMediaName initialName = SocialMediaName.Create("OldName", "https://old.com").Value!;
        SocialMediaPlatform socialMediaPlatform = SocialMediaPlatform.Create(initialName).Value!;
        SocialMediaName newName = SocialMediaName.Create("NewName", "https://old.com").Value!;

        Result<SocialMediaPlatform> result = socialMediaPlatform.WithSocialMediaName(newName);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(socialMediaPlatform.SocialMediaId, result.Value.SocialMediaId);
        Assert.Equal(newName, result.Value.SocialMediaName);
        Assert.Empty(result.Errors);
        Assert.NotSame(socialMediaPlatform, result.Value);
    }

    [Fact]
    public void SocialMediaId_ReturnsId()
    {
        SocialMediaName socialMediaName = SocialMediaName.Create("Test", "https://test.com").Value!;
        Result<SocialMediaPlatform> platformResult = SocialMediaPlatform.Create(socialMediaName);

        SocialMediaId actualId = platformResult.Value!.SocialMediaId;

        Assert.NotEqual(SocialMediaId.Empty(), actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        var emptyPlatform = SocialMediaPlatform.Empty();

        bool isEmpty = emptyPlatform.IsEmpty();

        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        SocialMediaName socialMediaName = SocialMediaName.Create("Facebook", "https://www.facebook.com").Value!;
        SocialMediaPlatform socialMediaPlatform = SocialMediaPlatform.Create(socialMediaName).Value!;

        bool isEmpty = socialMediaPlatform.IsEmpty();

        Assert.False(isEmpty);
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        const string name = "Twitter";
        const string platformURL = "https://twitter.com/test";
        SocialMediaName socialMediaName = SocialMediaName.Create(name, platformURL).Value!;
        SocialMediaPlatform socialMediaPlatform = SocialMediaPlatform.Create(socialMediaName).Value!;
        string expectedString = $"{name} ({platformURL})";

        string toStringResult = socialMediaPlatform.ToString();

        Assert.Equal(expectedString, toStringResult);
    }
}
