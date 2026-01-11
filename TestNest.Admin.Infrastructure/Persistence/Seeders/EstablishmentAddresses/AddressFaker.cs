using Bogus;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentAddresses;

public sealed class AddressFaker : Faker<Address>
{
    public AddressFaker() => CustomInstantiator(f =>
    {
        (decimal lat, decimal lng) = GenerateValidCoordinates(f);

        return Address.Create(
            addressLine: f.Address.StreetAddress(),
            city: f.Address.City(),
            municipality: f.Address.County(),
            province: f.Address.State(),
            region: f.Address.StateAbbr(),
            country: f.Address.CountryCode(),
            latitude: lat,
            longitude: lng
        ).Value!;
    });

    private static (decimal Latitude, decimal Longitude) GenerateValidCoordinates(Faker f) => (
            (decimal)f.Address.Latitude(-90, 90),
            (decimal)f.Address.Longitude(-180, 180));
}
