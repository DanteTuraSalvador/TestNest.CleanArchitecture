using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Contracts.Interfaces.Persistence;

public interface IEstablishmentRepository : IGenericRepository<Establishment, EstablishmentId>
{
    Task<bool> ExistsAsync(EstablishmentId establishmentId);

    Task DetachAsync(Establishment establishment);

    Task<bool> EstablishmentIdExists(EstablishmentId establishmentId);

    Task<bool> ExistsWithNameAndEmailAsync(
        EstablishmentName establishmentName,
        EmailAddress emailAddress,
        EstablishmentId establishmentId);

    Task<Result<IEnumerable<Establishment>>> ListAsync(ISpecification<Establishment> spec);

    Task<Result<int>> CountAsync(ISpecification<Establishment> spec);

}
