using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Models;

/// Represents a specific food safety inspection event at a premises.
public class Inspection
{
    public int Id { get; set; }

    [Required]
    public int PremisesId { get; set; }
    public Premises? Premises { get; set; }

    [Required]
    [Display(Name = "Inspection Date")]
    [DataType(DataType.Date)]
    public DateTime InspectionDate { get; set; }

    /// Score awarded from 0 to 100. Higher is better.
    [Range(0, 100)]
    public int Score { get; set; }

    /// Outcome of the inspection: Pass or Fail.
    [Required]
    public string Outcome { get; set; } = "Pass";

    public string Notes { get; set; } = string.Empty;

    /// One-to-many relationship: One Inspection can trigger multiple FollowUp actions.
    public List<FollowUp> FollowUps { get; set; } = new();
}
