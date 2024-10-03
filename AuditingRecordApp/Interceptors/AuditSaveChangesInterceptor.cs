using AuditingRecordApp.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using AuditingRecordApp.Services;

namespace AuditingRecordApp.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly AuditService _auditService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;

    public AuditSaveChangesInterceptor(
        ILogger<AuditSaveChangesInterceptor> logger,
        IHttpContextAccessor httpContextAccessor,
        AuditService auditService)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _auditService = auditService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
        InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return result;

        var entries = context.ChangeTracker.Entries<AuditableBaseEntity>()
            .Where(e => e.State is
                EntityState.Added or
                EntityState.Modified or
                EntityState.Deleted)
            .ToList();

        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            _logger.LogWarning("Current user ID is empty cannot create audit trails");
            return result;
        }

        foreach (var entry in entries)
        {
            var entityName = entry.Entity.GetType().Name;
            var primaryKey = entry.Properties
                .FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString();

            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                _logger.LogWarning($"Primary key is null or empty for entity {entityName}");
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUserId.ToString();
                entry.Entity.CreatedAt = DateTime.UtcNow;
                await _auditService.CreateAuditTrail(TrailType.Create, entry.Entity, primaryKey);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedBy = currentUserId.ToString();
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                await _auditService.CreateAuditTrail(TrailType.Update, entry.Entity, primaryKey);
            }
            else if (entry.State == EntityState.Deleted)
            {
                await _auditService.CreateAuditTrail(TrailType.Delete, entry.Entity, primaryKey);
            }
        }

        return result;
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out Guid parsedGuid) ? parsedGuid : Guid.Empty;
    }
}