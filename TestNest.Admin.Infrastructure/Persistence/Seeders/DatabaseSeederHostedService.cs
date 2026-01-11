using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders;

public sealed class DatabaseSeederHostedService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        IHostEnvironment? hostEnvironment = _serviceProvider.GetService<IHostEnvironment>();
        if (hostEnvironment?.IsDevelopment() == true)
        {
            DatabaseSeeder.SeedWithRetry(_serviceProvider);
        }
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
