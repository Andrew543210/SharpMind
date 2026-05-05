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

        var model = new List<MentorDashboardCourseVm>();
        foreach (var course in courses)
        {
            var averageRating = 0m;
            model.Add(new MentorDashboardCourseVm
            {
                CourseId = course.Id,
                Title = course.Title,
                Topic = course.Topic.ToString(),
                Level = course.Level,
                Description = course.Description,
                ModulesCount = course.Modules.Count,
                StudentsCount = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved),
                AverageRating = averageRating
            });
        }

        return View(model);
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

        var model = new MentorSubmissionsViewModel
        {
            Graded = submissions.Where(s => s.Status == SubmissionStatus.Graded).ToList(),
            Pending = submissions.Where(s => s.Status == SubmissionStatus.Pending).ToList()
        };

        return View(model);
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

    // ===================== РЕДАГУВАННЯ КУРСУ - НОВІ МЕТОДИ =====================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModuleAtPosition(int courseId, string title, string description, int? position)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await dbContext.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.MentorId == mentorId);
        if (course is null)
        {
            return NotFound();
        }

        var targetPosition = position ?? int.MaxValue;
        var modules = await dbContext.Modules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .ToListAsync();

        // Якщо позиція більше за кількість модулів, додаємо в кінець
        if (targetPosition > modules.Count)
        {
            targetPosition = modules.Count + 1;
        }
        else if (targetPosition < 1)
        {
            targetPosition = 1;
        }

        // Змінюємо Order для модулів, які мають Order >= targetPosition
        foreach (var m in modules.Where(m => m.Order >= targetPosition))
        {
            m.Order++;
        }

        var newModule = new CourseModule
        {
            CourseId = courseId,
            Title = title,
            Description = description,
            Order = targetPosition,
            Test = new Test { Title = $"Quiz for {title}" },
            PracticalTask = new PracticalTask { Title = $"Task for {title}", Description = "Describe expected implementation and deliverables." }
        };

        dbContext.Modules.Add(newModule);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = $"Module added at position {targetPosition}.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaterial(int materialId, int courseId)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var material = await dbContext.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if (material is null)
        {
            return NotFound();
        }

        dbContext.Materials.Remove(material);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material deleted.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMaterial(int materialId, int courseId, string title, string content)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var material = await dbContext.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if (material is null)
        {
            return NotFound();
        }

        material.Title = title;
        material.Content = content;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material updated.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int questionId, int courseId)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var question = await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == questionId);
        
        if (question is null)
        {
            return NotFound();
        }

        dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Question deleted.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int questionId, int courseId, string text, string option1, string option2, string option3, string option4, int correctOption)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var question = await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == questionId);
        
        if (question is null)
        {
            return NotFound();
        }

        question.Text = text;
        
        // Видаляємо старі варіанти відповідей
        dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
        
        // Додаємо нові
        question.AnswerOptions =
        [
            new AnswerOption { Text = option1, IsCorrect = correctOption == 1 },
            new AnswerOption { Text = option2, IsCorrect = correctOption == 2 },
            new AnswerOption { Text = option3, IsCorrect = correctOption == 3 },
            new AnswerOption { Text = option4, IsCorrect = correctOption == 4 }
        ];

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Question updated.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPracticalTask(int taskId, int courseId, string title, string description)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var task = await dbContext.PracticalTasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null)
        {
            return NotFound();
        }

        task.Title = title;
        task.Description = description;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Practical task updated.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(int moduleId, int courseId)
    {
        if (!await OwnsCourseAsync(courseId))
        {
            return NotFound();
        }

        var module = await dbContext.Modules
            .Include(m => m.Materials)
            .Include(m => m.Test!)
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
            .Include(m => m.PracticalTask)
            .FirstOrDefaultAsync(m => m.Id == moduleId);
        
        if (module is null)
        {
            return NotFound();
        }

        var moduleOrder = module.Order;
        
        // Видаляємо матеріали
        dbContext.Materials.RemoveRange(module.Materials);
        
        // Видаляємо тест з питаннями
        if (module.Test != null)
        {
            foreach (var question in module.Test.Questions)
            {
                dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
            }
            dbContext.Questions.RemoveRange(module.Test.Questions);
            dbContext.Tests.Remove(module.Test);
        }
        
        // Видаляємо практичне завдання
        if (module.PracticalTask != null)
        {
            dbContext.PracticalTasks.Remove(module.PracticalTask);
        }
        
        // Видаляємо модуль
        dbContext.Modules.Remove(module);
        await dbContext.SaveChangesAsync();
        
        // Змінюємо порядок модулів, які йдуть після видаленого
        var subsequentModules = await dbContext.Modules
            .Where(m => m.CourseId == courseId && m.Order > moduleOrder)
            .OrderBy(m => m.Order)
            .ToListAsync();
        
        foreach (var m in subsequentModules)
        {
            m.Order--;
        }
        
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Module and all its content deleted.";

        return RedirectToAction(nameof(EditCourse), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishCourse(int courseId)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var course = await dbContext.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Test!)
                    .ThenInclude(t => t.Questions)
            .Include(c => c.Tests.Where(t => t.IsFinal))
                .ThenInclude(t => t.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId && c.MentorId == mentorId);
        
        if (course is null)
        {
            return NotFound();
        }

        // Перевіряємо, що є хоча б один модуль з тестом
        if (course.Modules.Count == 0)
        {
            TempData["Error"] = "Course must have at least one module.";
            return RedirectToAction(nameof(EditCourse), new { courseId });
        }

        // Перевіряємо, що є фіналь-тест
        var finalTest = course.Tests.FirstOrDefault(t => t.IsFinal);
        if (finalTest is null || finalTest.Questions.Count == 0)
        {
            TempData["Error"] = "Course must have a final test with questions.";
            return RedirectToAction(nameof(EditCourse), new { courseId });
        }

        course.IsPublished = true;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Course published successfully!";

        return RedirectToAction(nameof(Dashboard));
    }

    private async Task<bool> OwnsCourseAsync(int courseId)
    {
        var mentorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return await dbContext.Courses.AnyAsync(c => c.Id == courseId && c.MentorId == mentorId);
    }
}

