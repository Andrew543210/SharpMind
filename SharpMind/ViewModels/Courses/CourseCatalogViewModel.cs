namespace SharpMind.ViewModels.Courses;

public class CourseCatalogViewModel
{
    public CourseFilterViewModel Filter { get; set; } = new();
    public List<CourseCardViewModel> Courses { get; set; } = [];
}

