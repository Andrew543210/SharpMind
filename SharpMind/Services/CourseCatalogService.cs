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

        // Applying sorting before ToListAsync to avoid client-side evaluation issues
        query = filter.SortBy switch
        {
            CourseSortType.Name when filter.SortDescending => query.OrderByDescending(c => c.Title),
            CourseSortType.Name => query.OrderBy(c => c.Title),
            CourseSortType.Price when filter.SortDescending => query.OrderByDescending(c => c.Price).ThenBy(c => c.Title),
            CourseSortType.Price => query.OrderBy(c => c.Price).ThenBy(c => c.Title),
            CourseSortType.Popularity when filter.SortDescending => query
                .AsEnumerable()
                .OrderByDescending(c => c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved))
                .ThenBy(c => c.Title)
                .AsQueryable(),
            CourseSortType.Popularity => query
                .AsEnumerable()
                .OrderBy(c => c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved))
                .ThenBy(c => c.Title)
                .AsQueryable(),
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

