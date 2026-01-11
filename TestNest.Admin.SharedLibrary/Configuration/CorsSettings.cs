namespace TestNest.Admin.SharedLibrary.Configuration;

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string PolicyName { get; set; } = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"];
    public string[] AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Api-Version", "X-Requested-With"];
    public string[] ExposedHeaders { get; set; } = ["X-Pagination", "X-Total-Count"];
    public bool AllowCredentials { get; set; } = true;
    public int MaxAgeInSeconds { get; set; } = 600;
}
