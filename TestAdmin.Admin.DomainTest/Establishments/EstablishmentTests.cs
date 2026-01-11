using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestAdmin.Admin.DomainTest.Establishments;

public class EstablishmentTests
{
    [Fact]
    public void WithStatus_ValidTransition_ReturnsNewEstablishment()
    {
        EstablishmentName establishmentName = EstablishmentName.Create("Test").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@test.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;
        EstablishmentStatus newStatus = EstablishmentStatus.Approval;

        Result<Establishment> result = establishment.WithStatus(newStatus);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishment.EstablishmentId, result.Value.EstablishmentId);
        Assert.Equal(newStatus, result.Value.EstablishmentStatus);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithStatus_InvalidTransition_ReturnsFailure()
    {
        EstablishmentName establishmentName = EstablishmentName.Create("Test").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@test.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        Result<Establishment> approvalEstablishmentResult = establishment.WithStatus(EstablishmentStatus.Approval);
        Assert.True(approvalEstablishmentResult.IsSuccess);
        Establishment approvalEstablishment = approvalEstablishmentResult.Value!;

        Result<Establishment> activeEstablishmentResult = approvalEstablishment.WithStatus(EstablishmentStatus.Active);
        Assert.True(activeEstablishmentResult.IsSuccess);
        Establishment activeEstablishment = activeEstablishmentResult.Value!;

        EstablishmentStatus newStatus = EstablishmentStatus.Pending;

        Result<Establishment> result = activeEstablishment.WithStatus(newStatus);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == nameof(EstablishmentStatusException.ErrorCode.InvalidStatusTransition));
    }

    [Fact]
    public void WithName_ValidName_ReturnsNewEstablishment()
    {
        EstablishmentName establishmentName = EstablishmentName.Create("Old Name").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@test.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;
        EstablishmentName newName = EstablishmentName.Create("New Name").Value!;

        Result<Establishment> result = establishment.WithName(newName);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishment.EstablishmentId, result.Value.EstablishmentId);
        Assert.Equal(newName, result.Value.EstablishmentName);
        Assert.Equal(establishment.EstablishmentEmail, result.Value.EstablishmentEmail);
        Assert.Equal(establishment.EstablishmentStatus, result.Value.EstablishmentStatus);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EstablishmentId_ReturnsId()
    {
        EstablishmentName establishmentName = EstablishmentName.Create("Test").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@test.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        EstablishmentId actualId = establishment.EstablishmentId;

        Assert.Equal(establishment.Id, actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        var emptyEstablishment = Establishment.Empty();

        bool isEmpty = emptyEstablishment.IsEmpty();

        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        EstablishmentName establishmentName = EstablishmentName.Create("Test").Value!;
        EmailAddress establishmentEmail = EmailAddress.Create("test@test.com").Value!;
        Establishment establishment = Establishment.Create(establishmentName, establishmentEmail).Value!;

        bool isEmpty = establishment.IsEmpty();

        Assert.False(isEmpty);
    }
}
