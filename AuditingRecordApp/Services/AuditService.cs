using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using System.Security.Claims;

namespace AuditingRecordApp.Services;

public class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task CreateAuditTrail<T>(TrailType trailType, T entity, string primaryKey)
        where T : AuditableBaseEntity
    {
        var entityName = typeof(T).Name;
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        if (trailType == TrailType.Update)
        {
            oldValues["LastModifiedBy"] = entity.UpdatedBy;
            oldValues["LastModifiedOn"] = entity.UpdatedAt;

            // Assuming you have a way to get the current user's ID
            var currentUserId = GetCurrentUserId();
            newValues["LastModifiedBy"] = currentUserId;
            newValues["LastModifiedOn"] = DateTime.UtcNow;

            // Update the entity
            entity.UpdatedBy = currentUserId.ToString();
            entity.UpdatedAt = DateTime.UtcNow;
        }
        else if (trailType == TrailType.Create)
        {
            newValues["CreatedBy"] = entity.CreatedBy;
            newValues["CreatedOn"] = entity.CreatedAt;
        }

        var auditTrail = new AuditTrails
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(entity.UpdatedBy ?? entity.CreatedBy),
            TrailType = trailType,
            EntityName = entityName,
            PrimaryKey = primaryKey,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedColumns = GetChangedColumns(oldValues, newValues),
            DateUtc = DateTime.UtcNow
        };

        _context.AuditTrails.Add(auditTrail);
        await _context.SaveChangesAsync();
    }

    private Guid? GetCurrentUserId()
    {
        var userId = _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userId, out Guid parsedGuid))
        {
            return parsedGuid;
        }

        return null;
    }

    private List<string> GetChangedColumns(Dictionary<string, object?> oldValues, Dictionary<string, object?> newValues)
    {
        var changedColumns = new List<string>();
        foreach (var key in newValues.Keys.Union(oldValues.Keys))
        {
            if (!oldValues.TryGetValue(key, out var oldValue))
            {
                changedColumns.Add(key);
            }
            else if (!newValues.TryGetValue(key, out var newValue))
            {
                changedColumns.Add(key);
            }
            else if (!Equals(oldValue, newValue))
            {
                changedColumns.Add(key);
            }
        }

        return changedColumns;
    }
}