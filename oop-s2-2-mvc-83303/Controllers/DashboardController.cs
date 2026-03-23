using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Data;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Task 2: Implement Dashboard Logic with required statistics and filtering.
    /// Task 3: Log access with filter parameters.
    /// </summary>
    public async Task<IActionResult> Index(string? town, string? riskRating)
    {
        // Task 3: Structured Logging of dashboard access
        _logger.LogInformation("Dashboard accessed. Filters - Town: {Town}, RiskRating: {RiskRating}", town ?? "All", riskRating ?? "All");

        var now = DateTime.Today;
        var firstDayOfMonth = new DateTime(now.Year, now.Month, 1);

        // Task 2: Calculate Statistics using EF Core
        var totalInspectionsThisMonth = await _context.Inspections
            .CountAsync(i => i.InspectionDate >= firstDayOfMonth);

        var failedInspectionsThisMonth = await _context.Inspections
            .CountAsync(i => i.InspectionDate >= firstDayOfMonth && i.Outcome == "Fail");

        var overdueFollowUps = await _context.FollowUps
            .CountAsync(f => f.Status == "Open" && f.DueDate < now);

        // Filtering logic for the detail list
        var query = _context.Inspections.Include(i => i.Premises).AsQueryable();

        if (!string.IsNullOrEmpty(town))
        {
            query = query.Where(i => i.Premises!.Town == town);
        }

        if (!string.IsNullOrEmpty(riskRating))
        {
            query = query.Where(i => i.Premises!.RiskRating == riskRating);
        }

        var viewModel = new DashboardViewModel
        {
            TotalInspectionsThisMonth = totalInspectionsThisMonth,
            FailedInspectionsThisMonth = failedInspectionsThisMonth,
            OverdueFollowUps = overdueFollowUps,
            RecentInspections = await query.OrderByDescending(i => i.InspectionDate).Take(10).ToListAsync(),
            Towns = await _context.Premises.Select(p => p.Town).Distinct().ToListAsync(),
            SelectedTown = town,
            SelectedRiskRating = riskRating
        };

        return View(viewModel);
    }
}
