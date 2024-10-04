using Newtonsoft.Json;

namespace AuditingRecordApp.Entity;
#nullable disable
public sealed class Electrician : AuditableBaseEntity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public bool IsAvailable { get; set; }

    [JsonIgnore] public Office Office { get; set; }

    [JsonIgnore] public ICollection<Repair> Repairs { get; set; }

    public static Electrician Create(
        Guid id, 
        string name, 
        string phone, 
        string email, 
        bool isAvailable, 
        Office office)
    {
        return new Electrician
        {
            Id = id,
            Name = name,
            Phone = phone,
            Email = email,
            IsAvailable = isAvailable,
            Office = office
        };
    }
}