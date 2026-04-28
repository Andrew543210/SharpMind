using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class Test
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    public int ModuleId { get; set; }
    public CourseModule? Module { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<TestResult> Results { get; set; } = new List<TestResult>();
}

