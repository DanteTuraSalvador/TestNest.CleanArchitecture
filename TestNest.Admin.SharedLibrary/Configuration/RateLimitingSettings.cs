namespace TestNest.Admin.SharedLibrary.Configuration;

public class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; set; } = 100;
    public int WindowInSeconds { get; set; } = 60;
    public int QueueLimit { get; set; } = 10;
}
