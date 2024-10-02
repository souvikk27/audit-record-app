namespace AuditingRecordApp.Entity;
#nullable disable
public sealed class Electrician : AuditableBaseEntity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public bool IsAvailable { get; set; }
    
    public Office Office { get; set; }
    public ICollection<Repair> Repairs { get; set; }
}