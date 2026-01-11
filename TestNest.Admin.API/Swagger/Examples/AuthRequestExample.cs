using Swashbuckle.AspNetCore.Filters;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Auth;

namespace TestNest.Admin.API.Swagger.Examples;

public class LoginRequestExample : IExamplesProvider<LoginRequest>
{
    public LoginRequest GetExamples()
    {
        return new LoginRequest
        {
            Email = "admin@testnest.com",
            Password = "Admin@123"
        };
    }
}

public class RefreshTokenRequestExample : IExamplesProvider<RefreshTokenRequest>
{
    public RefreshTokenRequest GetExamples()
    {
        return new RefreshTokenRequest
        {
            RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
        };
    }
}

public class RevokeTokenRequestExample : IExamplesProvider<RevokeTokenRequest>
{
    public RevokeTokenRequest GetExamples()
    {
        return new RevokeTokenRequest
        {
            RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
        };
    }
}
