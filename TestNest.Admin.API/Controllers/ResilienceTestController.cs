using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TestNest.Admin.API.Controllers;

/// <summary>
/// Controller for testing resilience features (Development only)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[AllowAnonymous]
public class ResilienceTestController : ControllerBase
{
    private readonly ILogger<ResilienceTestController> _logger;
    private readonly IConfiguration _configuration;
    private static int _requestCount = 0;
    private static int _failureCount = 0;
    private static bool _simulateFailure = false;

    public ResilienceTestController(
        ILogger<ResilienceTestController> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Test endpoint that simulates transient failures to demonstrate retry policy
    /// </summary>
    /// <param name="failCount">Number of times to fail before succeeding (default: 2)</param>
    [HttpGet("retry-test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult TestRetry([FromQuery] int failCount = 2)
    {
        _requestCount++;

        if (_failureCount < failCount)
        {
            _failureCount++;
            _logger.LogWarning(
                "Simulating transient failure {FailureCount}/{TotalFailures}. Request #{RequestCount}",
                _failureCount, failCount, _requestCount);

            return StatusCode(503, new
            {
                message = $"Simulated transient failure ({_failureCount}/{failCount})",
                requestNumber = _requestCount,
                timestamp = DateTime.UtcNow
            });
        }

        var result = new
        {
            message = "Success after retries!",
            totalRequests = _requestCount,
            failuresSimulated = _failureCount,
            timestamp = DateTime.UtcNow
        };

        // Reset counters
        _failureCount = 0;
        _requestCount = 0;

        _logger.LogInformation("Request succeeded after {Failures} simulated failures", failCount);
        return Ok(result);
    }

    /// <summary>
    /// Toggle failure simulation for circuit breaker testing
    /// </summary>
    [HttpPost("circuit-breaker/toggle")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ToggleFailureSimulation()
    {
        _simulateFailure = !_simulateFailure;
        _logger.LogWarning("Failure simulation is now: {Status}", _simulateFailure ? "ENABLED" : "DISABLED");

        return Ok(new
        {
            simulateFailure = _simulateFailure,
            message = _simulateFailure
                ? "All requests to /circuit-breaker/test will now fail (503)"
                : "Requests will succeed normally"
        });
    }

    /// <summary>
    /// Test endpoint for circuit breaker - fails when simulation is enabled
    /// </summary>
    [HttpGet("circuit-breaker/test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult TestCircuitBreaker()
    {
        _requestCount++;

        if (_simulateFailure)
        {
            _logger.LogError("Circuit breaker test: Simulated failure. Request #{RequestCount}", _requestCount);
            return StatusCode(503, new
            {
                message = "Service unavailable (simulated failure)",
                requestNumber = _requestCount,
                timestamp = DateTime.UtcNow,
                hint = "Call POST /circuit-breaker/toggle to disable failures"
            });
        }

        return Ok(new
        {
            message = "Service is healthy",
            requestNumber = _requestCount,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test endpoint with configurable delay to demonstrate timeout policy
    /// </summary>
    /// <param name="delaySeconds">Seconds to delay response (default: 5)</param>
    [HttpGet("timeout-test")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestTimeout([FromQuery] int delaySeconds = 5)
    {
        _logger.LogInformation("Starting delayed response. Delay: {Delay}s", delaySeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));

        return Ok(new
        {
            message = $"Response after {delaySeconds} second delay",
            delaySeconds,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current resilience configuration
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetResilienceConfig()
    {
        var resilienceSection = _configuration.GetSection("Resilience");
        var gracefulShutdownSection = _configuration.GetSection("GracefulShutdown");
        var connectionPoolingSection = _configuration.GetSection("ConnectionPooling");

        return Ok(new
        {
            resilience = new
            {
                retry = new
                {
                    enabled = resilienceSection.GetValue<bool>("Retry:Enabled"),
                    maxRetryAttempts = resilienceSection.GetValue<int>("Retry:MaxRetryAttempts"),
                    baseDelayMs = resilienceSection.GetValue<int>("Retry:BaseDelayInMilliseconds"),
                    useExponentialBackoff = resilienceSection.GetValue<bool>("Retry:UseExponentialBackoff")
                },
                circuitBreaker = new
                {
                    enabled = resilienceSection.GetValue<bool>("CircuitBreaker:Enabled"),
                    failureRatio = resilienceSection.GetValue<double>("CircuitBreaker:FailureRatio"),
                    breakDurationSeconds = resilienceSection.GetValue<int>("CircuitBreaker:BreakDurationInSeconds")
                },
                timeout = new
                {
                    enabled = resilienceSection.GetValue<bool>("Timeout:Enabled"),
                    timeoutSeconds = resilienceSection.GetValue<int>("Timeout:TimeoutInSeconds")
                }
            },
            gracefulShutdown = new
            {
                enabled = gracefulShutdownSection.GetValue<bool>("Enabled"),
                timeoutSeconds = gracefulShutdownSection.GetValue<int>("TimeoutInSeconds"),
                drainTimeSeconds = gracefulShutdownSection.GetValue<int>("DrainTimeInSeconds")
            },
            connectionPooling = new
            {
                enabled = connectionPoolingSection.GetValue<bool>("EnablePooling"),
                minPoolSize = connectionPoolingSection.GetValue<int>("MinPoolSize"),
                maxPoolSize = connectionPoolingSection.GetValue<int>("MaxPoolSize")
            },
            testEndpoints = new
            {
                retryTest = "GET /api/v1/ResilienceTest/retry-test?failCount=2",
                circuitBreakerToggle = "POST /api/v1/ResilienceTest/circuit-breaker/toggle",
                circuitBreakerTest = "GET /api/v1/ResilienceTest/circuit-breaker/test",
                timeoutTest = "GET /api/v1/ResilienceTest/timeout-test?delaySeconds=5"
            }
        });
    }

    /// <summary>
    /// Reset all test counters
    /// </summary>
    [HttpPost("reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult ResetCounters()
    {
        _requestCount = 0;
        _failureCount = 0;
        _simulateFailure = false;

        _logger.LogInformation("All resilience test counters have been reset");

        return Ok(new
        {
            message = "All counters reset",
            requestCount = _requestCount,
            failureCount = _failureCount,
            simulateFailure = _simulateFailure
        });
    }
}
