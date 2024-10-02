namespace AuditingRecordApp.Entity;
#nullable disable

public sealed class Office : AuditableBaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    
    public ICollection<Electrician> Electricians { get; set; }
}