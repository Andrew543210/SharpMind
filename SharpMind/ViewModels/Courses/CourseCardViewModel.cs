using SharpMind.Models;

namespace SharpMind.ViewModels.Courses;

public class CourseCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CourseTopic Topic { get; set; }
    public CourseLevel Level { get; set; }
    public decimal Price { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public int Popularity { get; set; }
}

