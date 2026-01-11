namespace TestNest.Admin.SharedLibrary.Configuration;

public class ResponseCachingSettings
{
    public const string SectionName = "ResponseCaching";

    public bool Enabled { get; set; } = true;
    public long MaxBodySizeInBytes { get; set; } = 64 * 1024 * 1024; // 64 MB
    public bool UseCaseSensitivePaths { get; set; } = false;
    public int DefaultDurationInSeconds { get; set; } = 60;
    public CacheProfileSettings[] CacheProfiles { get; set; } = [];
}

public class CacheProfileSettings
{
    public string Name { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; } = 60;
    public string? VaryByHeader { get; set; }
    public string? VaryByQueryKeys { get; set; }
    public bool NoStore { get; set; } = false;
}
