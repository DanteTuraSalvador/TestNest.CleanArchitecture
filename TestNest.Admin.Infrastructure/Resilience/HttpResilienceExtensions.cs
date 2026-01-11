using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using TestNest.Admin.SharedLibrary.Configuration;

namespace TestNest.Admin.Infrastructure.Resilience;

public static class HttpResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResilienceHandler(
        this IHttpClientBuilder builder,
        ResilienceSettings settings)
    {
        builder.AddResilienceHandler("StandardResilience", configure =>
        {
            // Add Timeout (outermost)
            if (settings.Timeout.Enabled)
            {
                configure.AddTimeout(TimeSpan.FromSeconds(settings.Timeout.TimeoutInSeconds));
            }

            // Add Circuit Breaker
            if (settings.CircuitBreaker.Enabled)
            {
                configure.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = settings.CircuitBreaker.FailureRatio,
                    MinimumThroughput = settings.CircuitBreaker.MinimumThroughput,
                    SamplingDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.SamplingDurationInSeconds),
                    BreakDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.BreakDurationInSeconds)
                });
            }

            // Add Retry
            if (settings.Retry.Enabled)
            {
                configure.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = settings.Retry.MaxRetryAttempts,
                    BackoffType = settings.Retry.UseExponentialBackoff
                        ? DelayBackoffType.Exponential
                        : DelayBackoffType.Constant,
                    Delay = TimeSpan.FromMilliseconds(settings.Retry.BaseDelayInMilliseconds),
                    MaxDelay = TimeSpan.FromMilliseconds(settings.Retry.MaxDelayInMilliseconds),
                    UseJitter = true
                });
            }
        });

        return builder;
    }

    public static IHttpClientBuilder AddStandardResilienceDefaults(this IHttpClientBuilder builder)
    {
        // Uses Microsoft's standard resilience handler with sensible defaults
        builder.AddStandardResilienceHandler();
        return builder;
    }
}
