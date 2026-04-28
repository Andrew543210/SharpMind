using System.ComponentModel.DataAnnotations;
using SharpMind.Models;

namespace SharpMind.ViewModels.Mentor;

public class CreateCourseViewModel
{
    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public CourseTopic Topic { get; set; }

    [Required]
    public CourseLevel Level { get; set; }

    [Range(0, 99999)]
    public decimal Price { get; set; }

    public bool IsPublished { get; set; } = true;
}

