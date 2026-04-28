using Microsoft.EntityFrameworkCore;
using SharpMind.Data;

namespace SharpMind.Services;

public class ProgressService(ApplicationDbContext dbContext) : IProgressService
{
    public async Task<decimal> GetCourseProgressPercentAsync(int courseId, string studentId)
    {
        var moduleIds = await dbContext.Modules
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync();

        if (moduleIds.Count == 0)
        {
            return 0;
        }

        var testIds = await dbContext.Tests
            .Where(t => moduleIds.Contains(t.ModuleId))
            .Select(t => t.Id)
            .ToListAsync();

        var taskIds = await dbContext.PracticalTasks
            .Where(t => moduleIds.Contains(t.ModuleId))
            .Select(t => t.Id)
            .ToListAsync();

        var totalUnits = testIds.Count + taskIds.Count;
        if (totalUnits == 0)
        {
            return 0;
        }

        var completedTests = await dbContext.TestResults
            .Where(r => r.StudentId == studentId && testIds.Contains(r.TestId) && r.ScorePercent >= 60)
            .CountAsync();

        var gradedTasks = await dbContext.PracticalSubmissions
            .Where(s => s.StudentId == studentId && taskIds.Contains(s.TaskId) && s.Status == Models.SubmissionStatus.Graded)
            .Select(s => s.TaskId)
            .Distinct()
            .CountAsync();

        return Math.Round((completedTests + gradedTasks) * 100m / totalUnits, 1);
    }
}

