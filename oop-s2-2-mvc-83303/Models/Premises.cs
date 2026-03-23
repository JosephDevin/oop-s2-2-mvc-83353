using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Models;

/// <summary>
/// Represents a food premises location (e.g., a restaurant, cafe, or shop).
/// </summary>
public class Premises
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Premises Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string Town { get; set; } = string.Empty;

    /// <summary>
    /// Risk level assigned to the premises: Low, Medium, or High.
    /// Used for dashboard filtering.
    /// </summary>
    [Required]
    [Display(Name = "Risk Rating")]
    public string RiskRating { get; set; } = "Low"; 

    /// <summary>
    /// One-to-many relationship: One Premises can have multiple Inspections.
    /// </summary>
    public List<Inspection> Inspections { get; set; } = new();
}
