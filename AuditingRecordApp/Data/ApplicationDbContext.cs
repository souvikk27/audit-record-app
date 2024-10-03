using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuditingRecordApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
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
}