using AuditingRecordApp.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AuditingRecordApp.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;

    public AuditSaveChangesInterceptor(
        ILogger<AuditSaveChangesInterceptor> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return result;

        var entries = context.ChangeTracker.Entries<AuditableBaseEntity>()
            .Where(e => e.State is
                EntityState.Added or
                EntityState.Modified or
                EntityState.Deleted)
            .ToList();


        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}