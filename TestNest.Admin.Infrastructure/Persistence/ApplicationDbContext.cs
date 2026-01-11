using Microsoft.EntityFrameworkCore;
using TestNest.Admin.Domain.Employees;
using TestNest.Admin.Domain.Establishments;
using TestNest.Admin.Domain.SocialMedias;
using TestNest.Admin.Domain.Users;

namespace TestNest.Admin.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<SocialMediaPlatform> SocialMediaPlatforms { get; set; }
    public DbSet<EmployeeRole> EmployeeRoles { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<EstablishmentAddress> EstablishmentAddresses { get; set; }
    public DbSet<EstablishmentContact> EstablishmentContacts { get; set; }
    public DbSet<EstablishmentPhone> EstablishmentPhones { get; set; }
    public DbSet<EstablishmentMember> EstablishmentMembers { get; set; }
    public DbSet<EstablishmentSocialMedia> EstablishmentSocialMedias { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure global query filter for soft delete
        ConfigureSoftDeleteFilter(modelBuilder);
    }

    private static void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
    {
        // Apply global query filter to exclude soft deleted entities
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(SharedLibrary.Common.BaseEntity.ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(SharedLibrary.Common.BaseEntity.ISoftDeletable.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }
}
