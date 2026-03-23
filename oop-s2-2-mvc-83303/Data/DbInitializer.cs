using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Data;

/// <summary>
/// Static helper to seed the database with initial roles, users, and test data.
/// </summary>
public static class DbInitializer
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        logger.LogInformation("Ensuring database is created and seeded.");
        
        // Ensures the SQLite database file is created and the schema is applied.
        context.Database.EnsureCreated();

        // 1. Seed Roles (Admin, Inspector, Viewer)
        string[] roleNames = { "Admin", "Inspector", "Viewer" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Seed Default Users for each role
        await CreateUser(userManager, "admin@test.com", "Admin123!", "Admin");
        await CreateUser(userManager, "inspector@test.com", "Inspector123!", "Inspector");
        await CreateUser(userManager, "viewer@test.com", "Viewer123!", "Viewer");

        // 3. Seed Domain Data if empty
        if (!context.Premises.Any())
        {
            SeedApplicationData(context);
        }
    }

    private static async Task CreateUser(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }

    private static void SeedApplicationData(ApplicationDbContext context)
    {
        var towns = new[] { "Dublin", "Cork", "Galway" };
        var premisesList = new List<Premises>();

        // Seed 12 Premises across 3 towns
        for (int i = 1; i <= 12; i++)
        {
            premisesList.Add(new Premises
            {
                Name = $"Restaurant {i}",
                Address = $"{i} Main St",
                Town = towns[i % 3],
                RiskRating = i % 3 == 0 ? "High" : (i % 3 == 1 ? "Medium" : "Low")
            });
        }
        context.Premises.AddRange(premisesList);
        context.SaveChanges();

        // Seed 25 Inspections at random premises
        var inspections = new List<Inspection>();
        var random = new Random();
        for (int i = 1; i <= 25; i++)
        {
            var pId = premisesList[random.Next(premisesList.Count)].Id;
            var date = DateTime.Now.AddDays(-random.Next(1, 60));
            var score = random.Next(50, 100);
            inspections.Add(new Inspection
            {
                PremisesId = pId,
                InspectionDate = date,
                Score = score,
                Outcome = score > 70 ? "Pass" : "Fail",
                Notes = $"Standard inspection {i}"
            });
        }
        context.Inspections.AddRange(inspections);
        context.SaveChanges();

        // Seed 10 Follow-ups (some overdue, some closed)
        for (int i = 1; i <= 10; i++)
        {
            var insp = inspections[random.Next(inspections.Count)];
            var isOverdue = i % 3 == 0;
            var isClosed = i % 2 == 0;

            context.FollowUps.Add(new FollowUp
            {
                InspectionId = insp.Id,
                DueDate = isOverdue ? DateTime.Now.AddDays(-5) : DateTime.Now.AddDays(5),
                Status = isClosed ? "Closed" : "Open",
                ClosedDate = isClosed ? DateTime.Now.AddDays(-1) : null
            });
        }
        context.SaveChanges();
    }
}
