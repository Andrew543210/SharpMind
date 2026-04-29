namespace SharpMind.ViewModels.Courses;

public class CourseRatingViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public List<CourseRatingEntryVm> Entries { get; set; } = new();
}

