using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Data;
using oop_s2_2_mvc_83303.Models;
using Xunit;
using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Tests;

/// <summary>
/// Unit tests for the Food Safety Tracker application.
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

    private static IList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, true);
        return results;
    }

    // ── FollowUp Validation ──────────────────────────────────────────────────

    [Fact]
    public void FollowUp_ClosedWithoutDate_ValidationFails()
    {
        var followUp = new FollowUp { Status = "Closed", ClosedDate = null };
        var results = Validate(followUp);

        Assert.Contains(results, r => r.ErrorMessage == "Closed Date is required when status is Closed.");
    }

    [Fact]
    public void FollowUp_ClosedWithDate_ValidationPasses()
    {
        var followUp = new FollowUp
        {
            InspectionId = 1,
            DueDate = DateTime.Today,
            Status = "Closed",
            ClosedDate = DateTime.Today
        };
        var results = Validate(followUp);

        Assert.DoesNotContain(results, r => r.ErrorMessage == "Closed Date is required when status is Closed.");
    }

    [Fact]
    public void FollowUp_OpenWithoutClosedDate_ValidationPasses()
    {
        var followUp = new FollowUp
        {
            InspectionId = 1,
            DueDate = DateTime.Today,
            Status = "Open",
            ClosedDate = null
        };
        var results = Validate(followUp);

        Assert.DoesNotContain(results, r => r.ErrorMessage == "Closed Date is required when status is Closed.");
    }

    // ── Inspection Validation ────────────────────────────────────────────────

    [Fact]
    public void Inspection_ScoreAbove100_ValidationFails()
    {
        var inspection = new Inspection
        {
            PremisesId = 1,
            InspectionDate = DateTime.Today,
            Score = 101,
            Outcome = "Pass"
        };
        var results = Validate(inspection);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Inspection.Score)));
    }

    [Fact]
    public void Inspection_ScoreBelowZero_ValidationFails()
    {
        var inspection = new Inspection
        {
            PremisesId = 1,
            InspectionDate = DateTime.Today,
            Score = -1,
            Outcome = "Pass"
        };
        var results = Validate(inspection);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Inspection.Score)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Inspection_ScoreInRange_ValidationPasses(int score)
    {
        var inspection = new Inspection
        {
            PremisesId = 1,
            InspectionDate = DateTime.Today,
            Score = score,
            Outcome = "Pass"
        };
        var results = Validate(inspection);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(Inspection.Score)));
    }

    // ── Premises Validation ──────────────────────────────────────────────────

    [Fact]
    public void Premises_MissingName_ValidationFails()
    {
        var premises = new Premises
        {
            Name = "",
            Address = "1 Main St",
            Town = "Dublin",
            RiskRating = "Low"
        };
        var results = Validate(premises);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Premises.Name)));
    }

    [Fact]
    public void Premises_AllFieldsPopulated_ValidationPasses()
    {
        var premises = new Premises
        {
            Name = "Test Cafe",
            Address = "1 Main St",
            Town = "Dublin",
            RiskRating = "High"
        };
        var results = Validate(premises);

        Assert.Empty(results);
    }

    // ── Query / Data Logic ───────────────────────────────────────────────────

    [Fact]
    public async Task OverdueFollowUps_Query_FiltersCorrectItems()
    {
        using var context = GetInMemoryContext();
        var today = DateTime.Today;
        context.FollowUps.AddRange(
            new FollowUp { Status = "Open", DueDate = today.AddDays(-1) },   // overdue
            new FollowUp { Status = "Open", DueDate = today.AddDays(1) },    // future
            new FollowUp { Status = "Closed", DueDate = today.AddDays(-1), ClosedDate = today }  // closed
        );
        await context.SaveChangesAsync();

        var overdueCount = await context.FollowUps.CountAsync(f => f.Status == "Open" && f.DueDate < today);

        Assert.Equal(1, overdueCount);
    }

    [Fact]
    public async Task Dashboard_FailedInspectionsCount_IsAccurate()
    {
        using var context = GetInMemoryContext();
        var firstOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        context.Inspections.AddRange(
            new Inspection { InspectionDate = firstOfMonth, Outcome = "Fail" },
            new Inspection { InspectionDate = firstOfMonth, Outcome = "Pass" },
            new Inspection { InspectionDate = firstOfMonth.AddDays(-35), Outcome = "Fail" }  // previous month
        );
        await context.SaveChangesAsync();

        var failedCount = await context.Inspections.CountAsync(
            i => i.InspectionDate >= firstOfMonth && i.Outcome == "Fail");

        Assert.Equal(1, failedCount);
    }

    [Fact]
    public async Task Premises_WithHighRisk_CanBeFilteredFromDb()
    {
        using var context = GetInMemoryContext();
        context.Premises.AddRange(
            new Premises { Name = "A", Address = "1 St", Town = "Dublin", RiskRating = "High" },
            new Premises { Name = "B", Address = "2 St", Town = "Cork",   RiskRating = "Low" },
            new Premises { Name = "C", Address = "3 St", Town = "Galway", RiskRating = "High" }
        );
        await context.SaveChangesAsync();

        var highRiskCount = await context.Premises.CountAsync(p => p.RiskRating == "High");

        Assert.Equal(2, highRiskCount);
    }

    [Fact]
    public async Task Inspections_PassCount_IsAccurate()
    {
        using var context = GetInMemoryContext();
        context.Inspections.AddRange(
            new Inspection { InspectionDate = DateTime.Today, Score = 90, Outcome = "Pass" },
            new Inspection { InspectionDate = DateTime.Today, Score = 45, Outcome = "Fail" },
            new Inspection { InspectionDate = DateTime.Today, Score = 75, Outcome = "Pass" }
        );
        await context.SaveChangesAsync();

        var passCount = await context.Inspections.CountAsync(i => i.Outcome == "Pass");

        Assert.Equal(2, passCount);
    }

    [Fact]
    public async Task FollowUps_OpenStatus_CanBeRetrieved()
    {
        using var context = GetInMemoryContext();
        context.FollowUps.AddRange(
            new FollowUp { Status = "Open",   DueDate = DateTime.Today.AddDays(5) },
            new FollowUp { Status = "Closed", DueDate = DateTime.Today, ClosedDate = DateTime.Today },
            new FollowUp { Status = "Open",   DueDate = DateTime.Today.AddDays(10) }
        );
        await context.SaveChangesAsync();

        var openCount = await context.FollowUps.CountAsync(f => f.Status == "Open");

        Assert.Equal(2, openCount);
    }

    // ── Authorization / Reflection ───────────────────────────────────────────

    [Fact]
    public void InspectionsController_CreatePost_HasAuthorizeAttributeWithRoles()
    {
        var type = typeof(oop_s2_2_mvc_83303.Controllers.InspectionsController);
        var method = type.GetMethod("Create", new[] { typeof(Inspection) });

        var attr = method?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

        Assert.NotNull(attr);
        Assert.Equal("Admin,Inspector", attr.Roles);
    }

    [Fact]
    public void FollowUpsController_CreatePost_HasAuthorizeAttributeWithRoles()
    {
        var type = typeof(oop_s2_2_mvc_83303.Controllers.FollowUpsController);
        var method = type.GetMethod("Create", new[] { typeof(FollowUp) });

        var attr = method?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .FirstOrDefault() as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;

        Assert.NotNull(attr);
    }
}
