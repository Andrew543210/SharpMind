namespace SharpMind.ViewModels.Student;

public class StudentDashboardCourseVm
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public decimal ProgressPercent { get; set; }
}

