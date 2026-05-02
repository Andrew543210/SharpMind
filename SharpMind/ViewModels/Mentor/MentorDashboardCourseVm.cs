using SharpMind.Models;

namespace SharpMind.ViewModels.Mentor;

public class MentorDashboardCourseVm
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public CourseLevel Level { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ModulesCount { get; set; }
    public int StudentsCount { get; set; }
    public decimal AverageRating { get; set; }
}
