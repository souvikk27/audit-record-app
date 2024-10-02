namespace AuditingRecordApp.Entity;
#nullable disable

public abstract class AuditableBaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    protected AuditableBaseEntity()
    {
    }

    protected AuditableBaseEntity(Guid id)
    {
        Id = id;
    }
}