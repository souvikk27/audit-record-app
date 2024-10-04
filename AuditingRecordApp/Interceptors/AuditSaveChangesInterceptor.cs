using AuditingRecordApp.Entity;
using AuditingRecordApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

#nullable disable

namespace AuditingRecordApp.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;
    private ICurrentSessionProvider _currentSessionProvider;

    public AuditSaveChangesInterceptor(
        ILogger<AuditSaveChangesInterceptor> logger,
        ICurrentSessionProvider currentSessionProvider)
    {
        _logger = logger;
        _currentSessionProvider = currentSessionProvider;
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

        var userName = _currentSessionProvider.GetUserId().ToString();

        if (userName is null or "Anonymous")
        {
            _logger.LogError("User name not found or anonymous user");
            userName = "system";
        }


        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userName;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userName;
                    break;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}