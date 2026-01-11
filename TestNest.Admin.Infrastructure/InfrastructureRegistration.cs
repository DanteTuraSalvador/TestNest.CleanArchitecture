using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using TestNest.Admin.Application.Contracts.Common;
using TestNest.Admin.Application.Contracts.Interfaces.Persistence;
using TestNest.Admin.Infrastructure.Persistence;
using TestNest.Admin.Infrastructure.Persistence.Interceptors;
using TestNest.Admin.Infrastructure.Persistence.Repositories;
using TestNest.Admin.Infrastructure.Persistence.Seeders;
using TestNest.Admin.Infrastructure.Persistence.UnitOfWork;
using TestNest.Admin.Infrastructure.Resilience;
using TestNest.Admin.SharedLibrary.Configuration;

namespace TestNest.Admin.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register EF Core interceptors
        _ = services.AddSingleton<AuditableEntityInterceptor>();
        _ = services.AddSingleton<SoftDeleteInterceptor>();

        // Bind settings
        var resilienceSettings = configuration.GetSection(ResilienceSettings.SectionName).Get<ResilienceSettings>() ?? new ResilienceSettings();
        var poolingSettings = configuration.GetSection(ConnectionPoolingSettings.SectionName).Get<ConnectionPoolingSettings>() ?? new ConnectionPoolingSettings();

        // Build optimized connection string with pooling settings
        var connectionString = ConnectionStringBuilder.BuildOptimizedConnectionString(
            configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
            poolingSettings);

        // Configure DbContext with resilience and connection pooling
        _ = services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(
                connectionString,
                sqlOptions =>
                {
                    // Enable retry on failure with SQL Server's built-in resilience
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: resilienceSettings.Retry.MaxRetryAttempts,
                        maxRetryDelay: TimeSpan.FromMilliseconds(resilienceSettings.Retry.MaxDelayInMilliseconds),
                        errorNumbersToAdd: null);

                    // Set command timeout
                    sqlOptions.CommandTimeout(poolingSettings.CommandTimeoutInSeconds > 0
                        ? poolingSettings.CommandTimeoutInSeconds
                        : resilienceSettings.Timeout.TimeoutInSeconds);

                    // Enable MARS for better connection utilization
                    if (poolingSettings.MultipleActiveResultSets)
                    {
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                });

            // Add interceptors
            var auditInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
            var softDeleteInterceptor = serviceProvider.GetRequiredService<SoftDeleteInterceptor>();
            options.AddInterceptors(auditInterceptor, softDeleteInterceptor);
        });

        // Register resilience pipeline for database operations
        _ = services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<ApplicationDbContext>>();
            return DatabaseResiliencePipeline.CreateNonGeneric(resilienceSettings, logger);
        });

        // Register generic resilience pipeline factory
        _ = services.AddSingleton(typeof(ResiliencePipeline<>), typeof(ResiliencePipelineFactory<>));

        _ = services.AddScoped<ISocialMediaPlatformRepository, SocialMediaPlatformRepository>();
        _ = services.AddScoped<IEmployeeRoleRepository, EmployeeRoleRepository>();
        _ = services.AddScoped<IEstablishmentRepository, EstablishmentRepository>();
        _ = services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        _ = services.AddScoped<IEstablishmentAddressRepository, EstablishmentAddressRepository>();
        _ = services.AddScoped<IEstablishmentContactRepository, EstablishmentContactRepository>();
        _ = services.AddScoped<IEstablishmentPhoneRepository, EstablishmentPhoneRepository>();
        _ = services.AddScoped<IEstablishmentMemberRepository, EstablishmentMemberRepository>();
        _ = services.AddScoped<IUserRepository, UserRepository>();
        _ = services.AddScoped<IUnitOfWork, UnitOfWork>();
        _ = services.AddHostedService<DatabaseSeederHostedService>();
        return services;
    }
}

internal sealed class ResiliencePipelineFactory<T>(IConfiguration configuration, ILogger<ResiliencePipelineFactory<T>> logger)
{
    public ResiliencePipeline<T> Pipeline { get; } = DatabaseResiliencePipeline.Create<T>(
        configuration.GetSection(ResilienceSettings.SectionName).Get<ResilienceSettings>() ?? new ResilienceSettings(),
        logger);

    public static implicit operator ResiliencePipeline<T>(ResiliencePipelineFactory<T> factory) => factory.Pipeline;
}

internal static class ConnectionStringBuilder
{
    public static string BuildOptimizedConnectionString(string baseConnectionString, ConnectionPoolingSettings settings)
    {
        if (string.IsNullOrWhiteSpace(baseConnectionString))
            return baseConnectionString;

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = settings.EnablePooling,
            MinPoolSize = settings.MinPoolSize,
            MaxPoolSize = settings.MaxPoolSize,
            ConnectTimeout = settings.ConnectTimeoutInSeconds,
            MultipleActiveResultSets = settings.MultipleActiveResultSets,
            TrustServerCertificate = settings.TrustServerCertificate,
            Encrypt = settings.Encrypt
        };

        // Connection lifetime (0 = no limit)
        if (settings.ConnectionLifetimeInSeconds > 0)
        {
            builder.LoadBalanceTimeout = settings.ConnectionLifetimeInSeconds;
        }

        return builder.ConnectionString;
    }
}
