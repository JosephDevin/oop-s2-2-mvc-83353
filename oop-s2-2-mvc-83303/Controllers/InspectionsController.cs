using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_2_mvc_83303.Data;
using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Controllers;

[Authorize]
public class InspectionsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<InspectionsController> _logger;

    public InspectionsController(ApplicationDbContext context, ILogger<InspectionsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Inspections.Include(i => i.Premises).ToListAsync());
    }

    [Authorize(Roles = "Admin,Inspector")]
    public IActionResult Create()
    {
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Create(Inspection inspection)
    {
        if (ModelState.IsValid)
        {
            _context.Add(inspection);
            await _context.SaveChangesAsync();
            
            // Task 3: Structured Logging on Create
            _logger.LogInformation("Inspection created. ID: {InspectionId}, PremisesId: {PremisesId}, User: {User}", 
                inspection.Id, inspection.PremisesId, User.Identity?.Name);
                
            return RedirectToAction(nameof(Index));
        }
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var inspection = await _context.Inspections.FindAsync(id);
        if (inspection == null) return NotFound();
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Inspector")]
    public async Task<IActionResult> Edit(int id, Inspection inspection)
    {
        if (id != inspection.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(inspection);
                await _context.SaveChangesAsync();
                
                // Task 3: Structured Logging on Update
                _logger.LogInformation("Inspection updated. ID: {InspectionId}, PremisesId: {PremisesId}, User: {User}", 
                    inspection.Id, inspection.PremisesId, User.Identity?.Name);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inspections.Any(e => e.Id == inspection.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["PremisesId"] = new SelectList(_context.Premises, "Id", "Name", inspection.PremisesId);
        return View(inspection);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        
        _logger.LogInformation("Viewing details for inspection ID: {Id}", id);

        var inspection = await _context.Inspections
            .Include(i => i.Premises)
            .Include(i => i.FollowUps)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (inspection == null)
        {
            _logger.LogWarning("Inspection with ID {Id} not found.", id);
            return NotFound();
        }

        return View(inspection);
    }
}
