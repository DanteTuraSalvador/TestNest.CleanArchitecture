using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Specifications.Common;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;

namespace TestNest.Admin.Application.Contracts.Interfaces.Persistence;

public interface IEmployeeRoleRepository : IGenericRepository<EmployeeRole, EmployeeRoleId>
{
    Task<Result<EmployeeRole>> GetEmployeeRoleByNameAsync(string roleName);

    Task DetachAsync(EmployeeRole employeeRole);

    Task<Result<IEnumerable<EmployeeRole>>> ListAsync(ISpecification<EmployeeRole> spec);

    Task<Result<int>> CountAsync(ISpecification<EmployeeRole> spec);

    Task<bool> RoleIdExists(EmployeeRoleId roleId);
}
