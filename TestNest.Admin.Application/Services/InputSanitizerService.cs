using System.Text.RegularExpressions;
using System.Web;
using TestNest.Admin.Application.Contracts.Interfaces.Service;

namespace TestNest.Admin.Application.Services;

public partial class InputSanitizerService : IInputSanitizer
{
    // Regex patterns for detecting potential threats
    private static readonly Regex ScriptTagPattern = ScriptTagRegex();
    private static readonly Regex JavaScriptPattern = JavaScriptRegex();
    private static readonly Regex EventHandlerPattern = EventHandlerRegex();
    private static readonly Regex SqlInjectionPattern = SqlInjectionRegex();
    private static readonly Regex HtmlTagPattern = HtmlTagRegex();

    public string Sanitize(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Trim whitespace
        var sanitized = input.Trim();

        // Remove null bytes
        sanitized = sanitized.Replace("\0", string.Empty);

        // Encode HTML entities to prevent XSS
        sanitized = HttpUtility.HtmlEncode(sanitized);

        return sanitized;
    }

    public string SanitizeHtml(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sanitized = input;

        // Remove script tags and their content
        sanitized = ScriptTagPattern.Replace(sanitized, string.Empty);

        // Remove javascript: protocol
        sanitized = JavaScriptPattern.Replace(sanitized, string.Empty);

        // Remove event handlers
        sanitized = EventHandlerPattern.Replace(sanitized, string.Empty);

        // Remove all HTML tags
        sanitized = HtmlTagPattern.Replace(sanitized, string.Empty);

        // Encode remaining content
        sanitized = HttpUtility.HtmlEncode(sanitized);

        return sanitized.Trim();
    }

    public string SanitizeSql(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sanitized = input;

        // Escape single quotes (primary SQL injection vector)
        sanitized = sanitized.Replace("'", "''");

        // Remove common SQL injection patterns
        sanitized = SqlInjectionPattern.Replace(sanitized, string.Empty);

        // Remove semicolons that could terminate statements
        sanitized = sanitized.Replace(";", string.Empty);

        // Remove comment markers
        sanitized = sanitized.Replace("--", string.Empty);
        sanitized = sanitized.Replace("/*", string.Empty);
        sanitized = sanitized.Replace("*/", string.Empty);

        return sanitized.Trim();
    }

    public bool ContainsPotentialXss(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Check for script tags
        if (ScriptTagPattern.IsMatch(input))
            return true;

        // Check for javascript: protocol
        if (JavaScriptPattern.IsMatch(input))
            return true;

        // Check for event handlers
        if (EventHandlerPattern.IsMatch(input))
            return true;

        // Check for data: protocol (can be used for XSS)
        if (input.Contains("data:", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public bool ContainsPotentialSqlInjection(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return SqlInjectionPattern.IsMatch(input);
    }

    [GeneratedRegex(@"<script\b[^<]*(?:(?!</script>)<[^<]*)*</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"javascript\s*:", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex JavaScriptRegex();

    [GeneratedRegex(@"\s*on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex EventHandlerRegex();

    [GeneratedRegex(@"\b(SELECT|INSERT|UPDATE|DELETE|DROP|UNION|ALTER|CREATE|TRUNCATE|EXEC|EXECUTE|XP_|SP_|0X)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex SqlInjectionRegex();

    [GeneratedRegex(@"<[^>]*>", RegexOptions.Compiled, "en-US")]
    private static partial Regex HtmlTagRegex();
}
