using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Mentor;

public class GradeSubmissionViewModel
{
    public int SubmissionId { get; set; }

    [Range(0, 100)]
    public int Grade { get; set; }

    [StringLength(1000)]
    public string? MentorComment { get; set; }
}

