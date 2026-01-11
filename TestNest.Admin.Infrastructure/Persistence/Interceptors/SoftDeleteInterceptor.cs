using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TestNest.Admin.SharedLibrary.Common.BaseEntity;

namespace TestNest.Admin.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that converts hard deletes to soft deletes for entities implementing ISoftDeletable.
/// </summary>
public sealed class SoftDeleteInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateSoftDeleteEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void UpdateSoftDeleteEntities(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var entries = context.ChangeTracker.Entries<ISoftDeletable>();
        var utcNow = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convert hard delete to soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedOnUtc = utcNow;
                entry.Entity.DeletedBy = GetCurrentUser(); // TODO: Replace with actual user from HttpContext
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
