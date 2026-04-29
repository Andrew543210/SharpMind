using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.Services;
using SharpMind.ViewModels.Courses;

namespace SharpMind.Controllers;

public class CoursesController(
    ApplicationDbContext dbContext,
    ICourseCatalogService catalogService,
    IProgressService progressService) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromQuery] CourseFilterViewModel filter)
    {
        var model = await catalogService.GetCatalogAsync(filter);
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var course = await dbContext.Courses
            .Include(c => c.Mentor)
            .Include(c => c.Modules.OrderBy(m => m.Order))
                .ThenInclude(m => m.Materials)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Test)
            .Include(c => c.Modules)
                .ThenInclude(m => m.PracticalTask)
            .Include(c => c.Tests)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);

        if (course is null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        EnrollmentStatus? enrollmentStatus = null;
        decimal progressPercent = 0;
        var canGetCertificate = false;
        int? certificateId = null;

        if (!string.IsNullOrEmpty(userId))
        {
            enrollmentStatus = await dbContext.Enrollments
                .Where(e => e.CourseId == id && e.StudentId == userId)
                .Select(e => (EnrollmentStatus?)e.Status)
                .FirstOrDefaultAsync();

            if (enrollmentStatus == EnrollmentStatus.Approved)
            {
                progressPercent = await progressService.GetCourseProgressPercentAsync(id, userId);
<<<<<<< Updated upstream
                canGetCertificate = await progressService.CanIssueCertificateAsync(id, userId);
                certificateId = await progressService.GetCertificateIdAsync(id, userId);
            }
        }
=======
                isCompleted = await progressService.IsCourseCompletedAsync(id, userId);

            .FirstOrDefaultAsync(t => t.CourseId == id && t.IsFinalTest);
>>>>>>> Stashed changes

        var finalTest = course.Tests.FirstOrDefault(t => t.IsFinal);

        return View(new CourseDetailsViewModel
        {
            Course = course,
            EnrollmentStatus = enrollmentStatus,
            ProgressPercent = progressPercent,
<<<<<<< Updated upstream
            CanGetCertificate = canGetCertificate,
            CertificateId = certificateId,
            FinalTest = finalTest
=======
            FinalTest = finalTest,
            CertificateId = certificateId
>>>>>>> Stashed changes
        });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestEnrollment(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ProgressPercent = progressPercent

        if (existing is null)
        {
            dbContext.Enrollments.Add(new Enrollment
            {
                CourseId = courseId,
                StudentId = userId,
                Status = EnrollmentStatus.Pending,
                RequestedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
            TempData["Success"] = "Enrollment request sent to mentor.";
        }

        return RedirectToAction(nameof(Details), new { id = courseId });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetCertificate(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (!await IsApprovedStudentAsync(courseId))
        {
            return Forbid();
        }

        try
        {
            var certificateId = await progressService.IssueCertificateAsync(courseId, userId);
            TempData["Success"] = "Сертифікат згенеровано.";
            return RedirectToAction(nameof(Certificate), new { id = certificateId });
        }
        catch (InvalidOperationException)
        {
            TempData["Error"] = "Умови завершення курсу ще не виконані.";
            return RedirectToAction(nameof(Details), new { id = courseId });
        }
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Certificate(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var certificate = await dbContext.Certificates
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id && c.StudentId == userId);

        if (certificate is null)
        {
            return NotFound();
        }

        return View(certificate);
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> TakeTest(int testId, int courseId)
    {
        if (!await IsApprovedStudentAsync(courseId))
        {
            return Forbid();
        }

        var test = await dbContext.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(t => t.Id == testId);

        if (test is null)
        {
            return NotFound();
        }

        var model = new TakeTestViewModel
        {
            TestId = test.Id,
            CourseId = courseId,
            TestTitle = test.Title,
            Questions = test.Questions.Select(q => new TakeTestViewModel.QuestionVm
            {
                QuestionId = q.Id,
                Text = q.Text,
                Options = q.AnswerOptions.Select(o => new TakeTestViewModel.OptionVm
                {
                    OptionId = o.Id,
                    Text = o.Text
                }).ToList()
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TakeTest(TakeTestViewModel model)
    {
        if (!await IsApprovedStudentAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var test = await dbContext.Tests
            .Include(t => t.Questions)
                .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(t => t.Id == model.TestId);

        if (test is null)
        {
            return NotFound();
        }

        var correctCount = 0;
        foreach (var question in test.Questions)
        {
            var answer = model.Questions.FirstOrDefault(q => q.QuestionId == question.Id);
            var correctOptionId = question.AnswerOptions.First(o => o.IsCorrect).Id;
            if (answer?.SelectedOptionId == correctOptionId)
            {
                correctCount++;
            }
        }

        var score = test.Questions.Count == 0 ? 0 : Math.Round(correctCount * 100m / test.Questions.Count, 2);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var existingResult = await dbContext.TestResults
            .FirstOrDefaultAsync(r => r.TestId == model.TestId && r.StudentId == userId);

        if (existingResult is null)
        {
            dbContext.TestResults.Add(new TestResult
            {
                TestId = model.TestId,
                StudentId = userId,
                ScorePercent = score,
                CompletedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingResult.ScorePercent = score;
            existingResult.CompletedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync();
        TempData["Success"] = $"Test completed. Your score: {score}%";

        return RedirectToAction(nameof(Details), new { id = model.CourseId });
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTask(SubmitTaskViewModel model)
    {
        if (!await IsApprovedStudentAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Answer text is required.";
            return RedirectToAction(nameof(Details), new { id = model.CourseId });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await dbContext.PracticalSubmissions
            .Where(s => s.TaskId == model.TaskId && s.StudentId == userId)
            .OrderByDescending(s => s.SubmittedAt)
            .FirstOrDefaultAsync();

        if (existing is null)
        {
            dbContext.PracticalSubmissions.Add(new PracticalSubmission
            {
                TaskId = model.TaskId,
                StudentId = userId,
                AnswerText = model.AnswerText,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Pending
            });
        }
        else
        {
            existing.AnswerText = model.AnswerText;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.Status = SubmissionStatus.Pending;
            existing.Grade = null;
            existing.MentorComment = null;
        }

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Practical task submitted.";
        return RedirectToAction(nameof(Details), new { id = model.CourseId });
    }

    private async Task<bool> IsApprovedStudentAsync(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        return await dbContext.Enrollments.AnyAsync(e =>
            e.CourseId == courseId && e.StudentId == userId && e.Status == EnrollmentStatus.Approved);
    }
}

