using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TestNest.Admin.API.HealthChecks;

public class StartupHealthCheck : IHealthCheck
{
    private volatile bool _isReady;

    public bool IsReady
    {
        get => _isReady;
        set => _isReady = value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_isReady)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Application has completed startup."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Application is still starting up."));
    }
}
