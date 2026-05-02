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

        var courses = new List<StudentDashboardCourseVm>();
        foreach (var enrollment in enrollments)
        {
            var progress = await progressService.GetCourseProgressPercentAsync(enrollment.CourseId, userId);
            var rating = await progressService.GetCourseRatingSummaryAsync(enrollment.CourseId, userId);
            var canGetCertificate = await progressService.CanIssueCertificateAsync(enrollment.CourseId, userId);
            courses.Add(new StudentDashboardCourseVm
            {
                CourseId = enrollment.CourseId,
                Title = enrollment.Course!.Title,
                MentorName = $"{enrollment.Course.Mentor?.FirstName} {enrollment.Course.Mentor?.LastName}".Trim(),
                ProgressPercent = progress,
                CertificateId = await progressService.GetCertificateIdAsync(enrollment.CourseId, userId),
                CanGetCertificate = canGetCertificate,
                RatingPoints = rating.Points,
                RatingRank = rating.Rank,
                RatingTotal = rating.TotalStudents
            });
        }

        var certificates = await dbContext.Certificates
            .Where(c => c.StudentId == userId)
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync();

        var model = new StudentDashboardViewModel
        {
            Courses = courses,
            Certificates = certificates
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> GenerateCertificate(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var canGetCertificate = await progressService.CanIssueCertificateAsync(courseId, userId);
        if (!canGetCertificate)
        {
            return BadRequest("You have not met the certificate requirements for this course.");
        }

        try
        {
            var certificateId = await progressService.IssueCertificateAsync(courseId, userId);
            TempData["Success"] = "Сертифікат успішно створено!";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Dashboard));
        }
    }

    [HttpGet]
    public async Task<IActionResult> DownloadCertificate(int certificateId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var certificate = await dbContext.Certificates
            .Where(c => c.Id == certificateId && c.StudentId == userId)
            .Include(c => c.Course)
            .FirstOrDefaultAsync();

        if (certificate is null)
        {
            return NotFound();
        }

        var certificateService = new CertificateService();
        var content = certificateService.GenerateCertificateContent(certificate);
        var fileName = $"Certificate_{certificate.CourseName}_{DateTime.Now:yyyy-MM-dd}.txt";

        return File(System.Text.Encoding.UTF8.GetBytes(content), "text/plain", fileName);
    }
}
