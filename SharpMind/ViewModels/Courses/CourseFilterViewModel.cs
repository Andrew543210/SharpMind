using SharpMind.Models;

namespace SharpMind.ViewModels.Courses;

public class CourseFilterViewModel
{
    public string? Search { get; set; }
    public CourseTopic? Topic { get; set; }
    public CourseLevel? Level { get; set; }
    public decimal? PriceFrom { get; set; }
    public decimal? PriceTo { get; set; }
    public CourseSortType SortBy { get; set; } = CourseSortType.Name;
    public bool SortDescending { get; set; }
}

