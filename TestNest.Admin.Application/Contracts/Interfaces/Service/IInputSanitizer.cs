namespace TestNest.Admin.Application.Contracts.Interfaces.Service;

public interface IInputSanitizer
{
    string Sanitize(string? input);
    string SanitizeHtml(string? input);
    string SanitizeSql(string? input);
    bool ContainsPotentialXss(string? input);
    bool ContainsPotentialSqlInjection(string? input);
}
