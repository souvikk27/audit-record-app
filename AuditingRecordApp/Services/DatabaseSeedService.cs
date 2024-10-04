using AuditingRecordApp.Data;
using AuditingRecordApp.Entity;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace AuditingRecordApp.Services;

public static class DatabaseSeedService
{
    public static async Task SeedDatabase(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (!await context.Offices.AnyAsync())
        {
            var offices = GenerateOfficeSeedData();
            await context.Offices.AddRangeAsync(offices);
            await context.SaveChangesAsync();

            var electricians = GenerateSeedData(offices);
            await context.Electricians.AddRangeAsync(electricians);
            await context.SaveChangesAsync();

            var repairs = GenerateRepairSeedData(electricians);
            await context.Repairs.AddRangeAsync(repairs);
            await context.SaveChangesAsync();
        }
    }

    private static List<Office> GenerateOfficeSeedData()
    {
        var offices = new List<Office>
        {
            Office.Create(Guid.NewGuid(),
                "Bright Sparks Inc.", "123 Main St, Anytown, " + "AN 12345",
                "555-0101"),
            Office.Create(Guid.NewGuid(), "Power Play Electric",
                "456 Oak Rd, Somewhere, SW 67890", "555-0202"),
            Office.Create(Guid.NewGuid(), "Voltage Experts",
                "789 Pine Ave, Elsewhere, EL 13579", "555-0303"),
            Office.Create(Guid.NewGuid(), "Circuit Masters",
                "321 Elm Blvd, Nowhere, NW 24680", "555-0404"),
            Office.Create(Guid.NewGuid(), "Watt's Up Electric",
                "654 Birch Ln, Everywhere, EV 97531", "555-0505"),
            Office.Create(Guid.NewGuid(), "Amp'd Services",
                "987 Cedar St, Anyplace, AP 86420", "555-0606"),
            Office.Create(Guid.NewGuid(), "Ohm Sweet Ohm",
                "147 Maple Dr, Someplace, SP 75319", "555-0707"),
            Office.Create(Guid.NewGuid(), "Current Solutions",
                "258 Willow Way, Othertown, OT 95135", "555-0808"),
            Office.Create(Guid.NewGuid(), "Shock & Awe Electric",
                "369 Spruce Ct, Thatplace, TP 15973", "555-0909"),
            Office.Create(Guid.NewGuid(), "Lightspeed Electric",
                "741 Ash Ave, ThisCity, TC 35791", "555-1010")
        };

        return offices;
    }

    public static List<Electrician> GenerateSeedData(List<Office> offices)
    {
        var electricians = new List<Electrician>();
        var faker = new Faker();

        foreach (var office in offices)
        {
            int numberOfElectricians = faker.Random.Int(2, 4);

            for (int i = 0; i < numberOfElectricians; i++)
            {
                var firstName = faker.Name.FirstName();
                var lastName = faker.Name.LastName();

                electricians.Add(Electrician.Create(
                    Guid.NewGuid(),
                    $"{firstName} {lastName}",
                    faker.Phone.PhoneNumber("###-###-####"),
                    faker.Internet.Email(firstName, lastName, office.Name.ToLower().Replace(" ", "")),
                    faker.Random.Bool(),
                    office
                ));
            }
        }

        return electricians;
    }

    public static List<Repair> GenerateRepairSeedData(List<Electrician> electricians)
    {
        var repairList = new List<Repair>();
        var faker = new Faker();

        foreach (var electrician in electricians)
        {
            int numberOfRepairs = faker.Random.Int(2, 4);

            for (int i = 0; i < numberOfRepairs; i++)
            {
                repairList.Add(Repair.Create(
                    Guid.NewGuid(),
                    faker.Company.CatchPhrase(),
                    DateTime.UtcNow,
                    electrician,
                    RepairStatus.InProgress
                ));
            }
        }

        return repairList;
    }
}