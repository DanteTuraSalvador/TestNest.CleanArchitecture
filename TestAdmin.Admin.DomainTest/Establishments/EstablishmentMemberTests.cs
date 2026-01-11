using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Establishments;

public class EstablishmentMemberTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> descriptionResult = MemberDescription.Create("Valid Description");
        Result<MemberTag> tagResult = MemberTag.Create("ValidTag");
        Result<MemberTitle> titleResult = MemberTitle.Create("Valid Title");

        // Act
        Result<EstablishmentMember> result = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            descriptionResult.Value!,
            tagResult.Value!,
            titleResult.Value!
        );

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(EstablishmentMemberId.Empty(), result.Value.EstablishmentMemberId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(employeeId, result.Value.EmployeeId);
        Assert.Equal(descriptionResult.Value!, result.Value.MemberDescription);
        Assert.Equal(tagResult.Value!, result.Value.MemberTag);
        Assert.Equal(titleResult.Value!, result.Value.MemberTitle);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithTitle_ValidTitle_ReturnsNewEstablishmentMember()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> descriptionResult = MemberDescription.Create("Valid Description");
        Result<MemberTag> tagResult = MemberTag.Create("ValidTag");
        Result<MemberTitle> initialTitleResult = MemberTitle.Create("Initial Title");

        Result<EstablishmentMember> establishmentMemberResult = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            descriptionResult.Value!,
            tagResult.Value!,
            initialTitleResult.Value!
        );
        Assert.True(establishmentMemberResult.IsSuccess);
        EstablishmentMember establishmentMember = establishmentMemberResult.Value!;

        Result<MemberTitle> newTitleResult = MemberTitle.Create("New Title");

        // Act
        Result<EstablishmentMember> result = establishmentMember.WithTitle(newTitleResult.Value!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentMember.EstablishmentMemberId, result.Value.EstablishmentMemberId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(employeeId, result.Value.EmployeeId);
        Assert.Equal(descriptionResult.Value!, result.Value.MemberDescription);
        Assert.Equal(tagResult.Value!, result.Value.MemberTag);
        Assert.Equal(newTitleResult.Value!, result.Value.MemberTitle);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithDescription_ValidDescription_ReturnsNewEstablishmentMember()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> initialDescriptionResult = MemberDescription.Create("Initial Description");
        Result<MemberTag> tagResult = MemberTag.Create("ValidTag");
        Result<MemberTitle> titleResult = MemberTitle.Create("Valid Title");

        Result<EstablishmentMember> establishmentMemberResult = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            initialDescriptionResult.Value!,
            tagResult.Value!,
            titleResult.Value!
        );
        Assert.True(establishmentMemberResult.IsSuccess);
        EstablishmentMember establishmentMember = establishmentMemberResult.Value!;

        Result<MemberDescription> newDescriptionResult = MemberDescription.Create("New Description");

        // Act
        Result<EstablishmentMember> result = establishmentMember.WithDescription(newDescriptionResult.Value!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentMember.EstablishmentMemberId, result.Value.EstablishmentMemberId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(employeeId, result.Value.EmployeeId);
        Assert.Equal(newDescriptionResult.Value!, result.Value.MemberDescription);
        Assert.Equal(tagResult.Value!, result.Value.MemberTag);
        Assert.Equal(titleResult.Value!, result.Value.MemberTitle);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithTag_ValidTag_ReturnsNewEstablishmentMember()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> descriptionResult = MemberDescription.Create("Valid Description");
        Result<MemberTag> initialTagResult = MemberTag.Create("InitialTag");
        Result<MemberTitle> titleResult = MemberTitle.Create("Valid Title");

        Result<EstablishmentMember> establishmentMemberResult = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            descriptionResult.Value!,
            initialTagResult.Value!,
            titleResult.Value!
        );
        Assert.True(establishmentMemberResult.IsSuccess);
        EstablishmentMember establishmentMember = establishmentMemberResult.Value!;

        Result<MemberTag> newTagResult = MemberTag.Create("NewTag");

        // Act
        Result<EstablishmentMember> result = establishmentMember.WithTag(newTagResult.Value!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentMember.EstablishmentMemberId, result.Value.EstablishmentMemberId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(employeeId, result.Value.EmployeeId);
        Assert.Equal(descriptionResult.Value!, result.Value.MemberDescription);
        Assert.Equal(newTagResult.Value!, result.Value.MemberTag);
        Assert.Equal(titleResult.Value!, result.Value.MemberTitle);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EstablishmentMemberId_ReturnsId()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> descriptionResult = MemberDescription.Create("Valid Description");
        Result<MemberTag> tagResult = MemberTag.Create("ValidTag");
        Result<MemberTitle> titleResult = MemberTitle.Create("Valid Title");

        EstablishmentMember establishmentMember = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            descriptionResult.Value!,
            tagResult.Value!,
            titleResult.Value!
        ).Value!;

        // Act
        EstablishmentMemberId actualId = establishmentMember.EstablishmentMemberId;

        // Assert
        Assert.Equal(establishmentMember.Id, actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        // Arrange
        var emptyMember = EstablishmentMember.Empty();

        // Act
        bool isEmpty = emptyMember.IsEmpty();

        // Assert
        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        // Arrange
        var establishmentId = EstablishmentId.New();
        var employeeId = EmployeeId.New();
        Result<MemberDescription> descriptionResult = MemberDescription.Create("Valid Description");
        Result<MemberTag> tagResult = MemberTag.Create("ValidTag");
        Result<MemberTitle> titleResult = MemberTitle.Create("Valid Title");

        EstablishmentMember establishmentMember = EstablishmentMember.Create(
            establishmentId,
            employeeId,
            descriptionResult.Value!,
            tagResult.Value!,
            titleResult.Value!
        ).Value!;

        // Act
        bool isEmpty = establishmentMember.IsEmpty();

        // Assert
        Assert.False(isEmpty);
    }
}
