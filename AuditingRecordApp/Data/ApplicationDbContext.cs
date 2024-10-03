using AuditingRecordApp.Entity;
using AuditingRecordApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AuditingRecordApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly ICurrentSessionProvider _currentSessionProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentSessionProvider currentSessionProvider)
        : base(options)
    {
        _currentSessionProvider = currentSessionProvider;
    }

    public DbSet<AuditTrails> AuditTrails { get; set; }
    public DbSet<Office> Offices { get; set; }
    public DbSet<Electrician> Electricians { get; set; }
    public DbSet<Repair> Repairs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ApplicationUser>().ToTable("users");
        modelBuilder.Entity<ApplicationRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>(entity => { entity.ToTable("user_roles"); });
        modelBuilder.Entity<IdentityUserClaim<string>>(entity => { entity.ToTable("user_claims"); });
        modelBuilder.Entity<IdentityUserLogin<string>>(entity => { entity.ToTable("user_logins"); });
        modelBuilder.Entity<IdentityRoleClaim<string>>(entity => { entity.ToTable("role_claims"); });
        modelBuilder.Entity<IdentityUserToken<string>>(entity => { entity.ToTable("user_tokens"); });
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var userId = _currentSessionProvider.GetUserId();

        SetAuditableProperties(userId);

        var auditEntries = HandleAuditingBeforeSaveChanges(userId).ToList();
        if (auditEntries.Count > 0)
        {
            await AuditTrails.AddRangeAsync(auditEntries, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditTrails> HandleAuditingBeforeSaveChanges(Guid? userId)
    {
        var auditableEntries = ChangeTracker.Entries<AuditableBaseEntity>()
            .Where(x => x.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
            .Select(x => CreateTrailEntry(userId, x))
            .ToList();

        return auditableEntries;
    }

    private static AuditTrails CreateTrailEntry(Guid? userId, EntityEntry<AuditableBaseEntity> entry)
    {
        var trailEntry = new AuditTrails
        {
            Id = Guid.NewGuid(),
            EntityName = entry.Entity.GetType().Name,
            UserId = userId,
            DateUtc = DateTime.UtcNow
        };

        SetAuditTrailPropertyValues(entry, trailEntry);
        SetAuditTrailNavigationValues(entry, trailEntry);
        SetAuditTrailReferenceValues(entry, trailEntry);

        return trailEntry;
    }

    /// <summary>
    /// Sets auditable properties for entities that are inherited from <see cref="IAuditableEntity"/>
    /// </summary>
    /// <param name="userId">User identifier that performed an action</param>
    /// <returns>Collection of auditable entities</returns>
    private void SetAuditableProperties(Guid? userId)
    {
        const string systemSource = "system";
        foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = userId?.ToString() ?? systemSource;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedBy = userId?.ToString() ?? systemSource;
                    break;
            }
        }
    }

    /// <summary>
    /// Sets column values to audit trail entity
    /// </summary>
    /// <param name="entry">Current entity entry ef core model</param>
    /// <param name="trailEntry">Audit trail entity</param>
    private static void SetAuditTrailPropertyValues(EntityEntry entry, AuditTrails trailEntry)
    {
        // Skip temp fields (that will be assigned automatically by ef core engine, for example: when inserting an entity
        foreach (var property in entry.Properties.Where(x => !x.IsTemporary))
        {
            if (property.Metadata.IsPrimaryKey())
            {
                trailEntry.PrimaryKey = property.CurrentValue?.ToString();
                continue;
            }

            // Filter properties that should not appear in the audit list
            if (property.Metadata.Name.Equals("PasswordHash"))
            {
                continue;
            }

            SetAuditTrailPropertyValue(entry, trailEntry, property);
        }
    }

    /// <summary>
    /// Sets a property value to the audit trail entity
    /// </summary>
    /// <param name="entry">Current entity entry ef core model</param>
    /// <param name="trailEntry">Audit trail entity</param>
    /// <param name="property">Entity property ef core model</param>
    private static void SetAuditTrailPropertyValue(EntityEntry entry, AuditTrails trailEntry, PropertyEntry property)
    {
        var propertyName = property.Metadata.Name;

        switch (entry.State)
        {
            case EntityState.Added:
                trailEntry.TrailType = TrailType.Create;
                trailEntry.NewValues[propertyName] = property.CurrentValue;

                break;

            case EntityState.Deleted:
                trailEntry.TrailType = TrailType.Delete;
                trailEntry.OldValues[propertyName] = property.OriginalValue;

                break;

            case EntityState.Modified:
                if (property.IsModified && (property.OriginalValue is null ||
                                            !property.OriginalValue.Equals(property.CurrentValue)))
                {
                    trailEntry.ChangedColumns.Add(propertyName);
                    trailEntry.TrailType = TrailType.Update;
                    trailEntry.OldValues[propertyName] = property.OriginalValue;
                    trailEntry.NewValues[propertyName] = property.CurrentValue;
                }

                break;
        }

        if (trailEntry.ChangedColumns.Count > 0)
        {
            trailEntry.TrailType = TrailType.Update;
        }
    }

    private static void SetAuditTrailNavigationValues(EntityEntry entry, AuditTrails trailEntry)
    {
        foreach (var navigation in entry.Navigations.Where(x => x.Metadata.IsCollection && x.IsModified))
        {
            if (navigation.CurrentValue is not IEnumerable<object> enumerable)
            {
                continue;
            }

            var collection = enumerable.ToList();
            if (collection.Count == 0)
            {
                continue;
            }

            var navigationName = collection.First().GetType().Name;
            trailEntry.ChangedColumns.Add(navigationName);
        }
    }

    private static void SetAuditTrailReferenceValues(EntityEntry entry, AuditTrails trailEntry)
    {
        foreach (var reference in entry.References.Where(x => x.IsModified))
        {
            var referenceName = reference.EntityEntry.Entity.GetType().Name;
            trailEntry.ChangedColumns.Add(referenceName);
        }
    }
}