using SharpMind.Models;

namespace SharpMind.ViewModels.Student;

public class StudentDashboardViewModel
{
    public List<StudentDashboardCourseVm> ActiveCourses { get; set; } = new();
    public List<StudentDashboardCourseVm> CompletedCourses { get; set; } = new();
    public List<Certificate> Certificates { get; set; } = new();
}
