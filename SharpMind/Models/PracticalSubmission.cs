using System.ComponentModel.DataAnnotations;
using SharpMind.Models.Identity;

namespace SharpMind.Models;

public class PracticalSubmission
{
    public int Id { get; set; }

    public int TaskId { get; set; }
    public PracticalTask? Task { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    [Required, StringLength(4000)]
    public string AnswerText { get; set; } = string.Empty;

    [StringLength(400)]
    public string? FilePath { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    public int? Grade { get; set; }

    [StringLength(1000)]
    public string? MentorComment { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
}

