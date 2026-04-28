using SharpMind.ViewModels.Courses;

namespace SharpMind.Services;

public interface ICourseCatalogService
{
    Task<CourseCatalogViewModel> GetCatalogAsync(CourseFilterViewModel filter);
}

