using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestNest.Admin.Application.Contracts.Interfaces.Service;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Auth;
using TestNest.Admin.SharedLibrary.Dtos.Responses.Auth;
using TestNest.Admin.SharedLibrary.Exceptions.Common;

namespace TestNest.Admin.API.Controllers;

/// <summary>
/// Handles user authentication and token management operations including login, token refresh, and token revocation.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    IAuthService authService,
    IErrorResponseService errorResponseService) : ControllerBase
{
    private readonly IAuthService _authService = authService;
    private readonly IErrorResponseService _errorResponseService = errorResponseService;

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <returns>A JWT token response with access and refresh tokens.</returns>
    /// <response code="200">Returns the JWT token response.</response>
    /// <response code="400">If the provided credentials are invalid.</response>
    /// <response code="401">If authentication fails.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        Result<TokenResponse> result = await _authService.LoginAsync(request);

        if (result.IsSuccess)
        {
            return Ok(result.Value!);
        }

        return HandleErrorResponse(result.ErrorType, result.Errors);
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <returns>A new JWT token response with access and refresh tokens.</returns>
    /// <response code="200">Returns the new JWT token response.</response>
    /// <response code="401">If the refresh token is invalid or expired.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TokenResponse))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        Result<TokenResponse> result = await _authService.RefreshTokenAsync(request);

        if (result.IsSuccess)
        {
            return Ok(result.Value!);
        }

        return HandleErrorResponse(result.ErrorType, result.Errors);
    }

    /// <summary>
    /// Revokes a refresh token, effectively logging out the user.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The refresh token was successfully revoked.</response>
    /// <response code="401">If the refresh token is invalid.</response>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        Result result = await _authService.RevokeTokenAsync(request.RefreshToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleErrorResponse(result.ErrorType, result.Errors);
    }

    private IActionResult HandleErrorResponse(ErrorType errorType, IEnumerable<Error> errors)
    {
        List<Error> safeErrors = errors?.ToList() ?? [];

        return errorType switch
        {
            ErrorType.Validation =>
                _errorResponseService.CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Validation Error", "Validation failed.",
                    new Dictionary<string, object> { { "errors", safeErrors } }),

            ErrorType.Unauthorized =>
                _errorResponseService.CreateProblemDetails(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized", "Authentication failed.",
                    new Dictionary<string, object> { { "errors", safeErrors } }),

            ErrorType.NotFound =>
                _errorResponseService.CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    "Not Found", "Resource not found.",
                    new Dictionary<string, object> { { "errors", safeErrors } }),

            _ => _errorResponseService.CreateProblemDetails(
                       StatusCodes.Status500InternalServerError,
                       "Internal Server Error", "An unexpected error occurred.")
        };
    }
}
