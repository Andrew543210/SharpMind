using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class PracticalTask
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    public int ModuleId { get; set; }
    public CourseModule? Module { get; set; }

    public ICollection<PracticalSubmission> Submissions { get; set; } = new List<PracticalSubmission>();
}

