using Bogus;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;
using TestNest.Admin.SharedLibrary.ValueObjects.Enums;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.Employees;

public sealed class EmployeeFaker : Faker<Employee>
{
    public EmployeeFaker(
        List<Establishment> establishments,
        List<EmployeeRole> roles) => CustomInstantiator(f =>
            {
                EmployeeNumber empNumber = GenerateValidEmployeeNumber(f);

                PersonName name = GenerateValidPersonName(f);

                EmailAddress email = GenerateValidEmail(f);

                Establishment establishment = f.PickRandom(establishments);
                EmployeeRole role = f.PickRandom(roles);

                Result<Employee> employeeResult = Employee.Create(
                    empNumber,
                    name,
                    email,
                    role.Id,
                    establishment.EstablishmentId
                );

                return employeeResult.IsSuccess
                    ? ApplyStatusTransition(employeeResult.Value!, f)
                    : null!;
            });

    private static EmployeeNumber GenerateValidEmployeeNumber(Faker f)
    {
        string number = f.Random.Replace("EMP-#####");
        return EmployeeNumber.Create(number).Value!;
    }

    private static PersonName GenerateValidPersonName(Faker f)
    {
        string firstName = f.Name.FirstName().Replace("'", "");
        string lastName = f.Name.LastName().Replace("'", "");
        return PersonName.Create(firstName, null, lastName).Value!;
    }

    private static EmailAddress GenerateValidEmail(Faker f)
    {
        string email = f.Internet.Email(
            firstName: f.Name.FirstName(),
            lastName: f.Name.LastName(),
            provider: "testnest.com"
        );
        return EmailAddress.Create(email).Value!;
    }

    private static Employee ApplyStatusTransition(Employee employee, Faker f)
    {
        EmployeeStatus[] items = [
            EmployeeStatus.Active,
            EmployeeStatus.Suspended,
            EmployeeStatus.Active
        ];
        EmployeeStatus status = f.PickRandom(items);

        return employee.WithEmployeeStatus(status).Value!;
    }
}
