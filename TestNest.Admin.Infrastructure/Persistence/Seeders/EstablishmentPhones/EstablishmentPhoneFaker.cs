using Bogus;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentPhones;

public sealed class EstablishmentPhoneFaker : Faker<EstablishmentPhone>
{
    public EstablishmentPhoneFaker(List<Establishment> establishments)
        => CustomInstantiator(f =>
            {
                Establishment establishment = f.PickRandom(establishments);
                PhoneNumber phone = GenerateValidPhoneNumber(f);
                bool isPrimary = !HasPrimaryPhone;

                return EstablishmentPhone.Create(
                    establishment.EstablishmentId,
                    phone,
                    isPrimary
                ).Value!;
            });

    private static PhoneNumber GenerateValidPhoneNumber(Faker f)
    {
        PhoneNumber phoneNumber;
        do
        {
            string phone = f.Phone.PhoneNumber("+#############")
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Trim();

            Result<PhoneNumber> phoneResult = PhoneNumber.Create(phone);
            if (phoneResult.IsSuccess)
            {
                phoneNumber = phoneResult.Value!;
                break;
            }
        } while (true);

        return phoneNumber;
    }

    private static bool HasPrimaryPhone => false;
}
