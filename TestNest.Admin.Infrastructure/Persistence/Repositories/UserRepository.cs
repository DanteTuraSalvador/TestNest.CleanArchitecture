using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Domain.Users;
using TestNest.Admin.Infrastructure.Persistence.Repositories.Common;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : GenericRepository<User, UserId>(dbContext), IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Result<User>> GetByEmailAsync(EmailAddress email)
    {
        try
        {
            User? user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.Email == email.Email);

            if (user == null)
            {
                var exception = UserException.NotFound();
                return Result<User>.Failure(
                    ErrorType.NotFound,
                    new Error(exception.Code.ToString(), exception.Message));
            }

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                ErrorType.Internal,
                new Error("DatabaseError", ex.Message));
        }
    }

    public async Task<Result<User>> GetByRefreshTokenAsync(string refreshToken)
    {
        try
        {
            User? user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null)
            {
                var exception = UserException.InvalidRefreshToken();
                return Result<User>.Failure(
                    ErrorType.NotFound,
                    new Error(exception.Code.ToString(), exception.Message));
            }

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                ErrorType.Internal,
                new Error("DatabaseError", ex.Message));
        }
    }

    public async Task<bool> EmailExistsAsync(EmailAddress email)
        => await _dbContext.Users.AnyAsync(u => u.Email.Email == email.Email);

    public override async Task<Result<User>> GetByIdAsync(UserId userId)
    {
        try
        {
            User? user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                var exception = UserException.NotFound();
                return Result<User>.Failure(
                    ErrorType.NotFound,
                    new Error(exception.Code.ToString(), exception.Message));
            }

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                ErrorType.Internal,
                new Error("DatabaseError", ex.Message));
        }
    }

    public override async Task<Result<User>> AddAsync(User user)
    {
        try
        {
            _ = await _dbContext.Users.AddAsync(user);
            return Result<User>.Success(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
        {
            var exception = UserException.DuplicateEmail();
            return Result<User>.Failure(
                ErrorType.Conflict,
                new Error(exception.Code.ToString(), exception.Message));
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                ErrorType.Internal,
                new Error("AddFailed", ex.Message));
        }
    }

    public override async Task<Result<User>> UpdateAsync(User user)
    {
        try
        {
            bool exists = await _dbContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == user.Id);

            if (!exists)
            {
                var exception = UserException.NotFound();
                return Result<User>.Failure(
                    ErrorType.NotFound,
                    new Error(exception.Code.ToString(), exception.Message));
            }

            _ = _dbContext.Update(user);
            return Result<User>.Success(user);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2601)
        {
            var exception = UserException.DuplicateEmail();
            return Result<User>.Failure(
                ErrorType.Conflict,
                new Error(exception.Code.ToString(), exception.Message));
        }
        catch (Exception ex)
        {
            return Result<User>.Failure(
                ErrorType.Internal,
                new Error("DatabaseError", ex.Message));
        }
    }

    public override async Task<Result> DeleteAsync(UserId userId)
    {
        try
        {
            User? user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return Result.Failure(
                    ErrorType.NotFound,
                    new Error(UserException.NotFound().Code.ToString(), UserException.NotFound().Message));
            }

            // Remove will trigger soft delete interceptor
            _ = _dbContext.Users.Remove(user);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                ErrorType.Internal,
                new Error("DatabaseError", ex.Message));
        }
    }
}
