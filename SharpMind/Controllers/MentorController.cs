using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.ViewModels.Mentor;

namespace SharpMind.Controllers;

[Authorize(Roles = "Mentor")]
public class MentorController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var courses = await dbContext.Courses
            .Where(c => c.MentorId == mentorId)
            .Include(c => c.Modules)
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return View(courses);
    }

    [HttpGet]
    public IActionResult CreateCourse()
    {
        return View(new CreateCourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CreateCourseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var course = new Course
        {
            Title = model.Title,
            Description = model.Description,
            Topic = model.Topic,
            Level = model.Level,
            Price = model.Price,
            MentorId = mentorId,
            IsPublished = model.IsPublished,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Course created.";
        return RedirectToAction(nameof(EditCourse), new { courseId = course.Id });
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int courseId)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await dbContext.Courses
            .Include(c => c.Modules.OrderBy(m => m.Order))
                .ThenInclude(m => m.Materials)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Test!)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.AnswerOptions)
            .Include(c => c.Modules)
                .ThenInclude(m => m.PracticalTask)
            .Include(c => c.Tests.Where(t => t.IsFinal))
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.MentorId == mentorId);

        if (course is null)
        {
            return NotFound();
        }

        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModule(int courseId, string title, string description)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await dbContext.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.MentorId == mentorId);
        if (course is null)
        {
            return NotFound();
        }

        var nextOrder = await dbContext.Modules.Where(m => m.CourseId == courseId).Select(m => m.Order).DefaultIfEmpty(0).MaxAsync() + 1;

        var module = new CourseModule
        {
            CourseId = courseId,
            Title = title,
            Description = description,
            Order = nextOrder,
            Test = new Test { Title = $"Quiz for {title}" },
            PracticalTask = new PracticalTask { Title = $"Task for {title}", Description = "Describe expected implementation and deliverables." }
        };

        dbContext.Modules.Add(module);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Module added with empty test and practical task.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMaterial(int moduleId, int courseId, string title, string content)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        dbContext.Materials.Add(new Material
        {
            ModuleId = moduleId,
            Title = title,
            Content = content
        });

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material added.";
        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int testId, int courseId, string text, string option1, string option2, string option3, string option4, int correctOption)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var question = new Question
        {
            TestId = testId,
            Text = text,
            AnswerOptions =
            [
                new AnswerOption { Text = option1, IsCorrect = correctOption == 1 },
                new AnswerOption { Text = option2, IsCorrect = correctOption == 2 },
                new AnswerOption { Text = option3, IsCorrect = correctOption == 3 },
                new AnswerOption { Text = option4, IsCorrect = correctOption == 4 }
            ]
        };

        dbContext.Questions.Add(question);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Question added.";
        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnsureFinalTest(int courseId, string title)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var finalTest = await dbContext.Tests.FirstOrDefaultAsync(t => t.CourseId == courseId && t.IsFinal);
        if (finalTest is null)
        {
            dbContext.Tests.Add(new Test
            {
                CourseId = courseId,
                IsFinal = true,
                Title = string.IsNullOrWhiteSpace(title) ? "Final course test" : title.Trim()
            });
        }
        else if (!string.IsNullOrWhiteSpace(title))
        {
            finalTest.Title = title.Trim();
        }

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Final test updated.";
        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpGet]
    public async Task<IActionResult> EnrollmentRequests()
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var requests = await dbContext.Enrollments
            .Where(e => e.Status == EnrollmentStatus.Pending && e.Course!.MentorId == mentorId)
            .Include(e => e.Course)
            .Include(e => e.Student)
            .OrderBy(e => e.RequestedAt)
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetEnrollmentStatus(int enrollmentId, EnrollmentStatus status)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var enrollment = await dbContext.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.Course!.MentorId == mentorId);

        if (enrollment is null)
        {
            return NotFound();
        }

        enrollment.Status = status;
        enrollment.EnrolledAt = status == EnrollmentStatus.Approved ? DateTime.UtcNow : null;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Enrollment status updated.";

        return RedirectToAction(nameof(EnrollmentRequests));
    }

    [HttpGet]
    public async Task<IActionResult> Submissions()
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var submissions = await dbContext.PracticalSubmissions
            .Where(s => s.Task!.Module!.Course!.MentorId == mentorId)
            .Include(s => s.Student)
            .Include(s => s.Task!)
                .ThenInclude(t => t.Module!)
                    .ThenInclude(m => m.Course)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync();

        return View(submissions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GradeSubmission(GradeSubmissionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Invalid grading payload.";
            return RedirectToAction(nameof(Submissions));
        }

        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var submission = await dbContext.PracticalSubmissions
            .Include(s => s.Task!)
                .ThenInclude(t => t.Module!)
                    .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(s => s.Id == model.SubmissionId && s.Task!.Module!.Course!.MentorId == mentorId);

        if (submission is null)
        {
            return NotFound();
        }

        submission.Grade = model.Grade;
        submission.MentorComment = model.MentorComment;
        submission.Status = SubmissionStatus.Graded;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Submission graded.";
        return RedirectToAction(nameof(Submissions));
    }

    private async Task<bool> OwnsCourseAsync(int courseId)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return await dbContext.Courses.AnyAsync(c => c.Id == courseId && c.MentorId == mentorId);
    }
}

