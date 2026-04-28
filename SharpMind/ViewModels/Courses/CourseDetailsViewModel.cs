using SharpMind.Models;

namespace SharpMind.ViewModels.Courses;

public class CourseDetailsViewModel
{
    public Course Course { get; set; } = null!;
    public EnrollmentStatus? EnrollmentStatus { get; set; }
    public decimal ProgressPercent { get; set; }
}

