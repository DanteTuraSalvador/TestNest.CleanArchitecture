using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.Infrastructure.Persistence.Seeders.Employees;
using TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentAddresses;
using TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentContacts;
using TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentMembers;
using TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentPhones;
using TestNest.Admin.Infrastructure.Persistence.Seeders.Establishments;
using TestNest.Admin.Infrastructure.Persistence.Seeders.SocialMediaPlatforms;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders;

public static class DataSeeder
{
    public static void SeedEstablishments(
    ApplicationDbContext context,
    int numberOfEstablishments = 20)
    {
        if (!context.Establishments.Any())
        {
            var faker = new EstablishmentFaker();

            var establishments = faker.Generate(numberOfEstablishments * 4)
                .Where(e => e != null && !e.IsEmpty())
                .Take(numberOfEstablishments)
                .ToList();

            if (establishments.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Failed to generate any valid establishments after {numberOfEstablishments * 2} attempts"
                );
            }

            context.Set<Establishment>().AddRange(establishments);
            _ = context.SaveChanges();
            Console.WriteLine($"Successfully seeded {establishments.Count} valid establishments");
        }
    }

    public static void TestFakerSuccessRate()
    {
        var faker = new EstablishmentFaker();
        int sampleSize = 1000;

        List<Establishment> establishments = faker.Generate(sampleSize);
        int validCount = establishments.Count(e => e != null && !e.IsEmpty());

        Console.WriteLine($"\nFaker Success Rate Test:");
        Console.WriteLine($"Attempted: {sampleSize}");
        Console.WriteLine($"Valid: {validCount}");
        Console.WriteLine($"Success Rate: {validCount / (double)sampleSize:P}");
        Console.WriteLine($"Invalid Reasons:");
        establishments.Where(e => e == null || e.IsEmpty())
            .Take(10).ToList().ForEach(e => Console.WriteLine($"- {(e == null ? "NULL" : "Invalid")}"));
    }

    public static void SeedEmployeeRoles(
       ApplicationDbContext context,
       int numberOfRoles = 20)
    {
        if (!context.EmployeeRoles.Any())
        {
            var faker = new EmployeeRoleFaker();

            var roles = faker.Generate(numberOfRoles * 2)
                .Where(r => !r.IsEmpty())
                .GroupBy(r => r.RoleName.Name)
                .Select(g => g.First())
                .Take(numberOfRoles)
                .ToList();

            context.EmployeeRoles.AddRange(roles);
            _ = context.SaveChanges();
        }
    }

    public static void SeedEmployees(
        ApplicationDbContext context,
        int numberOfEmployees = 50)
    {
        if (!context.Employees.Any() &&
            context.Establishments.Any() &&
            context.EmployeeRoles.Any())
        {
            var faker = new EmployeeFaker(
                establishments: [.. context.Establishments],
                roles: [.. context.EmployeeRoles]
            );

            var employees = faker.Generate(numberOfEmployees)
                .Where(e => e != null && !e.IsEmpty())
                .ToList();

            context.Employees.AddRange(employees);
            _ = context.SaveChanges();
        }
    }

    public static void SeedSocialMediaPlatforms(
        ApplicationDbContext context,
        int numberOfPlatforms = 15)
    {
        if (!context.SocialMediaPlatforms.Any())
        {
            var faker = new SocialMediaPlatformFaker();

            var platforms = faker.Generate(numberOfPlatforms * 2)
                .Where(p => !p.IsEmpty())
                .GroupBy(p => p.SocialMediaName.Name)
                .Select(g => g.First())
                .Take(numberOfPlatforms)
                .ToList();

            context.SocialMediaPlatforms.AddRange(platforms);
            _ = context.SaveChanges();
        }
    }

    public static void SeedEstablishmentAddresses(
       ApplicationDbContext context,
       int addressesPerEstablishment = 3)
    {
        if (!context.EstablishmentAddresses.Any())
        {
            var establishments = context.Establishments.ToList();
            var faker = new EstablishmentAddressFaker(establishments);

            var allAddresses = new List<EstablishmentAddress>();

            foreach (Establishment? establishment in establishments)
            {
                int count = Random.Shared.Next(1, addressesPerEstablishment + 1);
                List<EstablishmentAddress> addresses = faker.Generate(count);

                if (addresses.Count != 0)
                {
                    addresses[0] = addresses[0].WithIsPrimary(true).Value!;
                }

                allAddresses.AddRange(addresses);
            }

            var validAddresses = allAddresses
                .Where(a => a != null && !a.IsEmpty() && a.Address != null && a.Address.IsEmpty())
                .ToList();

            context.EstablishmentAddresses.AddRange(validAddresses);
            _ = context.SaveChanges();
        }
    }

    public static void SeedEstablishmentContacts(
        ApplicationDbContext context,
        int contactsPerEstablishment = 2)
    {
        if (!context.EstablishmentContacts.Any())
        {
            var establishments = context.Establishments.ToList();
            var faker = new EstablishmentContactFaker(establishments);

            var allContacts = new List<EstablishmentContact>();

            foreach (Establishment? establishment in establishments)
            {
                var contacts = faker.Generate(contactsPerEstablishment)
                    .Where(c => !c.IsEmpty())
                    .ToList();

                if (contacts.Count != 0)
                {
                    contacts[0] = contacts[0].WithPrimaryFlag(true).Value!;
                }

                allContacts.AddRange(contacts);
            }

            context.EstablishmentContacts.AddRange(allContacts);
            _ = context.SaveChanges();
        }
    }

    public static void SeedEstablishmentPhones(
        ApplicationDbContext context,
        int phonesPerEstablishment = 2)
    {
        if (!context.EstablishmentPhones.Any())
        {
            var establishments = context.Establishments.ToList();
            var faker = new EstablishmentPhoneFaker(establishments);

            var allPhones = new List<EstablishmentPhone>();

            foreach (Establishment? establishment in establishments)
            {
                List<EstablishmentPhone> phones = faker.Generate(phonesPerEstablishment);

                if (phones.Count != 0)
                {
                    phones[0] = phones[0].WithIsPrimary(true).Value!;
                }

                allPhones.AddRange(phones);
            }

            context.EstablishmentPhones.AddRange(allPhones);
            _ = context.SaveChanges();
        }
    }

    public static void SeedEstablishmentMembers(
        ApplicationDbContext context,
        int membersPerEstablishment = 3)
    {
        if (!context.EstablishmentMembers.Any() &&
            context.Establishments.Any() &&
            context.Employees.Any())
        {
            var establishments = context.Establishments.ToList();
            var employees = context.Employees
                .Where(e => e.EstablishmentId != EstablishmentId.Empty())
                .ToList();

            var faker = new EstablishmentMemberFaker(establishments, employees);
            var allMembers = new List<EstablishmentMember>();

            foreach (Establishment? establishment in establishments)
            {
                if (!employees.Any(e => e.EstablishmentId == establishment.EstablishmentId))
                {
                    continue;
                }

                var members = faker.Generate(membersPerEstablishment)
                    .Where(m => !m.IsEmpty())
                    .ToList();

                allMembers.AddRange(members);
            }

            var uniqueMembers = allMembers
                .GroupBy(m => new { m.EstablishmentId, m.EmployeeId })
                .Select(g => g.First())
                .ToList();

            context.EstablishmentMembers.AddRange(uniqueMembers);
            _ = context.SaveChanges();
        }
    }
}
