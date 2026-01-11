using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System.IO.Compression;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Azure.Monitor.OpenTelemetry.Exporter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using TestNest.Admin.API.Exceptions;
using TestNest.Admin.API.HealthChecks;
using TestNest.Admin.API.Telemetry;
using TestNest.Admin.API.Middleware;
using TestNest.Admin.Application;
using TestNest.Admin.Infrastructure;
using TestNest.Admin.Infrastructure.Persistence;
using TestNest.Admin.API.HostedServices;
using TestNest.Admin.SharedLibrary.Authorization;
using TestNest.Admin.SharedLibrary.Configuration;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Employee;
using TestNest.Admin.SharedLibrary.Dtos.Requests.Establishment;
using TestNest.Admin.SharedLibrary.Exceptions.Common;

namespace TestNest.Admin.API;

public static class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog bootstrap logger for capturing startup errors
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            Log.Information("Starting TestNest.Admin.API");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Configure Azure Key Vault for non-Development environments
        if (!builder.Environment.IsDevelopment())
        {
            var keyVaultUri = builder.Configuration["KeyVault:Uri"];
            if (!string.IsNullOrEmpty(keyVaultUri))
            {
                builder.Configuration.AddAzureKeyVault(
                    new Uri(keyVaultUri),
                    new DefaultAzureCredential());
            }
        }

        // Configure Serilog from appsettings.json
        _ = builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithThreadId()
            .Enrich.WithCorrelationId()
            .Enrich.WithProperty("Application", "TestNest.Admin.API")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

        _ = builder.Services.AddControllers()
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver());

        // Configure API Versioning
        _ = builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new MediaTypeApiVersionReader("ver"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        _ = builder.Services.AddEndpointsApiExplorer();

        var openApiPatchSchema = new OpenApiSchema
        {
            Type = "array",
            Items = new OpenApiSchema
            {
                Type = "object",
                Properties = new Dictionary<string, OpenApiSchema>
                {
                    { "op", new OpenApiSchema { Type = "string", Description = "Operation type (replace, add, remove, etc.)" } },
                    { "path", new OpenApiSchema { Type = "string", Description = "Path to the property to update" } },
                    { "value", new OpenApiSchema { Type = "object", Description = "New value for the property" } }
                }
            }
        };

        _ = builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Palawan Nest", Version = "v1" });
            c.MapType<JsonPatchDocument<EmployeePatchRequest>>(() => openApiPatchSchema);
            c.MapType<JsonPatchDocument<EstablishmentPatchRequest>>(() => openApiPatchSchema);
            c.MapType<JsonPatchDocument<EstablishmentAddressPatchRequest>>(() => openApiPatchSchema);
            c.MapType<JsonPatchDocument<EstablishmentContactPatchRequest>>(() => openApiPatchSchema);
            c.MapType<JsonPatchDocument<EstablishmentMemberPatchRequest>>(() => openApiPatchSchema);
            c.MapType<JsonPatchDocument<EstablishmentPhonePatchRequest>>(() => openApiPatchSchema);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // Enable request/response examples
            c.EnableAnnotations();
            c.ExampleFilters();
        });

        // Register Swagger examples
        _ = builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

        _ = builder.Services.AddScoped<IErrorResponseService, ErrorResponseService>();
        _ = builder.Services.AddHttpContextAccessor();
        _ = builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        _ = builder.Services.AddDbContext<ApplicationDbContext>(options =>
             options.UseSqlServer(
                 builder.Configuration.GetConnectionString("DefaultConnection")
                 //,sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
             )
         );

        _ = builder.Services.AddPersistenceServices(builder.Configuration);
        _ = builder.Services.AddApplicationServices(builder.Configuration);

        // Configure Graceful Shutdown
        _ = builder.Services.Configure<GracefulShutdownSettings>(
            builder.Configuration.GetSection(GracefulShutdownSettings.SectionName));
        _ = builder.Services.AddHostedService<GracefulShutdownService>();

        var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;

        _ = builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        _ = builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireAdmin, policy =>
                policy.RequireRole(Roles.Admin));

            options.AddPolicy(Policies.RequireManager, policy =>
                policy.RequireRole(Roles.Manager));

            options.AddPolicy(Policies.RequireStaff, policy =>
                policy.RequireRole(Roles.Staff));

            options.AddPolicy(Policies.RequireManagerOrAdmin, policy =>
                policy.RequireRole(Roles.Admin, Roles.Manager));
        });

        // Configure CORS
        var corsSettings = builder.Configuration
            .GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>() ?? new CorsSettings();

        _ = builder.Services.AddCors(options =>
        {
            options.AddPolicy(corsSettings.PolicyName, policy =>
            {
                if (corsSettings.AllowedOrigins.Length > 0)
                {
                    _ = policy.WithOrigins(corsSettings.AllowedOrigins);
                }
                else
                {
                    _ = policy.AllowAnyOrigin();
                }

                if (corsSettings.AllowedMethods.Length > 0)
                {
                    _ = policy.WithMethods(corsSettings.AllowedMethods);
                }
                else
                {
                    _ = policy.AllowAnyMethod();
                }

                if (corsSettings.AllowedHeaders.Length > 0)
                {
                    _ = policy.WithHeaders(corsSettings.AllowedHeaders);
                }
                else
                {
                    _ = policy.AllowAnyHeader();
                }

                if (corsSettings.ExposedHeaders.Length > 0)
                {
                    _ = policy.WithExposedHeaders(corsSettings.ExposedHeaders);
                }

                if (corsSettings.AllowCredentials && corsSettings.AllowedOrigins.Length > 0)
                {
                    _ = policy.AllowCredentials();
                }

                _ = policy.SetPreflightMaxAge(TimeSpan.FromSeconds(corsSettings.MaxAgeInSeconds));
            });
        });

        // Configure Response Caching
        var responseCachingSettings = builder.Configuration
            .GetSection(ResponseCachingSettings.SectionName)
            .Get<ResponseCachingSettings>() ?? new ResponseCachingSettings();

        if (responseCachingSettings.Enabled)
        {
            _ = builder.Services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = responseCachingSettings.MaxBodySizeInBytes;
                options.UseCaseSensitivePaths = responseCachingSettings.UseCaseSensitivePaths;
            });

            _ = builder.Services.AddControllers(options =>
            {
                foreach (var profile in responseCachingSettings.CacheProfiles)
                {
                    options.CacheProfiles.Add(profile.Name, new Microsoft.AspNetCore.Mvc.CacheProfile
                    {
                        Duration = profile.DurationInSeconds,
                        VaryByHeader = profile.VaryByHeader,
                        NoStore = profile.NoStore
                    });
                }
            });
        }

        // Configure Response Compression
        var compressionSettings = builder.Configuration
            .GetSection(CompressionSettings.SectionName)
            .Get<CompressionSettings>() ?? new CompressionSettings();

        if (compressionSettings.Enabled)
        {
            _ = builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = compressionSettings.EnableForHttps;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = compressionSettings.MimeTypes;
            });

            _ = builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = compressionSettings.Level;
            });

            _ = builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = compressionSettings.Level;
            });
        }

        // Configure Health Checks
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        _ = builder.Services.AddSingleton<StartupHealthCheck>();
        _ = builder.Services.Configure<MemoryHealthCheckOptions>(options =>
        {
            options.ThresholdInMegabytes = 500;
        });

        _ = builder.Services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(
                "startup",
                tags: ["ready"])
            .AddCheck<MemoryHealthCheck>(
                "memory",
                HealthStatus.Degraded,
                tags: ["live", "memory"])
            .AddSqlServer(
                connectionString ?? string.Empty,
                name: "sqlserver",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "sql", "ready"]);

        // Configure Health Checks UI (Development only)
        if (builder.Environment.IsDevelopment())
        {
            _ = builder.Services.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(30);
                options.MaximumHistoryEntriesPerEndpoint(50);
                options.AddHealthCheckEndpoint("TestNest.Admin.API", "/health");
            }).AddInMemoryStorage();
        }

        // Configure Application Insights
        var appInsightsSettings = builder.Configuration
            .GetSection(ApplicationInsightsSettings.SectionName)
            .Get<ApplicationInsightsSettings>() ?? new ApplicationInsightsSettings();

        if (!string.IsNullOrEmpty(appInsightsSettings.ConnectionString))
        {
            _ = builder.Services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsSettings.ConnectionString;
                options.EnableAdaptiveSampling = appInsightsSettings.EnableAdaptiveSampling;
                options.EnableDependencyTrackingTelemetryModule = appInsightsSettings.EnableDependencyTracking;
                options.EnableRequestTrackingTelemetryModule = appInsightsSettings.EnableRequestTracking;
            });

            _ = builder.Services.AddSingleton<ITelemetryInitializer>(
                new CloudRoleNameTelemetryInitializer(appInsightsSettings.CloudRoleName));
        }

        // Configure OpenTelemetry for Distributed Tracing
        var otelSettings = builder.Configuration
            .GetSection(OpenTelemetrySettings.SectionName)
            .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

        if (otelSettings.Enabled)
        {
            _ = builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(
                        serviceName: otelSettings.ServiceName,
                        serviceVersion: otelSettings.ServiceVersion)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = builder.Environment.EnvironmentName,
                        ["host.name"] = Environment.MachineName
                    }))
                .WithTracing(tracing =>
                {
                    if (otelSettings.Tracing.EnableAspNetCoreInstrumentation)
                    {
                        _ = tracing.AddAspNetCoreInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.Filter = httpContext =>
                                !httpContext.Request.Path.StartsWithSegments("/health") &&
                                !httpContext.Request.Path.StartsWithSegments("/health-ui");
                        });
                    }

                    if (otelSettings.Tracing.EnableHttpClientInstrumentation)
                    {
                        _ = tracing.AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        });
                    }

                    if (otelSettings.Tracing.EnableSqlClientInstrumentation)
                    {
                        _ = tracing.AddSqlClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                            options.SetDbStatementForText = true;
                            options.SetDbStatementForStoredProcedure = true;
                        });
                    }

                    if (otelSettings.EnableConsoleExporter && builder.Environment.IsDevelopment())
                    {
                        _ = tracing.AddConsoleExporter();
                    }

                    if (!string.IsNullOrEmpty(otelSettings.OtlpEndpoint))
                    {
                        _ = tracing.AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri(otelSettings.OtlpEndpoint);
                        });
                    }

                    if (otelSettings.EnableAzureMonitorExporter &&
                        !string.IsNullOrEmpty(otelSettings.AzureMonitorConnectionString))
                    {
                        _ = tracing.AddAzureMonitorTraceExporter(options =>
                        {
                            options.ConnectionString = otelSettings.AzureMonitorConnectionString;
                        });
                    }
                });
        }

        // Configure Rate Limiting
        var rateLimitingSettings = builder.Configuration
            .GetSection(RateLimitingSettings.SectionName)
            .Get<RateLimitingSettings>() ?? new RateLimitingSettings();

        _ = builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("fixed", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitingSettings.PermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowInSeconds);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitingSettings.QueueLimit;
            });

            options.AddSlidingWindowLimiter("sliding", limiterOptions =>
            {
                limiterOptions.PermitLimit = rateLimitingSettings.PermitLimit;
                limiterOptions.Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowInSeconds);
                limiterOptions.SegmentsPerWindow = 4;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitingSettings.QueueLimit;
            });

            options.AddTokenBucketLimiter("token", limiterOptions =>
            {
                limiterOptions.TokenLimit = rateLimitingSettings.PermitLimit;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = rateLimitingSettings.QueueLimit;
                limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimitingSettings.WindowInSeconds);
                limiterOptions.TokensPerPeriod = rateLimitingSettings.PermitLimit / 2;
                limiterOptions.AutoReplenishment = true;
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var clientId = httpContext.User.Identity?.Name
                    ?? httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientId,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitingSettings.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowInSeconds),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = rateLimitingSettings.QueueLimit
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds.ToString("0")
                    : rateLimitingSettings.WindowInSeconds.ToString();

                context.HttpContext.Response.Headers["Retry-After"] = retryAfter;

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = $"Rate limit exceeded. Please retry after {retryAfter} seconds."
                }, cancellationToken: token);
            };
        });

        // Configure graceful shutdown timeout for the host
        var gracefulShutdownSettings = builder.Configuration
            .GetSection(GracefulShutdownSettings.SectionName)
            .Get<GracefulShutdownSettings>() ?? new GracefulShutdownSettings();

        _ = builder.Host.ConfigureHostOptions(options =>
        {
            options.ShutdownTimeout = TimeSpan.FromSeconds(gracefulShutdownSettings.TimeoutInSeconds);
        });

        WebApplication app = builder.Build();

        // Apply Response Compression - must be early in pipeline
        if (compressionSettings.Enabled)
        {
            _ = app.UseResponseCompression();
        }

        // Configure Security Headers
        var securityHeadersSettings = builder.Configuration
            .GetSection(SecurityHeadersSettings.SectionName)
            .Get<SecurityHeadersSettings>() ?? new SecurityHeadersSettings();

        // Add Serilog request logging - logs HTTP requests with timing, status code, and path
        _ = app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode > 499
                    ? LogEventLevel.Error
                    : elapsed > 3000
                        ? LogEventLevel.Warning
                        : LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                }
            };
        });

        if (app.Environment.IsDevelopment())
        {
            _ = app.UseSwagger();
            _ = app.UseSwaggerUI();
        }

        // Apply Security Headers middleware early in the pipeline
        _ = app.UseSecurityHeaders(securityHeadersSettings);

        // HSTS for production
        if (!app.Environment.IsDevelopment() && securityHeadersSettings.EnableHsts)
        {
            _ = app.UseHsts();
        }

        // Only redirect to HTTPS in non-development environments
        // In development, this can cause issues with Swagger and token preservation
        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseHttpsRedirection();
        }

        _ = app.UseMiddleware<ExceptionHandlingMiddleware>();

        // Apply CORS - must be before authentication
        _ = app.UseCors(corsSettings.PolicyName);

        // Apply Response Caching
        if (responseCachingSettings.Enabled)
        {
            _ = app.UseResponseCaching();
        }

        // Apply Rate Limiting
        _ = app.UseRateLimiter();

        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        _ = app.MapControllers();

        // Map Health Check endpoints
        _ = app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        _ = app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        _ = app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Health Check UI (Development only)
        if (app.Environment.IsDevelopment())
        {
            _ = app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
            });
        }

        // Mark startup as complete
        var startupHealthCheck = app.Services.GetRequiredService<StartupHealthCheck>();
        startupHealthCheck.IsReady = true;

            Log.Information("TestNest.Admin.API started successfully");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
