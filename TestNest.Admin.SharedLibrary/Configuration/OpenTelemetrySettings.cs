namespace TestNest.Admin.SharedLibrary.Configuration;

public class OpenTelemetrySettings
{
    public const string SectionName = "OpenTelemetry";

    public bool Enabled { get; set; } = true;
    public string ServiceName { get; set; } = "TestNest.Admin.API";
    public string ServiceVersion { get; set; } = "1.0.0";
    public string OtlpEndpoint { get; set; } = string.Empty;
    public bool EnableConsoleExporter { get; set; } = false;
    public bool EnableAzureMonitorExporter { get; set; } = false;
    public string AzureMonitorConnectionString { get; set; } = string.Empty;
    public TracingSettings Tracing { get; set; } = new();
}

public class TracingSettings
{
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    public bool EnableSqlClientInstrumentation { get; set; } = true;
    public double SamplingRatio { get; set; } = 1.0;
}
