using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Models;

/// <summary>
/// Represents a required action following an inspection (e.g., a re-visit).
/// Implements IValidatableObject for complex cross-field validation.
/// </summary>
public class FollowUp : IValidatableObject
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign Key linking to the parent Inspection.
    /// </summary>
    [Required]
    public int InspectionId { get; set; }
    public Inspection? Inspection { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Current status: Open or Closed.
    /// </summary>
    [Required]
    public string Status { get; set; } = "Open";

    /// <summary>
    /// The date the follow-up was actually addressed. 
    /// Must be provided if Status is 'Closed'.
    /// </summary>
    [Display(Name = "Closed Date")]
    [DataType(DataType.Date)]
    public DateTime? ClosedDate { get; set; }

    /// <summary>
    /// Custom validation logic to ensure data integrity.
    /// Requirement: Cannot close a follow-up without a ClosedDate.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Status == "Closed" && !ClosedDate.HasValue)
        {
            yield return new ValidationResult(
                "Closed Date is required when status is Closed.", 
                new[] { nameof(ClosedDate) }
            );
        }
    }
}
