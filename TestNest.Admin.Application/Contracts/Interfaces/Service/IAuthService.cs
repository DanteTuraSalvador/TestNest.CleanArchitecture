using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Auth;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Auth;

namespace TestNest.Admin.Application.Contracts.Interfaces.Service;

public interface IAuthService
{
    Task<Result<TokenResponse>> LoginAsync(LoginRequest request);
    Task<Result<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request);
    Task<Result> RevokeTokenAsync(string refreshToken);
}
