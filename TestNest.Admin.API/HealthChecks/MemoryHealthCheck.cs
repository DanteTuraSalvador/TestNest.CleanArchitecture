using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace TestNest.Admin.API.HealthChecks;

public class MemoryHealthCheck : IHealthCheck
{
    private readonly IOptionsMonitor<MemoryHealthCheckOptions> _options;

    public MemoryHealthCheck(IOptionsMonitor<MemoryHealthCheckOptions> options)
    {
        _options = options;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var options = _options.CurrentValue;
        var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var allocatedMegabytes = allocatedBytes / 1024.0 / 1024.0;

        var data = new Dictionary<string, object>
        {
            { "AllocatedMegabytes", allocatedMegabytes },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) },
            { "ThresholdMegabytes", options.ThresholdInMegabytes }
        };

        var status = allocatedMegabytes < options.ThresholdInMegabytes
            ? HealthStatus.Healthy
            : HealthStatus.Degraded;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: $"Memory usage: {allocatedMegabytes:F2} MB / {options.ThresholdInMegabytes} MB threshold",
            data: data));
    }
}

public class MemoryHealthCheckOptions
{
    public long ThresholdInMegabytes { get; set; } = 500;
}
