using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestAdmin.Admin.DomainTest.Establishments;

public class EstablishmentAddressTests
{
    [Fact]
    public void Create_ValidInput_ReturnsSuccess()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);

        Result<EstablishmentAddress> result = EstablishmentAddress.Create(establishmentId, addressResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(EstablishmentAddressId.Empty(), result.Value.EstablishmentAddressId);
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(addressResult.Value!, result.Value.Address);
        Assert.True(result.Value.IsPrimary); // Default is true
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Create_EmptyEstablishmentId_ReturnsFailure()
    {
        var emptyEstablishmentId = EstablishmentId.Empty();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);

        Result<EstablishmentAddress> result = EstablishmentAddress.Create(emptyEstablishmentId, addressResult.Value!);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == nameof(StronglyTypedIdException.ErrorCode.NullId));
    }

    [Fact]
    public void Create_EmptyAddress_ReturnsFailure()
    {
        var establishmentId = EstablishmentId.New();
        var emptyAddress = Address.Empty();

        Result<EstablishmentAddress> result = EstablishmentAddress.Create(establishmentId, emptyAddress);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == nameof(AddressException));
    }

    [Fact]
    public void Create_ValidInputWithOptionalIsPrimaryFalse_ReturnsSuccessWithIsPrimaryFalse()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        const bool isPrimary = false;

        Result<EstablishmentAddress> result = EstablishmentAddress.Create(establishmentId, addressResult.Value!, isPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(isPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithAddress_ValidAddress_ReturnsNewEstablishmentAddress()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> initialAddressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, initialAddressResult.Value!);
        Assert.True(establishmentAddressResult.IsSuccess);
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        Result<Address> newAddressResult = Address.Create("New Line", "New City", "New Municipality", "New Province", "New Region", "New Country", 30.0m, 40.0m);

        Result<EstablishmentAddress> result = establishmentAddress.WithAddress(newAddressResult.Value!);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentAddress.EstablishmentAddressId, result.Value.EstablishmentAddressId); // Same instance ID
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(newAddressResult.Value!, result.Value.Address);
        Assert.Equal(establishmentAddress.IsPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithAddress_EmptyAddress_ReturnsFailure()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> initialAddressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, initialAddressResult.Value!);
        Assert.True(establishmentAddressResult.IsSuccess);
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        var emptyAddress = Address.Empty();

        Result<EstablishmentAddress> result = establishmentAddress.WithAddress(emptyAddress);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == nameof(AddressException));
    }

    [Fact]
    public void WithIsPrimary_SetsIsPrimary()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, addressResult.Value!);
        Assert.True(establishmentAddressResult.IsSuccess);
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        const bool newIsPrimary = false;

        Result<EstablishmentAddress> result = establishmentAddress.WithIsPrimary(newIsPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentAddress.EstablishmentAddressId, result.Value.EstablishmentAddressId); // Same instance
        Assert.Equal(newIsPrimary, result.Value.IsPrimary);
        Assert.Equal(establishmentAddress.Address, result.Value.Address);
        Assert.Equal(establishmentAddress.EstablishmentId, result.Value.EstablishmentId);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithDetails_ValidInput_ReturnsNewEstablishmentAddressWithSameId()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> initialAddressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, initialAddressResult.Value!);
        Assert.True(establishmentAddressResult.IsSuccess);
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        Result<Address> newAddressResult = Address.Create("New Line", "New City", "New Municipality", "New Province", "New Region", "New Country", 30.0m, 40.0m);
        const bool newIsPrimary = false;

        Result<EstablishmentAddress> result = establishmentAddress.WithDetails(newAddressResult.Value!, newIsPrimary);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(establishmentAddress.EstablishmentAddressId, result.Value.EstablishmentAddressId); // Same instance ID
        Assert.Equal(establishmentId, result.Value.EstablishmentId);
        Assert.Equal(newAddressResult.Value!, result.Value.Address);
        Assert.Equal(newIsPrimary, result.Value.IsPrimary);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void WithDetails_EmptyAddress_ReturnsFailure()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> initialAddressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        Result<EstablishmentAddress> establishmentAddressResult = EstablishmentAddress.Create(establishmentId, initialAddressResult.Value!);
        Assert.True(establishmentAddressResult.IsSuccess);
        EstablishmentAddress establishmentAddress = establishmentAddressResult.Value!;
        var emptyAddress = Address.Empty();
        const bool newIsPrimary = false;

        Result<EstablishmentAddress> result = establishmentAddress.WithDetails(emptyAddress, newIsPrimary);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, error => error.Code == nameof(AddressException));
    }

    [Fact]
    public void EstablishmentAddressId_ReturnsId()
    {
        _ = EstablishmentAddressId.New();
        var establishmentId = EstablishmentId.New();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        EstablishmentAddress establishmentAddress = EstablishmentAddress.Create(establishmentId, addressResult.Value!).Value!;

        EstablishmentAddressId actualId = establishmentAddress.EstablishmentAddressId;

        Assert.Equal(establishmentAddress.Id, actualId);
    }

    [Fact]
    public void IsEmpty_ReturnsTrueForEmptyInstance()
    {
        var emptyAddress = EstablishmentAddress.Empty();

        bool isEmpty = emptyAddress.IsEmpty();

        Assert.True(isEmpty);
    }

    [Fact]
    public void IsEmpty_ReturnsFalseForNonEmptyInstance()
    {
        var establishmentId = EstablishmentId.New();
        Result<Address> addressResult = Address.Create("Line 1", "City", "Municipality", "Province", "Region", "Country", 10.0m, 20.0m);
        EstablishmentAddress establishmentAddress = EstablishmentAddress.Create(establishmentId, addressResult.Value!).Value!;

        bool isEmpty = establishmentAddress.IsEmpty();

        Assert.False(isEmpty);
    }
}
