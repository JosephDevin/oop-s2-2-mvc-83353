using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Data;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Controllers;

[Authorize]
public class FollowUpsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FollowUpsController> _logger;

    public FollowUpsController(ApplicationDbContext context, ILogger<FollowUpsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(int inspectionId)
    {
        var inspection = await _context.Inspections.FindAsync(inspectionId);
        if (inspection == null) return NotFound();

        ViewBag.InspectionId = inspectionId;
        return View(new FollowUp { InspectionId = inspectionId, DueDate = DateTime.Now.AddDays(7) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(FollowUp followUp)
    {
        var inspection = await _context.Inspections.FindAsync(followUp.InspectionId);
        if (inspection == null) return NotFound();

        if (followUp.DueDate < inspection.InspectionDate)
        {
            _logger.LogWarning("Validation failure: creating FollowUp with due date {DueDate} before inspection date {InspectionDate}", 
                followUp.DueDate, inspection.InspectionDate);
            ModelState.AddModelError("DueDate", "Due date must be after the inspection date.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(followUp);
            await _context.SaveChangesAsync();
            _logger.LogInformation("FollowUp created. InspectionId: {InspectionId}, FollowUpId: {FollowUpId}", 
                followUp.InspectionId, followUp.Id);
            return RedirectToAction("Details", "Inspections", new { id = followUp.InspectionId });
        }
        return View(followUp);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var followUp = await _context.FollowUps.FindAsync(id);
        if (followUp == null) return NotFound();
        return View(followUp);
    }

    /// <summary>
    /// Task 4: Business Logic & Validation in Edit Action.
    /// Task 3: Log a Warning if closing without a ClosedDate.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, FollowUp followUp)
    {
        if (id != followUp.Id) return NotFound();

        // Task 4 & Task 3: Logic check for Closed status without ClosedDate
        if (followUp.Status == "Closed" && !followUp.ClosedDate.HasValue)
        {
            _logger.LogWarning("Business rule violation: Attempted to close FollowUp ID {Id} without a ClosedDate.", followUp.Id);
            ModelState.AddModelError("ClosedDate", "Closed Date is required when status is Closed.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(followUp);
                await _context.SaveChangesAsync();
                _logger.LogInformation("FollowUp updated. ID: {FollowUpId}, Status: {Status}", followUp.Id, followUp.Status);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FollowUps.Any(e => e.Id == followUp.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction("Details", "Inspections", new { id = followUp.InspectionId });
        }
        return View(followUp);
    }
}
