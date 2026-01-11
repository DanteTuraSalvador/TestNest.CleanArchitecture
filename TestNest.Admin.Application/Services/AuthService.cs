using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Application.Contracts.Interfaces.Service;
using TestNest.Admin.Domain.Users;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Configuration;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Auth;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Auth;
using TestNest.Admin.SharedLibrary.Exceptions;
using TestNest.Admin.SharedLibrary.Exceptions.Common;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
    }

    public async Task<Result<TokenResponse>> LoginAsync(LoginRequest request)
    {
        var emailResult = EmailAddress.Create(request.Email);
        if (!emailResult.IsSuccess)
        {
            return Result<TokenResponse>.Failure(ErrorType.Validation, emailResult.Errors);
        }

        var userResult = await _userRepository.GetByEmailAsync(emailResult.Value!);
        if (!userResult.IsSuccess)
        {
            var exception = UserException.InvalidCredentials();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        var user = userResult.Value!;

        if (!user.IsActive)
        {
            var exception = UserException.UserInactive();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            var exception = UserException.InvalidCredentials();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        user.WithRefreshToken(refreshToken, refreshTokenExpiry);
        user.RecordLogin();

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Email} logged in successfully", user.Email.Email);

        return Result<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            TokenType = "Bearer"
        });
    }

    public async Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userResult = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (!userResult.IsSuccess)
        {
            var exception = UserException.InvalidRefreshToken();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        var user = userResult.Value!;

        if (!user.HasValidRefreshToken(request.RefreshToken))
        {
            var exception = UserException.InvalidRefreshToken();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        if (!user.IsActive)
        {
            var exception = UserException.UserInactive();
            return Result<TokenResponse>.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays);

        user.WithRefreshToken(refreshToken, refreshTokenExpiry);

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Email} refreshed token successfully", user.Email.Email);

        return Result<TokenResponse>.Success(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            TokenType = "Bearer"
        });
    }

    public async Task<Result> RevokeTokenAsync(string refreshToken)
    {
        var userResult = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (!userResult.IsSuccess)
        {
            var exception = UserException.InvalidRefreshToken();
            return Result.Failure(
                ErrorType.Unauthorized,
                new Error(exception.Code.ToString(), exception.Message));
        }

        var user = userResult.Value!;
        user.ClearRefreshToken();

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Email} revoked refresh token", user.Email.Email);

        return Result.Success();
    }

    private async Task<string> GenerateAccessTokenAsync(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.Value.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, $"{user.Name.FirstName} {user.Name.LastName}"),
            new("userId", user.UserId.Value.ToString())
        };

        if (user.EmployeeId is not null)
        {
            claims.Add(new Claim("employeeId", user.EmployeeId.Value.ToString()));

            var employeeResult = await _employeeRepository.GetByIdAsync(user.EmployeeId);
            if (employeeResult.IsSuccess && employeeResult.Value?.EmployeeRole is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, employeeResult.Value.EmployeeRole.RoleName.Name));
            }
        }

        if (!string.IsNullOrEmpty(user.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, user.Role));
        }

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
