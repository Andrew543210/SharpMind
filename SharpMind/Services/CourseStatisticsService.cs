using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;

namespace SharpMind.Services;

public interface ICourseStatisticsService
{
    Task<CourseStatisticsVm> GetCourseStatisticsAsync(int courseId);
}

public class CourseStatisticsService(ApplicationDbContext dbContext) : ICourseStatisticsService
{
    public async Task<CourseStatisticsVm> GetCourseStatisticsAsync(int courseId)
    {
        var course = await dbContext.Courses
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            throw new InvalidOperationException($"Course with ID {courseId} not found");
        }

        var totalEnrollments = course.Enrollments.Count;
        var approvedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved);
        
        // Підраховуємо студентів, які завершили курс (мають сертифікат)
        var completedCount = await dbContext.Certificates
            .Where(c => c.CourseId == courseId)
            .CountAsync();
        
        // Підраховуємо студентів, які мають результати тестів (активно вивчають)
        var activeCount = await dbContext.TestResults
            .Where(tr => tr.Test!.CourseId == courseId || tr.Test!.Module!.CourseId == courseId)
            .Select(tr => tr.StudentId)
            .Distinct()
            .CountAsync();

        // Студенти у процесі = всі записані - завершили
        var inProgress = Math.Max(0, approvedEnrollments - completedCount);

        return new CourseStatisticsVm
        {
            CourseId = courseId,
            CourseName = course.Title,
            TotalViews = totalEnrollments, // Можемо потім тракувати переглядів окремо
            TotalEnrollments = approvedEnrollments,
            Completed = completedCount,
            InProgress = inProgress,
            CompletionRate = approvedEnrollments > 0 
                ? Math.Round((completedCount * 100m) / approvedEnrollments, 2)
                : 0
        };
    }
}

public class CourseStatisticsVm
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalEnrollments { get; set; }
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public decimal CompletionRate { get; set; }
}

