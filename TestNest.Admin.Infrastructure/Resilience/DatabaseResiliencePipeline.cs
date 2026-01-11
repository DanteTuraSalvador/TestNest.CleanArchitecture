using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using TestNest.Admin.SharedLibrary.Configuration;

namespace TestNest.Admin.Infrastructure.Resilience;

public static class DatabaseResiliencePipeline
{
    public static ResiliencePipeline<T> Create<T>(
        ResilienceSettings settings,
        ILogger logger)
    {
        var pipelineBuilder = new ResiliencePipelineBuilder<T>();

        // Add Timeout strategy (outermost - applies to entire operation)
        if (settings.Timeout.Enabled)
        {
            pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(settings.Timeout.TimeoutInSeconds),
                OnTimeout = args =>
                {
                    logger.LogWarning(
                        "Database operation timed out after {Timeout}s",
                        settings.Timeout.TimeoutInSeconds);
                    return default;
                }
            });
        }

        // Add Circuit Breaker strategy
        if (settings.CircuitBreaker.Enabled)
        {
            pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions<T>
            {
                FailureRatio = settings.CircuitBreaker.FailureRatio,
                MinimumThroughput = settings.CircuitBreaker.MinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.SamplingDurationInSeconds),
                BreakDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.BreakDurationInSeconds),
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<SqlException>()
                    .Handle<DbUpdateException>()
                    .Handle<TimeoutException>(),
                OnOpened = args =>
                {
                    logger.LogWarning(
                        "Circuit breaker opened. Break duration: {BreakDuration}s. Reason: {Outcome}",
                        settings.CircuitBreaker.BreakDurationInSeconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed. Normal operation resumed.");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Circuit breaker half-opened. Testing if service recovered.");
                    return default;
                }
            });
        }

        // Add Retry strategy (innermost - retries individual operations)
        if (settings.Retry.Enabled)
        {
            pipelineBuilder.AddRetry(new RetryStrategyOptions<T>
            {
                MaxRetryAttempts = settings.Retry.MaxRetryAttempts,
                BackoffType = settings.Retry.UseExponentialBackoff
                    ? DelayBackoffType.Exponential
                    : DelayBackoffType.Constant,
                Delay = TimeSpan.FromMilliseconds(settings.Retry.BaseDelayInMilliseconds),
                MaxDelay = TimeSpan.FromMilliseconds(settings.Retry.MaxDelayInMilliseconds),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<T>()
                    .Handle<SqlException>(ex => IsTransientSqlException(ex))
                    .Handle<DbUpdateException>(ex => ex.InnerException is SqlException sqlEx && IsTransientSqlException(sqlEx))
                    .Handle<TimeoutException>(),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry attempt {AttemptNumber} of {MaxRetries} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        settings.Retry.MaxRetryAttempts,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return default;
                }
            });
        }

        return pipelineBuilder.Build();
    }

    public static ResiliencePipeline CreateNonGeneric(
        ResilienceSettings settings,
        ILogger logger)
    {
        var pipelineBuilder = new ResiliencePipelineBuilder();

        // Add Timeout strategy
        if (settings.Timeout.Enabled)
        {
            pipelineBuilder.AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = TimeSpan.FromSeconds(settings.Timeout.TimeoutInSeconds),
                OnTimeout = args =>
                {
                    logger.LogWarning(
                        "Database operation timed out after {Timeout}s",
                        settings.Timeout.TimeoutInSeconds);
                    return default;
                }
            });
        }

        // Add Circuit Breaker strategy
        if (settings.CircuitBreaker.Enabled)
        {
            pipelineBuilder.AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = settings.CircuitBreaker.FailureRatio,
                MinimumThroughput = settings.CircuitBreaker.MinimumThroughput,
                SamplingDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.SamplingDurationInSeconds),
                BreakDuration = TimeSpan.FromSeconds(settings.CircuitBreaker.BreakDurationInSeconds),
                ShouldHandle = new PredicateBuilder()
                    .Handle<SqlException>()
                    .Handle<DbUpdateException>()
                    .Handle<TimeoutException>(),
                OnOpened = args =>
                {
                    logger.LogWarning(
                        "Circuit breaker opened. Break duration: {BreakDuration}s. Reason: {Outcome}",
                        settings.CircuitBreaker.BreakDurationInSeconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return default;
                },
                OnClosed = args =>
                {
                    logger.LogInformation("Circuit breaker closed. Normal operation resumed.");
                    return default;
                },
                OnHalfOpened = args =>
                {
                    logger.LogInformation("Circuit breaker half-opened. Testing if service recovered.");
                    return default;
                }
            });
        }

        // Add Retry strategy
        if (settings.Retry.Enabled)
        {
            pipelineBuilder.AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = settings.Retry.MaxRetryAttempts,
                BackoffType = settings.Retry.UseExponentialBackoff
                    ? DelayBackoffType.Exponential
                    : DelayBackoffType.Constant,
                Delay = TimeSpan.FromMilliseconds(settings.Retry.BaseDelayInMilliseconds),
                MaxDelay = TimeSpan.FromMilliseconds(settings.Retry.MaxDelayInMilliseconds),
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<SqlException>(ex => IsTransientSqlException(ex))
                    .Handle<DbUpdateException>(ex => ex.InnerException is SqlException sqlEx && IsTransientSqlException(sqlEx))
                    .Handle<TimeoutException>(),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry attempt {AttemptNumber} of {MaxRetries} after {Delay}ms. Exception: {Exception}",
                        args.AttemptNumber,
                        settings.Retry.MaxRetryAttempts,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return default;
                }
            });
        }

        return pipelineBuilder.Build();
    }

    private static bool IsTransientSqlException(SqlException ex)
    {
        // SQL Server transient error numbers
        // https://docs.microsoft.com/en-us/azure/azure-sql/database/troubleshoot-common-errors-issues
        int[] transientErrorNumbers =
        [
            -2,      // Timeout
            -1,      // Connection error
            2,       // Timeout
            53,      // Could not connect
            121,     // Semaphore timeout
            233,     // Connection initialization error
            258,     // Timeout
            1205,    // Deadlock
            1222,    // Lock request timeout
            4060,    // Cannot open database
            4221,    // Login failed
            10053,   // Connection forcibly closed
            10054,   // Connection reset
            10060,   // Connection timeout
            10922,   // Rebuild required
            10928,   // Resource limit reached
            10929,   // Resource limit reached
            10936,   // Resource limit reached
            11001,   // Host not found
            40143,   // Connection could not be initialized
            40197,   // Error processing request
            40501,   // Service busy
            40540,   // Database unavailable
            40544,   // Database size quota reached
            40549,   // Session terminated - long running transaction
            40550,   // Session terminated - too many locks
            40551,   // Session terminated - excessive tempdb usage
            40552,   // Session terminated - excessive transaction log usage
            40553,   // Session terminated - excessive memory usage
            40613,   // Database unavailable
            40615,   // Cannot connect
            40627,   // Operation in progress
            40636,   // Cannot open
            40650,   // Databases are partitioned
            40671,   // Gateway communication failure
            41301,   // Dependency failure
            41302,   // Transaction aborted
            41305,   // Repeatable read validation failure
            41325,   // Serializable validation failure
            41839,   // Transaction exceeded max number of commit dependencies
            49918,   // Not enough resources
            49919,   // Not enough resources
            49920    // Not enough resources
        ];

        return transientErrorNumbers.Contains(ex.Number);
    }
}
