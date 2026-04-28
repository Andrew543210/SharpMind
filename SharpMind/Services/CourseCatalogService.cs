using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.ViewModels.Courses;

namespace SharpMind.Services;

public class CourseCatalogService(ApplicationDbContext dbContext) : ICourseCatalogService
{
    public async Task<CourseCatalogViewModel> GetCatalogAsync(CourseFilterViewModel filter)
    {
        var query = dbContext.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished)
            .Include(c => c.Mentor)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(search));
        }

        if (filter.Topic.HasValue)
        {
            query = query.Where(c => c.Topic == filter.Topic.Value);
        }

        if (filter.Level.HasValue)
        {
            query = query.Where(c => c.Level == filter.Level.Value);
        }

        query = filter.SortBy switch
        {
            CourseSortType.Name => query.OrderBy(c => c.Title),
            CourseSortType.Price => query.OrderBy(c => c.Price).ThenBy(c => c.Title),
            CourseSortType.Popularity => query.OrderByDescending(c => c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved))
                .ThenBy(c => c.Title),
            _ => query.OrderBy(c => c.Title)
        };

        var courses = await query
            .Select(c => new CourseCardViewModel
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Topic = c.Topic,
                Level = c.Level,
                Price = c.Price,
                MentorName = c.Mentor != null ? $"{c.Mentor.FirstName} {c.Mentor.LastName}".Trim() : c.MentorId,
                Popularity = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved)
            })
            .ToListAsync();

        return new CourseCatalogViewModel
        {
            Filter = filter,
            Courses = courses
        };
    }
}

