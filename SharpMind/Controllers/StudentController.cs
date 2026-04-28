using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Services;
using SharpMind.ViewModels.Student;

namespace SharpMind.Controllers;

[Authorize(Roles = "Student")]
public class StudentController(ApplicationDbContext dbContext, IProgressService progressService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var enrollments = await dbContext.Enrollments
            .Where(e => e.StudentId == userId && e.Status == Models.EnrollmentStatus.Approved)
            .Include(e => e.Course)
                .ThenInclude(c => c!.Mentor)
            .ToListAsync();

        var model = new List<StudentDashboardCourseVm>();
        foreach (var enrollment in enrollments)
        {
            var progress = await progressService.GetCourseProgressPercentAsync(enrollment.CourseId, userId);
            model.Add(new StudentDashboardCourseVm
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course!.Title,
                MentorName = $"{enrollment.Course.Mentor?.FirstName} {enrollment.Course.Mentor?.LastName}".Trim(),
                ProgressPercent = progress
            });
        }

        return View(model);
    }
}

