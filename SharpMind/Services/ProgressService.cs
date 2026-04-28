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
            .Where(t => t.ModuleId.HasValue && moduleIds.Contains(t.ModuleId.Value))
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

    public async Task<bool> CanIssueCertificateAsync(int courseId, string studentId)
    {
        var moduleIds = await dbContext.Modules
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync();

        if (moduleIds.Count == 0)
        {
            return false;
        }

        var moduleTests = await dbContext.Tests
            .Where(t => t.ModuleId.HasValue && moduleIds.Contains(t.ModuleId.Value))
            .Select(t => new { t.Id, ModuleId = t.ModuleId!.Value })
            .ToListAsync();

        var moduleTasks = await dbContext.PracticalTasks
            .Where(t => moduleIds.Contains(t.ModuleId))
            .Select(t => new { t.Id, t.ModuleId })
            .ToListAsync();

        var bestModuleScores = await dbContext.TestResults
            .Where(r => r.StudentId == studentId && moduleTests.Select(t => t.Id).Contains(r.TestId))
            .GroupBy(r => r.TestId)
            .Select(g => new { TestId = g.Key, BestScore = g.Max(x => x.ScorePercent) })
            .ToListAsync();

        var gradedModuleTasks = await dbContext.PracticalSubmissions
            .Where(s => s.StudentId == studentId
                        && moduleTasks.Select(t => t.Id).Contains(s.TaskId)
                        && s.Grade != null)
            .Select(s => s.TaskId)
            .Distinct()
            .ToListAsync();

        var completedModules = 0;
        foreach (var moduleId in moduleIds)
        {
            var moduleTest = moduleTests.FirstOrDefault(t => t.ModuleId == moduleId);
            var moduleTask = moduleTasks.FirstOrDefault(t => t.ModuleId == moduleId);
            if (moduleTest is null || moduleTask is null)
            {
                continue;
            }

            var isTestPassed = bestModuleScores
                .Any(r => r.TestId == moduleTest.Id && r.BestScore >= 80);
            var isPracticeGraded = gradedModuleTasks.Contains(moduleTask.Id);

            if (isTestPassed && isPracticeGraded)
            {
                completedModules++;
            }
        }

        if (completedModules < 4)
        {
            return false;
        }

        var finalTestId = await dbContext.Tests
            .Where(t => t.CourseId == courseId && t.IsFinal)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync();

        if (!finalTestId.HasValue)
        {
            return false;
        }

        var bestFinalScore = await dbContext.TestResults
            .Where(r => r.StudentId == studentId && r.TestId == finalTestId.Value)
            .Select(r => (decimal?)r.ScorePercent)
            .MaxAsync();

        return bestFinalScore.HasValue && bestFinalScore.Value >= 80;
    }

    public Task<int?> GetCertificateIdAsync(int courseId, string studentId)
    {
        return dbContext.Certificates
            .Where(c => c.CourseId == courseId && c.StudentId == studentId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> IssueCertificateAsync(int courseId, string studentId)
    {
        var existingId = await GetCertificateIdAsync(courseId, studentId);
        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        if (!await CanIssueCertificateAsync(courseId, studentId))
        {
            throw new InvalidOperationException("Course completion requirements are not met.");
        }

        var student = await dbContext.Users
            .Where(u => u.Id == studentId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstAsync();

        var course = await dbContext.Courses
            .Where(c => c.Id == courseId)
            .Select(c => c.Title)
            .FirstAsync();

        var certificate = new Models.Certificate
        {
            StudentId = studentId,
            CourseId = courseId,
            IssueDate = DateTime.UtcNow,
            UniqueNumber = Guid.NewGuid(),
            FullName = $"{student.FirstName} {student.LastName}".Trim(),
            CourseName = course,
            PlatformName = "SharpMind"
        };

        dbContext.Certificates.Add(certificate);
        await dbContext.SaveChangesAsync();
        return certificate.Id;
    }
}

