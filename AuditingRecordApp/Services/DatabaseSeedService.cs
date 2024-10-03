using AuditingRecordApp.Entity;
using Microsoft.AspNetCore.Identity;

namespace AuditingRecordApp.Services;

public static class DatabaseSeedService
{
    public static async Task SeedDefaultUserAsync(UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // Create default roles if they don't exist
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new ApplicationRole(role));
            }
        }

        // Create default admin user if it doesn't exist
        var adminUser = await userManager.FindByEmailAsync("admin@example.com");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "souvikk27",
                Email = "souvik@coreapp.com",
                EmailConfirmed = true,
                FirstName = "Souvik",
                LastName = "Kundu",
                IsAdmin = true
            };

            var result = await userManager.CreateAsync(adminUser, "@Tavish!12");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                var exceptions = result.Errors.Select(e => new Exception(e.Description));
                throw new AggregateException("Failed to create default admin user.", exceptions);
            }
        }
    }
}