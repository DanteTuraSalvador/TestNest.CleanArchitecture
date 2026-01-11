namespace TestNest.Admin.SharedLibrary.Configuration;

public class ConnectionPoolingSettings
{
    public const string SectionName = "ConnectionPooling";

    public int MinPoolSize { get; set; } = 5;
    public int MaxPoolSize { get; set; } = 100;
    public int ConnectionLifetimeInSeconds { get; set; } = 0;
    public int ConnectionIdleTimeoutInSeconds { get; set; } = 300;
    public bool EnablePooling { get; set; } = true;
    public bool MultipleActiveResultSets { get; set; } = true;
    public int ConnectTimeoutInSeconds { get; set; } = 30;
    public int CommandTimeoutInSeconds { get; set; } = 30;
    public bool TrustServerCertificate { get; set; } = false;
    public bool Encrypt { get; set; } = true;
}
