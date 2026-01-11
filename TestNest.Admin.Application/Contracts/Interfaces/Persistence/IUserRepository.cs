using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Domain.Users;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Contracts.Interfaces.Persistence;

public interface IUserRepository : IGenericRepository<User, UserId>
{
    Task<Result<User>> GetByEmailAsync(EmailAddress email);
    Task<Result<User>> GetByRefreshTokenAsync(string refreshToken);
    Task<bool> EmailExistsAsync(EmailAddress email);
}
