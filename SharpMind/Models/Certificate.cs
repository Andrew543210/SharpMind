using System.ComponentModel.DataAnnotations;
using SharpMind.Models.Identity;

namespace SharpMind.Models;

public class Certificate
{
    public int Id { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public Guid UniqueNumber { get; set; } = Guid.NewGuid();

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string CourseName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string PlatformName { get; set; } = "SharpMind";
}
