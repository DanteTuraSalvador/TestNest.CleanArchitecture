using Bogus;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.Employees;

public sealed class EmployeeRoleFaker : Faker<EmployeeRole>
{
    public EmployeeRoleFaker()
    {
        _ = CustomInstantiator(f =>
        {
            for (int i = 0; i < 5; i++)
            {
                RoleName roleName = GenerateValidRoleName(f);
                Result<EmployeeRole> result = EmployeeRole.Create(roleName);

                if (result.IsSuccess)
                {
                    return result.Value!;
                }
            }
            return EmployeeRole.Empty();
        });

        _ = FinishWith((f, role) =>
        {
            if (role.IsEmpty())
            {
                Console.WriteLine("Failed to generate valid role");
            }
        });
    }

    private static RoleName GenerateValidRoleName(Faker f)
    {
        string baseRole = f.Name.JobTitle()
            .Replace("Engineer", "Specialist")
            .Replace("Executive", "Manager")
            .Replace("Technician", "Coordinator");

        string department = f.Commerce.Department(1);
        string level = f.PickRandom("Junior ", "Senior ", "Lead ", "Chief ", "");

        string fullRole = $"{level}{department} {baseRole}"
            .Trim()
            .Replace(",", "")
            .Replace(".", "")
            .Replace("  ", " ");

        fullRole = fullRole.Length > 100
            ? fullRole[..100]
            : fullRole;

        return RoleName.Create(fullRole).Value!;
    }
}
