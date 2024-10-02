namespace AuditingRecordApp.Entity;
#nullable disable
public sealed class Repair : AuditableBaseEntity
{
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public Electrician Electrician { get; set; }
    public RepairStatus Status { get; set; }
}

public enum RepairStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}