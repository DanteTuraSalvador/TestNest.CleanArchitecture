namespace TestNest.Admin.SharedLibrary.Configuration;

public class ResilienceSettings
{
    public const string SectionName = "Resilience";

    public RetrySettings Retry { get; set; } = new();
    public CircuitBreakerSettings CircuitBreaker { get; set; } = new();
    public TimeoutSettings Timeout { get; set; } = new();
    public BulkheadSettings Bulkhead { get; set; } = new();
}

public class RetrySettings
{
    public bool Enabled { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int BaseDelayInMilliseconds { get; set; } = 200;
    public int MaxDelayInMilliseconds { get; set; } = 30000;
    public bool UseExponentialBackoff { get; set; } = true;
}

public class CircuitBreakerSettings
{
    public bool Enabled { get; set; } = true;
    public double FailureRatio { get; set; } = 0.5;
    public int MinimumThroughput { get; set; } = 10;
    public int SamplingDurationInSeconds { get; set; } = 30;
    public int BreakDurationInSeconds { get; set; } = 30;
}

public class TimeoutSettings
{
    public bool Enabled { get; set; } = true;
    public int TimeoutInSeconds { get; set; } = 30;
}

public class BulkheadSettings
{
    public bool Enabled { get; set; } = true;
    public int MaxParallelization { get; set; } = 100;
    public int MaxQueuingActions { get; set; } = 50;
}
