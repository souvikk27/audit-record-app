﻿using System.Text.Json.Serialization;

namespace AuditingRecordApp.Entity;
#nullable disable

public sealed class Office : AuditableBaseEntity
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }

    [JsonIgnore]
    public ICollection<Electrician> Electricians { get; set; }

    public static Office Create(
        Guid id,
        string name, 
        string address, 
        string phone)
    {
        return new Office
        {
            Id = id,
            Name = name,
            Address = address,
            Phone = phone
        };
    }
}