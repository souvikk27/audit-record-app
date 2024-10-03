using System.Security.Claims;
using AuditingRecordApp.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

#nullable disable

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

        var userName = GetUserName();

        if (userName is null or "Anonymous")
        {
            _logger.LogError("User name not found or anonymous user");
            userName = "system";
        }


        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = userName;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userName;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private string GetUserName()
    {
        return _httpContextAccessor.HttpContext!.User.Identity is not { IsAuthenticated: true }
            ? "Anonymous"
            : _httpContextAccessor.HttpContext?.User?.Identities
                .FirstOrDefault()?
                .Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?
                .Value;
    }
}