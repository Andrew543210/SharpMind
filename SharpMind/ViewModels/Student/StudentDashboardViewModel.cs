using SharpMind.Models;

namespace SharpMind.ViewModels.Student;

public class StudentDashboardViewModel
{
    public List<StudentDashboardCourseVm> Courses { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
}
