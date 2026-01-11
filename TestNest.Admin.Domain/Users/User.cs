using TestNest.Admin.SharedLibrary.Common.BaseEntity;
using TestNest.Admin.SharedLibrary.Common.Guards;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.StronglyTypeIds;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    public UserId UserId => Id;
    public EmailAddress Email { get; private set; }
    public string PasswordHash { get; private set; }
    public PersonName Name { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTime { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public EmployeeId? EmployeeId { get; private set; }
    public string? Role { get; private set; }

    public bool IsEmpty() => this == Empty();

    private static readonly Lazy<User> _empty = new(() => new User());

    public static User Empty() => _empty.Value;

    private User() : base(UserId.Empty())
        => (Email, PasswordHash, Name, IsActive, CreatedAt, Role)
            = (EmailAddress.Empty(), string.Empty, PersonName.Empty(), false, DateTime.MinValue, null);

    private User(
        UserId userId,
        EmailAddress email,
        string passwordHash,
        PersonName name,
        bool isActive,
        DateTime createdAt,
        EmployeeId? employeeId = null,
        string? role = null) : base(userId)
        => (Email, PasswordHash, Name, IsActive, CreatedAt, EmployeeId, Role)
            = (email, passwordHash, name, isActive, createdAt, employeeId, role);

    public static Result<User> Create(
        EmailAddress email,
        string passwordHash,
        PersonName name,
        EmployeeId? employeeId = null,
        string? role = null)
    {
        Result<EmailAddress> emailResult = EmailAddress.Create(email.Email);
        Result<PersonName> personNameResult = PersonName.Create(name.FirstName, name.MiddleName, name.LastName);

        var result = Result.Combine(
            emailResult.ToResult(),
            personNameResult.ToResult(),
            Guard.AgainstCondition(string.IsNullOrWhiteSpace(passwordHash), static () => UserException.PasswordRequired())
        );

        return result.IsSuccess
            ? Result<User>.Success(new User(
                UserId.New(),
                emailResult.Value!,
                passwordHash,
                personNameResult.Value!,
                true,
                DateTime.UtcNow,
                employeeId,
                role))
            : Result<User>.Failure(ErrorType.Validation, result.Errors);
    }

    public User WithRefreshToken(string refreshToken, DateTime expiryTime)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = expiryTime;
        return this;
    }

    public User ClearRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTime = null;
        return this;
    }

    public User RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        return this;
    }

    public Result<User> Deactivate()
    {
        if (!IsActive)
        {
            return Result<User>.Failure(
                ErrorType.Validation,
                new Error("UserAlreadyInactive", "User is already inactive"));
        }

        IsActive = false;
        return Result<User>.Success(this);
    }

    public Result<User> Activate()
    {
        if (IsActive)
        {
            return Result<User>.Failure(
                ErrorType.Validation,
                new Error("UserAlreadyActive", "User is already active"));
        }

        IsActive = true;
        return Result<User>.Success(this);
    }

    public bool HasValidRefreshToken(string refreshToken)
        => RefreshToken == refreshToken
           && RefreshTokenExpiryTime.HasValue
           && RefreshTokenExpiryTime.Value > DateTime.UtcNow;
}
