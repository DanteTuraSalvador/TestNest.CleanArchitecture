using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestNest.Admin.Infrastructure.Persistence.Seeders.Users;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders;

public static class DatabaseSeeder
{
    public static void SeedWithRetry(IServiceProvider serviceProvider, int maxRetries = 3)
    {
        int retryCount = 0;
        bool seeded = false;

        while (retryCount < maxRetries && !seeded)
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            try
            {
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.Migrate();

                UserSeeder.SeedAdminUser(dbContext);
                DataSeeder.SeedSocialMediaPlatforms(dbContext);
                DataSeeder.SeedEstablishments(dbContext, 20);
                DataSeeder.SeedEstablishmentAddresses(dbContext);
                DataSeeder.SeedEstablishmentContacts(dbContext);
                DataSeeder.SeedEstablishmentPhones(dbContext);
                DataSeeder.SeedEmployeeRoles(dbContext, 5);
                DataSeeder.SeedEmployees(dbContext, 50);
                DataSeeder.SeedEstablishmentMembers(dbContext);

                Console.WriteLine("Database seeded successfully!");
                seeded = true;
            }
            catch (Exception ex)
            {
                retryCount++;
                Console.WriteLine($"Seeding attempt {retryCount} failed: {ex.Message}");

                if (retryCount >= maxRetries)
                {
                    Console.WriteLine("MAX RETRIES REACHED. SEEDING FAILED.");
                    throw;
                }

                int delay = (int)Math.Pow(2, retryCount);
                Console.WriteLine($"Retrying in {delay} seconds...");
                Thread.Sleep(delay * 1000);
            }
        }
    }
}
