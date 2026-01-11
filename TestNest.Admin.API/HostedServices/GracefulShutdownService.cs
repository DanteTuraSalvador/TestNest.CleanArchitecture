using Microsoft.Extensions.Options;
using TestNest.Admin.SharedLibrary.Configuration;

namespace TestNest.Admin.API.HostedServices;

public class GracefulShutdownService : IHostedService
{
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<GracefulShutdownService> _logger;
    private readonly GracefulShutdownSettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public GracefulShutdownService(
        IHostApplicationLifetime applicationLifetime,
        ILogger<GracefulShutdownService> logger,
        IOptions<GracefulShutdownSettings> settings,
        IServiceProvider serviceProvider)
    {
        _applicationLifetime = applicationLifetime;
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Graceful shutdown is disabled");
            return Task.CompletedTask;
        }

        _applicationLifetime.ApplicationStarted.Register(OnStarted);
        _applicationLifetime.ApplicationStopping.Register(OnStopping);
        _applicationLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _logger.LogInformation(
            "Application started. Graceful shutdown configured with {Timeout}s timeout",
            _settings.TimeoutInSeconds);
    }

    private void OnStopping()
    {
        _logger.LogInformation(
            "Application is shutting down. Waiting {DrainTime}s for requests to drain...",
            _settings.DrainTimeInSeconds);

        if (_settings.WaitForRequestsToComplete && _settings.DrainTimeInSeconds > 0)
        {
            // Allow time for in-flight requests to complete
            Thread.Sleep(TimeSpan.FromSeconds(_settings.DrainTimeInSeconds));
        }

        _logger.LogInformation("Drain period complete. Proceeding with shutdown...");
    }

    private void OnStopped()
    {
        _logger.LogInformation("Application has stopped gracefully");

        // Flush any remaining logs
        try
        {
            if (_serviceProvider is IDisposable disposable)
            {
                _logger.LogDebug("Disposing service provider...");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during shutdown cleanup");
        }
    }
}
