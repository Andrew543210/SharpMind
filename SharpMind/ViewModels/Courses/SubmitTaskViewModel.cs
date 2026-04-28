using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Courses;

public class SubmitTaskViewModel
{
    public int TaskId { get; set; }
    public int CourseId { get; set; }

    [Required, StringLength(4000)]
    public string AnswerText { get; set; } = string.Empty;
}

