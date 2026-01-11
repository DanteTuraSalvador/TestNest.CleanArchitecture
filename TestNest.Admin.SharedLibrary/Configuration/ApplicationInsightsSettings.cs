namespace TestNest.Admin.SharedLibrary.Configuration;

public class ApplicationInsightsSettings
{
    public const string SectionName = "ApplicationInsights";

    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableAdaptiveSampling { get; set; } = true;
    public bool EnableDependencyTracking { get; set; } = true;
    public bool EnableRequestTracking { get; set; } = true;
    public bool EnableExceptionTracking { get; set; } = true;
    public bool EnablePerformanceCounterCollection { get; set; } = true;
    public string CloudRoleName { get; set; } = "TestNest.Admin.API";
}
