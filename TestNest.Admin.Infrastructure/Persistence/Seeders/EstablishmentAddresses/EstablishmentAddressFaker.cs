using Bogus;
using TestNest.Admin.Domain.Establishments;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentAddresses;

public sealed class EstablishmentAddressFaker : Faker<EstablishmentAddress>
{
    private const bool DefaultPrimaryAddressValue = false;

    public EstablishmentAddressFaker(List<Establishment> establishments)
    {
        var addressFaker = new AddressFaker();

        _ = CustomInstantiator(f =>
        {
            Establishment establishment = f.PickRandom(establishments);
            bool isPrimary = DefaultPrimaryAddressValue;

            return EstablishmentAddress.Create(
                establishmentId: establishment.EstablishmentId,
                address: addressFaker.Generate(),
                isPrimary: isPrimary
            ).Value!;
        });
    }
}
