using TestNest.Admin.SharedLibrary.Configuration;

namespace TestNest.Admin.API.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersSettings _settings;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersSettings settings)
    {
        _next = next;
        _settings = settings;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options - Prevents MIME-sniffing
        context.Response.Headers["X-Content-Type-Options"] = _settings.XContentTypeOptions;

        // X-Frame-Options - Prevents clickjacking
        context.Response.Headers["X-Frame-Options"] = _settings.XFrameOptions;

        // Referrer-Policy - Controls referrer information
        context.Response.Headers["Referrer-Policy"] = _settings.ReferrerPolicy;

        // Content-Security-Policy - Prevents XSS and other injection attacks
        if (!string.IsNullOrEmpty(_settings.ContentSecurityPolicy))
        {
            context.Response.Headers["Content-Security-Policy"] = _settings.ContentSecurityPolicy;
        }

        // Permissions-Policy - Controls browser features
        if (!string.IsNullOrEmpty(_settings.PermissionsPolicy))
        {
            context.Response.Headers["Permissions-Policy"] = _settings.PermissionsPolicy;
        }

        // X-XSS-Protection - Legacy header for older browsers
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Cache-Control for API responses
        if (!context.Response.Headers.ContainsKey("Cache-Control"))
        {
            context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
        }

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder, SecurityHeadersSettings settings)
        => builder.UseMiddleware<SecurityHeadersMiddleware>(settings);
}
