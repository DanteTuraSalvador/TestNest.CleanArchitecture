namespace TestNest.Admin.SharedLibrary.Configuration;

public class GracefulShutdownSettings
{
    public const string SectionName = "GracefulShutdown";

    public bool Enabled { get; set; } = true;
    public int TimeoutInSeconds { get; set; } = 30;
    public bool WaitForRequestsToComplete { get; set; } = true;
    public int DrainTimeInSeconds { get; set; } = 5;
}
