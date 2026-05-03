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

        if (filter.PriceFrom.HasValue)
        {
            query = query.Where(c => c.Price >= filter.PriceFrom.Value);
        }

        if (filter.PriceTo.HasValue)
        {
            query = query.Where(c => c.Price <= filter.PriceTo.Value);
        }

        // First materialize with Select to get the calculated popularity
        var materialized = await query
            .Select(c => new
            {
                Course = c,
                Popularity = c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved)
            })
            .ToListAsync();

        // Then apply sorting in-memory to avoid IQueryable/IAsyncEnumerable issues
        var sorted = filter.SortBy switch
        {
            // For Name: invert the sort direction compared to other sorts
            // Up arrow (↑) = Descending Z->A, Down arrow (↓) = Ascending A->Z
            CourseSortType.Name when filter.SortDescending => materialized.OrderBy(x => x.Course.Title),
            CourseSortType.Name => materialized.OrderByDescending(x => x.Course.Title),
            
            // Price sorting (normal direction)
            CourseSortType.Price when filter.SortDescending => materialized.OrderByDescending(x => x.Course.Price).ThenBy(x => x.Course.Title),
            CourseSortType.Price => materialized.OrderBy(x => x.Course.Price).ThenBy(x => x.Course.Title),
            
            // Popularity sorting
            CourseSortType.Popularity when filter.SortDescending => materialized.OrderByDescending(x => x.Popularity).ThenBy(x => x.Course.Title),
            CourseSortType.Popularity => materialized.OrderBy(x => x.Popularity).ThenBy(x => x.Course.Title),
            _ => materialized.OrderBy(x => x.Course.Title)
        };

        var courses = sorted
            .Select(x => new CourseCardViewModel
            {
                Id = x.Course.Id,
                Title = x.Course.Title,
                Description = x.Course.Description,
                Topic = x.Course.Topic,
                Level = x.Course.Level,
                Price = x.Course.Price,
                MentorName = x.Course.Mentor != null ? $"{x.Course.Mentor.FirstName} {x.Course.Mentor.LastName}".Trim() : x.Course.MentorId,
                Popularity = x.Popularity
            })
            .ToList();

        return new CourseCatalogViewModel
        {
            Filter = filter,
            Courses = courses
        };
    }
}

