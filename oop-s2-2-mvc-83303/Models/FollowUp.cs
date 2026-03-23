using System.ComponentModel.DataAnnotations;

namespace oop_s2_2_mvc_83303.Models;

/// Represents a required action following an inspection (e.g., a re-visit).
public class FollowUp : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    public int InspectionId { get; set; }
    public Inspection? Inspection { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    // Open or Closed.
    [Required]
    public string Status { get; set; } = "Open";

    // The date the follow-up was actually addressed. 
    [Display(Name = "Closed Date")]
    [DataType(DataType.Date)]
    public DateTime? ClosedDate { get; set; }

    // Custom validation logic to ensure data integrity.
    // Requirement: Cannot close a follow-up without a ClosedDate.
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
