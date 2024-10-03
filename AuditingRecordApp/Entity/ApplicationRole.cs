using Microsoft.AspNetCore.Identity;

namespace AuditingRecordApp.Entity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole(string name) : base(name)
    {
    }
}