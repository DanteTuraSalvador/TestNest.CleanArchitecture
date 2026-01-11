namespace TestNest.Admin.SharedLibrary.Exceptions;

public sealed class UserException : Exception
{
    public enum ErrorCode
    {
        NotFound,
        DuplicateEmail,
        InvalidCredentials,
        Validation,
        InvalidRefreshToken,
        UserInactive,
        PasswordRequired
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
    {
        { ErrorCode.NotFound, "User not found." },
        { ErrorCode.DuplicateEmail, "A user with this email already exists." },
        { ErrorCode.InvalidCredentials, "Invalid email or password." },
        { ErrorCode.Validation, "Validation error." },
        { ErrorCode.InvalidRefreshToken, "Invalid or expired refresh token." },
        { ErrorCode.UserInactive, "User account is inactive." },
        { ErrorCode.PasswordRequired, "Password hash is required." }
    };

    public ErrorCode Code { get; }
    public new string Message { get; }

    private UserException(ErrorCode code, string message) : base(message)
    {
        Code = code;
        Message = message;
    }

    public static UserException NotFound() => new(ErrorCode.NotFound, ErrorMessages[ErrorCode.NotFound]);
    public static UserException DuplicateEmail() => new(ErrorCode.DuplicateEmail, ErrorMessages[ErrorCode.DuplicateEmail]);
    public static UserException InvalidCredentials() => new(ErrorCode.InvalidCredentials, ErrorMessages[ErrorCode.InvalidCredentials]);
    public static UserException InvalidRefreshToken() => new(ErrorCode.InvalidRefreshToken, ErrorMessages[ErrorCode.InvalidRefreshToken]);
    public static UserException UserInactive() => new(ErrorCode.UserInactive, ErrorMessages[ErrorCode.UserInactive]);
    public static UserException PasswordRequired() => new(ErrorCode.PasswordRequired, ErrorMessages[ErrorCode.PasswordRequired]);
    public static UserException Validation(string message) => new(ErrorCode.Validation, message);
}
