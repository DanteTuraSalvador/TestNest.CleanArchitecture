using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TestNest.Admin.SharedLibrary.Common.BaseEntity;

namespace TestNest.Admin.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit fields (CreatedOnUtc, ModifiedOnUtc, CreatedBy, ModifiedBy)
/// for entities implementing IAuditableEntity.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditableEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateAuditableEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<IAuditableEntity>();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOnUtc = utcNow;
                entry.Entity.CreatedBy = GetCurrentUser(); // TODO: Replace with actual user from HttpContext
            }

            if (entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                entry.Entity.ModifiedOnUtc = utcNow;
                entry.Entity.ModifiedBy = GetCurrentUser(); // TODO: Replace with actual user from HttpContext
            }
        }
    }

    private static string? GetCurrentUser()
    {
        // TODO: Implement actual user retrieval from HttpContext or ICurrentUserService
        // For now, return a placeholder
        return "System";
    }
}

/// <summary>
/// Extension methods for EntityEntry.
/// </summary>
internal static class EntityEntryExtensions
{
    /// <summary>
    /// Checks if any owned entities have been modified.
    /// </summary>
    public static bool HasChangedOwnedEntities(this Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        return entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            r.TargetEntry.State is EntityState.Added or EntityState.Modified);
    }
}
