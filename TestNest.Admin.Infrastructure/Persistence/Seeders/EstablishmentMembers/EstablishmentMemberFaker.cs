using Bogus;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.EstablishmentMembers;

public sealed class EstablishmentMemberFaker : Faker<EstablishmentMember>
{
    public EstablishmentMemberFaker(
        List<Establishment> establishments,
        List<Employee> employees)
    {
        var employeesByEstablishment = employees
            .GroupBy(e => e.EstablishmentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        _ = CustomInstantiator(f =>
        {
            var validEstablishments = establishments
                .Where(e => employeesByEstablishment.ContainsKey(e.EstablishmentId))
                .ToList();

            if (validEstablishments.Count == 0)
            {
                return EstablishmentMember.Empty();
            }

            Establishment establishment = f.PickRandom(validEstablishments);
            List<Employee> employeesInEstablishment = employeesByEstablishment[establishment.EstablishmentId];
            Employee employee = f.PickRandom(employeesInEstablishment);

            MemberTitle title;
            do
            {
                Result<MemberTitle> titleResult = MemberTitle.Create(f.Name.JobTitle());
                title = titleResult.IsSuccess ? titleResult.Value! : MemberTitle.Empty();
            } while (title.IsEmpty());

            MemberDescription description;
            do
            {
                Result<MemberDescription> descResult = MemberDescription.Create(f.Lorem.Sentence());
                description = descResult.IsSuccess ? descResult.Value! : MemberDescription.Empty();
            } while (description.IsEmpty());

            MemberTag tag;
            do
            {
                Result<MemberTag> tagResult = MemberTag.Create(f.Lorem.Word());
                tag = tagResult.IsSuccess ? tagResult.Value! : MemberTag.Empty();
            } while (tag.IsEmpty());

            return EstablishmentMember.Create(
                establishment.EstablishmentId,
                employee.Id,
                description,
                tag,
                title
            ).Value!;
        });
    }
}
