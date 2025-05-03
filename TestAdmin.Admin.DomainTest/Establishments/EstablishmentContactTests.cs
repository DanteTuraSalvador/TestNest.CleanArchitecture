using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Establishments;

public class EstablishmentContactTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var establishmentId = EstablishmentId.New();
        Result<PersonName> personNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");

        Result<EstablishmentContact> result = EstablishmentContact.Create(establishmentId, personNameResult.Value!, phoneNumberResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(EstablishmentContactId.Empty(), result.Value.EstablishmentContactId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(personNameResult.Value!, result.Value.ContactPerson);
        Assert.Equal(phoneNumberResult.Value!, result.Value.ContactPhone);
        Assert.True(result.Value.IsPrimary); // Default is true
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithContactPerson_ValidPersonName_ReturnsNewEstablishmentContact()
    {
        var establishmentId = EstablishmentId.New();
        Result<PersonName> initialPersonNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        Result<EstablishmentContact> establishmentContactResult = EstablishmentContact.Create(establishmentId, initialPersonNameResult.Value!, phoneNumberResult.Value!);
        Assert.True(establishmentContactResult.IsSuccess);
        EstablishmentContact establishmentContact = establishmentContactResult.Value!;
        Result<PersonName> newPersonNameResult = PersonName.Create("Jane", "Middle", "Smith");

        Result<EstablishmentContact> result = establishmentContact.WithContactPerson(newPersonNameResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentContact.EstablishmentContactId, result.Value.EstablishmentContactId); // Same ID
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(newPersonNameResult.Value!, result.Value.ContactPerson);
        Assert.Equal(phoneNumberResult.Value!, result.Value.ContactPhone);
        Assert.Equal(establishmentContact.IsPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithContactPhone_ValidPhoneNumber_ReturnsNewEstablishmentContact()
    {
        var establishmentId = EstablishmentId.New();
        Result<PersonName> personNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> initialPhoneNumberResult = PhoneNumber.Create("1234567890");
        Result<EstablishmentContact> establishmentContactResult = EstablishmentContact.Create(establishmentId, personNameResult.Value!, initialPhoneNumberResult.Value!);
        Assert.True(establishmentContactResult.IsSuccess);
        EstablishmentContact establishmentContact = establishmentContactResult.Value!;
        Result<PhoneNumber> newPhoneNumberResult = PhoneNumber.Create("9876543210");

        Result<EstablishmentContact> result = establishmentContact.WithContactPhone(newPhoneNumberResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentContact.EstablishmentContactId, result.Value.EstablishmentContactId); 
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(personNameResult.Value!, result.Value.ContactPerson);
        Assert.Equal(newPhoneNumberResult.Value!, result.Value.ContactPhone);
        Assert.Equal(establishmentContact.IsPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithPrimaryFlag_SetsIsPrimary()
    {
        var establishmentId = EstablishmentId.New();
        Result<PersonName> personNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        Result<EstablishmentContact> establishmentContactResult = EstablishmentContact.Create(establishmentId, personNameResult.Value!, phoneNumberResult.Value!);
        Assert.True(establishmentContactResult.IsSuccess);
        EstablishmentContact establishmentContact = establishmentContactResult.Value!;
        const bool newIsPrimary = false;

        Result<EstablishmentContact> result = establishmentContact.WithPrimaryFlag(newIsPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentContact.EstablishmentContactId, result.Value.EstablishmentContactId);
        Assert.Equal(newIsPrimary, result.Value.IsPrimary);
        Assert.Equal(establishmentContact.ContactPerson, result.Value.ContactPerson);
        Assert.Equal(establishmentContact.ContactPhone, result.Value.ContactPhone);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void EstablishmentContactId_ReturnsId()
    {
        _ = EstablishmentContactId.New();
        var establishmentId = EstablishmentId.New();
        Result<PersonName> personNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        EstablishmentContact establishmentContact = EstablishmentContact.Create(establishmentId, personNameResult.Value!, phoneNumberResult.Value!).Value!;

        EstablishmentContactId actualId = establishmentContact.EstablishmentContactId;

        Assert.Equal(establishmentContact.Id, actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        var emptyContact = EstablishmentContact.Empty();

        bool isEmpty = emptyContact.IsEmpty();

        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        var establishmentId = EstablishmentId.New();
        Result<PersonName> personNameResult = PersonName.Create("John", "Jane", "Doe");
        Result<PhoneNumber> phoneNumberResult = PhoneNumber.Create("1234567890");
        EstablishmentContact establishmentContact = EstablishmentContact.Create(establishmentId, personNameResult.Value!, phoneNumberResult.Value!).Value!;

        bool isEmpty = establishmentContact.IsEmpty();

        Assert.False(isEmpty);
    }
}
