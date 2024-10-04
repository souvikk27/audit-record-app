namespace AuditingRecordApp.Entity;
#nullable disable
public sealed class Repair : AuditableBaseEntity
{
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public Electrician Electrician { get; set; }
    public RepairStatus Status { get; set; }

    public static Repair Create(
        Guid id, 
        string description, 
        DateTime date, 
        Electrician electrician, 
        RepairStatus status)
    {
        return new Repair
        {
            Id = id,
            Description = description,
            Date = date,
            Electrician = electrician,
            Status = status
        };
    }
}

public enum RepairStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}