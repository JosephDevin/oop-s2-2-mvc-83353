using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Models;

/// <summary>
/// Represents a specific food safety inspection event at a premises.
/// </summary>
public class Inspection
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign Key to the Premises being inspected.
    /// </summary>
    [Required]
    public int PremisesId { get; set; }
    public Premises? Premises { get; set; }

    [Required]
    [Display(Name = "Inspection Date")]
    [DataType(DataType.Date)]
    public DateTime InspectionDate { get; set; }

    /// <summary>
    /// Score awarded from 0 to 100. Higher is better.
    /// </summary>
    [Range(0, 100)]
    public int Score { get; set; }

    /// <summary>
    /// Outcome of the inspection: Pass or Fail.
    /// </summary>
    [Required]
    public string Outcome { get; set; } = "Pass";

    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// One-to-many relationship: One Inspection can trigger multiple FollowUp actions.
    /// </summary>
    public List<FollowUp> FollowUps { get; set; } = new();
}
