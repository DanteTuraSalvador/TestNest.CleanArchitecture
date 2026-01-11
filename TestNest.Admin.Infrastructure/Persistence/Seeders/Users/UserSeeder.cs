using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Domain.Users;
using TestNest.Admin.SharedLibrary.Authorization;
using TestNest.Admin.SharedLibrary.Common.Results;
using TestNest.Admin.SharedLibrary.ValueObjects;

namespace TestNest.Admin.Infrastructure.Persistence.Seeders.Users;

public static class UserSeeder
{
    public static void SeedAdminUser(ApplicationDbContext context)
    {
        User? existingAdmin = context.Users
            .FirstOrDefault(u => EF.Property<string>(u.Email, "Email") == "admin@testnest.com");

        if (existingAdmin is null)
        {
            Result<EmailAddress> emailResult = EmailAddress.Create("admin@testnest.com");
            Result<PersonName> nameResult = PersonName.Create("System", null, "Administrator");

            if (!emailResult.IsSuccess || !nameResult.IsSuccess)
            {
                Console.WriteLine("Failed to create admin user: Invalid email or name");
                return;
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            Result<User> userResult = User.Create(
                emailResult.Value!,
                passwordHash,
                nameResult.Value!,
                null,
                Roles.Admin
            );

            if (userResult.IsSuccess)
            {
                context.Users.Add(userResult.Value!);
                context.SaveChanges();
                Console.WriteLine("Admin user seeded successfully!");
                Console.WriteLine("Email: admin@testnest.com");
                Console.WriteLine("Password: Admin@123");
                Console.WriteLine($"Role: {Roles.Admin}");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", userResult.Errors.Select(e => e.Message))}");
            }
        }
        else if (existingAdmin.Role is null)
        {
            context.Database.ExecuteSqlRaw(
                "UPDATE Users SET Role = {0} WHERE Email = {1}",
                Roles.Admin,
                "admin@testnest.com");
            Console.WriteLine($"Updated admin user with role: {Roles.Admin}");
        }
    }
}
