using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Establishments;
public class EstablishmentPhoneTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");

        Result<EstablishmentPhone> result = EstablishmentPhone.Create(establishmentId, phoneNumberResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(EstablishmentPhoneId.Empty(), result.Value.EstablishmentPhoneId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(phoneNumberResult.Value!, result.Value.EstablishmentPhoneNumber);
        Assert.True(result.Value.IsPrimary); 
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Create_ValidInputWithOptionalIsPrimaryFalse_ReturnsSuccessWithIsPrimaryFalse()
    {
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        const bool isPrimary = false;

        Result<EstablishmentPhone> result = EstablishmentPhone.Create(establishmentId, phoneNumberResult.Value!, isPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(isPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithPhoneNumber_ValidPhoneNumber_ReturnsNewEstablishmentPhone()
    {
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> initialPhoneNumberResult = PhoneNumber.Create("1234567890");
        Result<EstablishmentPhone> establishmentPhoneResult = EstablishmentPhone.Create(establishmentId, initialPhoneNumberResult.Value!);
        Assert.True(establishmentPhoneResult.IsSuccess);
        EstablishmentPhone establishmentPhone = establishmentPhoneResult.Value!;
        Result<PhoneNumber> newPhoneNumberResult = PhoneNumber.Create("9876543210");

        Result<EstablishmentPhone> result = establishmentPhone.WithPhoneNumber(newPhoneNumberResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentPhone.EstablishmentPhoneId, result.Value.EstablishmentPhoneId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(newPhoneNumberResult.Value!, result.Value.EstablishmentPhoneNumber);
        Assert.Equal(establishmentPhone.IsPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithIsPrimary_SetsIsPrimary()
    {
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        Result<EstablishmentPhone> establishmentPhoneResult = EstablishmentPhone.Create(establishmentId, phoneNumberResult.Value!);
        Assert.True(establishmentPhoneResult.IsSuccess);
        EstablishmentPhone establishmentPhone = establishmentPhoneResult.Value!;
        const bool newIsPrimary = false;

        Result<EstablishmentPhone> result = establishmentPhone.WithIsPrimary(newIsPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentPhone.EstablishmentPhoneId, result.Value.EstablishmentPhoneId);
        Assert.Equal(newIsPrimary, result.Value.IsPrimary);
        Assert.Equal(establishmentPhone.EstablishmentPhoneNumber, result.Value.EstablishmentPhoneNumber);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EstablishmentPhoneId_ReturnsId()
    {
        _ = EstablishmentPhoneId.New();
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        EstablishmentPhone establishmentPhone = EstablishmentPhone.Create(establishmentId, phoneNumberResult.Value!).Value!;

        EstablishmentPhoneId actualId = establishmentPhone.EstablishmentPhoneId;

        Assert.Equal(establishmentPhone.Id, actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        var emptyPhone = EstablishmentPhone.Empty();

        bool isEmpty = emptyPhone.IsEmpty();

        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        var establishmentId = EstablishmentId.New();
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        EstablishmentPhone establishmentPhone = EstablishmentPhone.Create(establishmentId, phoneNumberResult.Value!).Value!;

        bool isEmpty = establishmentPhone.IsEmpty();

        Assert.False(isEmpty);
    }
}
