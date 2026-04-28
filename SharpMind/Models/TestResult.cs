using System.ComponentModel.DataAnnotations;
using SharpMind.Models.Identity;

namespace SharpMind.Models;

public class TestResult
{
    public int Id { get; set; }

    public int TestId { get; set; }
    public Test? Test { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    [Range(0, 100)]
    public decimal ScorePercent { get; set; }

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

