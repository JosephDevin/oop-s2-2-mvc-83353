using oop_s2_2_mvc_83303.Models;

namespace oop_s2_2_mvc_83303.Models;

public class DashboardViewModel
{
    public int TotalInspectionsThisMonth { get; set; }
    public int FailedInspectionsThisMonth { get; set; }
    public int OverdueFollowUps { get; set; }
    
    public List<Inspection> RecentInspections { get; set; } = new();
    
    // Filters
    public string? SelectedTown { get; set; }
    public string? SelectedRiskRating { get; set; }
    
    public List<string> Towns { get; set; } = new();
    public List<string> RiskRatings { get; set; } = new() { "Low", "Medium", "High" };
}
