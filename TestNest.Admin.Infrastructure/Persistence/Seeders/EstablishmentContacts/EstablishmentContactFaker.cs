using Bogus;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentContacts;

public sealed class EstablishmentContactFaker : Faker<EstablishmentContact>
{
    private static bool HasPrimaryContact => false;

    public EstablishmentContactFaker(List<Establishment> establishments)
        => CustomInstantiator(f =>
            {
                Establishment establishment = f.PickRandom(establishments);
                (PersonName personName, PhoneNumber phoneNumber) = GenerateValidContact(f);

                return EstablishmentContact.Create(establishment.EstablishmentId,
                    personName,
                    phoneNumber,
                    isPrimary: !HasPrimaryContact).Value!;
            });

    private static (PersonName Name, PhoneNumber Phone) GenerateValidContact(Faker f)
    {
        PersonName personName;
        do
        {
            string firstName = f.Name.FirstName().Replace("'", "");
            string lastName = f.Name.LastName().Replace("'", "");
            string middleName = f.Name.FirstName().Replace("'", "");

            Result<PersonName> nameResult = PersonName.Create(firstName, middleName, lastName);
            if (nameResult.IsSuccess)
            {
                personName = nameResult.Value!;
                break;
            }
        } while (true);

        PhoneNumber phoneNumber;
        do
        {
            string phone = f.Phone.PhoneNumber("+###########")
                        .Replace(" ", "")
                        .Replace("-", "");

            Result<PhoneNumber> phoneResult = PhoneNumber.Create(phone);
            if (phoneResult.IsSuccess)
            {
                phoneNumber = phoneResult.Value!;
                break;
            }
        } while (true);

        return (personName, phoneNumber);
    }
}
