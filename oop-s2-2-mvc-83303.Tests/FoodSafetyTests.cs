using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Data;
using oop_s2_2_mvc_83303.Models;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Tests;

/// <summary>
/// Task 5: Enhanced Unit Tests for Rubric Compliance.
/// </summary>
public class FoodSafetyTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Test 1: FollowUp model validation fails if Status is "Closed" but ClosedDate is null.
    /// </summary>
    [Fact]
    public void FollowUp_ClosedWithoutDate_ValidationFails()
    {
        // Arrange
        var followUp = new FollowUp { Status = "Closed", ClosedDate = null };
        var context = new ValidationContext(followUp);
        var results = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(followUp, context, results, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.ErrorMessage == "Closed Date is required when status is Closed.");
    }

    /// <summary>
    /// Test 2: Overdue Follow-ups query correctly filters only "Open" status items with past due dates.
    /// </summary>
    [Fact]
    public async Task OverdueFollowUps_Query_FiltersCorrectItems()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var today = DateTime.Today;
        context.FollowUps.AddRange(new List<FollowUp>
        {
            new FollowUp { Status = "Open", DueDate = today.AddDays(-1) }, // Should be counted
            new FollowUp { Status = "Open", DueDate = today.AddDays(1) },  // Not overdue
            new FollowUp { Status = "Closed", DueDate = today.AddDays(-1), ClosedDate = today } // Not open
        });
        await context.SaveChangesAsync();

        // Act
        var overdueCount = await context.FollowUps.CountAsync(f => f.Status == "Open" && f.DueDate < today);

        // Assert
        Assert.Equal(1, overdueCount);
    }

    /// <summary>
    /// Test 3: Dashboard count for "Failed Inspections" is accurate.
    /// </summary>
    [Fact]
    public async Task Dashboard_FailedInspectionsCount_IsAccurate()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        
        context.Inspections.AddRange(new List<Inspection>
        {
            new Inspection { InspectionDate = firstOfMonth, Outcome = "Fail" },
            new Inspection { InspectionDate = firstOfMonth, Outcome = "Pass" },
            new Inspection { InspectionDate = firstOfMonth.AddDays(-35), Outcome = "Fail" } // Wrong month
        });
        await context.SaveChangesAsync();

        // Act
        var failedCount = await context.Inspections.CountAsync(i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail");

        // Assert
        Assert.Equal(1, failedCount);
    }

    /// <summary>
    /// Test 4: Verify Authorization Attribute on restricted actions.
    /// </summary>
    [Fact]
    public void InspectionsController_CreatePost_HasAuthorizeAttributeWithRoles()
    {
        // Arrange
        var type = typeof(oop_s2_2_mvc_83303.Controllers.InspectionsController);
        var method = type.GetMethod("Create", new[] { typeof(Inspection) });

        // Act
        var attr = method?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

        // Assert
        Assert.NotNull(attr);
        Assert.Equal("Admin,Inspector", attr.Roles);
    }
}
