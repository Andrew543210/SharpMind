using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.Models.Identity;
using SharpMind.Services;
using SharpMind.ViewModels.Admin;

namespace SharpMind.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    ICourseStatisticsService statisticsService) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Users = await userManager.Users.OrderBy(u => u.Email).ToListAsync();
        ViewBag.Courses = await dbContext.Courses.Include(c => c.Mentor).OrderBy(c => c.Title).ToListAsync();
        ViewBag.Mentors = await userManager.GetUsersInRoleAsync(AppRoles.Mentor);
        return View();
    }

    [HttpGet]
    public IActionResult CreateMentor()
    {
        return View(new CreateMentorViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateMentor(CreateMentorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await userManager.AddToRoleAsync(user, AppRoles.Mentor);
        TempData["Success"] = "Mentor account created.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpGet]
    public async Task<IActionResult> ManageRole(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var model = new ManageRoleViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? user.Id,
            SelectedRole = currentRoles.FirstOrDefault() ?? AppRoles.Student
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageRole(ManageRoleViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (!await roleManager.RoleExistsAsync(model.SelectedRole))
        {
            ModelState.AddModelError(nameof(model.SelectedRole), "Role does not exist.");
            return View(model);
        }

        var current = await userManager.GetRolesAsync(user);
        if (current.Any())
        {
            await userManager.RemoveFromRolesAsync(user, current);
        }

        await userManager.AddToRoleAsync(user, model.SelectedRole);
        TempData["Success"] = "Role updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMentor(int courseId, string mentorId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
        {
            return NotFound();
        }

        var mentor = await userManager.FindByIdAsync(mentorId);
        if (mentor is null || !await userManager.IsInRoleAsync(mentor, AppRoles.Mentor))
        {
            TempData["Error"] = "Selected user is not a mentor.";
            return RedirectToAction(nameof(Dashboard));
        }

        course.MentorId = mentorId;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Course mentor updated.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int courseId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
        {
            return NotFound();
        }

        dbContext.Courses.Remove(course);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Course deleted.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = string.Join("; ", result.Errors.Select(e => e.Description));
        }
        else
        {
            TempData["Success"] = "User deleted.";
        }

        return RedirectToAction(nameof(Dashboard));
    }

    // ===================== УПРАВЛІННЯ КУРСАМИ =====================

    [HttpGet]
    public async Task<IActionResult> Courses(string? search)
    {
        var query = dbContext.Courses
            .Include(c => c.Mentor)
            .Include(c => c.Modules)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));
        }

        var courses = await query.OrderBy(c => c.Title).ToListAsync();

        var courseStats = new List<(Course Course, CourseStatisticsVm Stats)>();
        foreach (var course in courses)
        {
            var stats = await statisticsService.GetCourseStatisticsAsync(course.Id);
            courseStats.Add((course, stats));
        }

        ViewBag.Search = search;
        return View(courseStats);
    }

    [HttpGet]
    public async Task<IActionResult> EditCourseAdmin(int courseId)
    {
        var course = await dbContext.Courses
            .Include(c => c.Mentor)
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
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            return NotFound();
        }

        var stats = await statisticsService.GetCourseStatisticsAsync(courseId);
        ViewBag.Statistics = stats;

        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCourseAdmin(int courseId, string title, string description, decimal price, string level)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
        {
            return NotFound();
        }

        if (Enum.TryParse<CourseLevel>(level, out var parsedLevel))
        {
            course.Level = parsedLevel;
        }

        course.Title = title;
        course.Description = description;
        course.Price = price;

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Course updated.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCoursePublish(int courseId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
        {
            return NotFound();
        }

        course.IsPublished = !course.IsPublished;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = $"Course {(course.IsPublished ? "published" : "unpublished")}.";
        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpGet]
    public async Task<IActionResult> CourseStatistics(int courseId)
    {
        var course = await dbContext.Courses
            .Include(c => c.Mentor)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course is null)
        {
            return NotFound();
        }

        var stats = await statisticsService.GetCourseStatisticsAsync(courseId);
        
        // Отримуємо додаткову інформацію про студентів
        var enrolledStudents = await dbContext.Enrollments
            .Where(e => e.CourseId == courseId && e.Status == EnrollmentStatus.Approved)
            .Include(e => e.Student)
            .ToListAsync();

        var certificates = await dbContext.Certificates
            .Where(c => c.CourseId == courseId)
            .Select(c => c.StudentId)
            .ToListAsync();

        ViewBag.EnrolledStudents = enrolledStudents;
        ViewBag.CertificateHolders = certificates;

        return View(stats);
    }

    [HttpGet]
    public async Task<IActionResult> Support(string? filter)
    {
        var tickets = string.IsNullOrEmpty(filter) || filter == "pending"
            ? await dbContext.SupportTickets
                .Include(t => t.User)
                .Where(t => t.Status == TicketStatus.Pending)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync()
            : await dbContext.SupportTickets
                .Include(t => t.User)
                .Where(t => t.Status == TicketStatus.Resolved)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        ViewBag.Filter = filter ?? "pending";
        return View(tickets);
    }

    [HttpGet]
    public async Task<IActionResult> TicketDetail(int id)
    {
        var ticket = await dbContext.SupportTickets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (ticket is null)
        {
            return NotFound();
        }

        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyToTicket(int ticketId, string reply)
    {
        var ticket = await dbContext.SupportTickets.FindAsync(ticketId);
        if (ticket is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(reply))
        {
            TempData["Error"] = "Відповідь не може бути порожною.";
            return RedirectToAction(nameof(TicketDetail), new { id = ticketId });
        }

        ticket.AdminReply = reply;
        ticket.ReplyDate = DateTime.UtcNow;
        ticket.Status = TicketStatus.Resolved;
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Відповідь надіслана.";
        return RedirectToAction(nameof(TicketDetail), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMaterial(int moduleId, int courseId, string title, string content)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        dbContext.Materials.Add(new Material
        {
            ModuleId = moduleId,
            Title = title,
            Content = content
        });

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material added.";
        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddQuestion(int testId, int courseId, string text, string option1, string option2, string option3, string option4, int correctOption)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

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
        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnsureFinalTest(int courseId, string title)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

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
        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddModuleAtPosition(int courseId, string title, string description, int? position)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var targetPosition = position ?? int.MaxValue;
        var modules = await dbContext.Modules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .ToListAsync();

        if (targetPosition > modules.Count)
            targetPosition = modules.Count + 1;
        else if (targetPosition < 1)
            targetPosition = 1;

        foreach (var m in modules.Where(m => m.Order >= targetPosition))
            m.Order++;

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

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMaterial(int materialId, int courseId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var material = await dbContext.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if (material is null)
            return NotFound();

        dbContext.Materials.Remove(material);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material deleted.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMaterial(int materialId, int courseId, string title, string content)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var material = await dbContext.Materials.FirstOrDefaultAsync(m => m.Id == materialId);
        if (material is null)
            return NotFound();

        material.Title = title;
        material.Content = content;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Material updated.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int questionId, int courseId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var question = await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == questionId);
        
        if (question is null)
            return NotFound();

        dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
        dbContext.Questions.Remove(question);
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Question deleted.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int questionId, int courseId, string text, string option1, string option2, string option3, string option4, int correctOption)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var question = await dbContext.Questions
            .Include(q => q.AnswerOptions)
            .FirstOrDefaultAsync(q => q.Id == questionId);
        
        if (question is null)
            return NotFound();

        question.Text = text;
        
        dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
        
        question.AnswerOptions =
        [
            new AnswerOption { Text = option1, IsCorrect = correctOption == 1 },
            new AnswerOption { Text = option2, IsCorrect = correctOption == 2 },
            new AnswerOption { Text = option3, IsCorrect = correctOption == 3 },
            new AnswerOption { Text = option4, IsCorrect = correctOption == 4 }
        ];

        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Question updated.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPracticalTask(int taskId, int courseId, string title, string description)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var task = await dbContext.PracticalTasks.FirstOrDefaultAsync(t => t.Id == taskId);
        if (task is null)
            return NotFound();

        task.Title = title;
        task.Description = description;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Practical task updated.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteModule(int moduleId, int courseId)
    {
        var course = await dbContext.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound();

        var module = await dbContext.Modules
            .Include(m => m.Materials)
            .Include(m => m.Test!)
                .ThenInclude(t => t.Questions)
                    .ThenInclude(q => q.AnswerOptions)
            .Include(m => m.PracticalTask)
            .FirstOrDefaultAsync(m => m.Id == moduleId);
        
        if (module is null)
            return NotFound();

        var moduleOrder = module.Order;
        
        dbContext.Materials.RemoveRange(module.Materials);
        
        if (module.Test != null)
        {
            foreach (var question in module.Test.Questions)
                dbContext.AnswerOptions.RemoveRange(question.AnswerOptions);
            dbContext.Questions.RemoveRange(module.Test.Questions);
            dbContext.Tests.Remove(module.Test);
        }
        
        if (module.PracticalTask != null)
            dbContext.PracticalTasks.Remove(module.PracticalTask);
        
        dbContext.Modules.Remove(module);
        await dbContext.SaveChangesAsync();
        
        var subsequentModules = await dbContext.Modules
            .Where(m => m.CourseId == courseId && m.Order > moduleOrder)
            .OrderBy(m => m.Order)
            .ToListAsync();
        
        foreach (var m in subsequentModules)
            m.Order--;
        
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Module and all its content deleted.";

        return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishCourse(int courseId)
    {
        var course = await dbContext.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Test!)
                    .ThenInclude(t => t.Questions)
            .Include(c => c.Tests.Where(t => t.IsFinal))
                .ThenInclude(t => t.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);
        
        if (course is null)
            return NotFound();

        if (course.Modules.Count == 0)
        {
            TempData["Error"] = "Course must have at least one module.";
            return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
        }

        var finalTest = course.Tests.FirstOrDefault(t => t.IsFinal);
        if (finalTest is null || finalTest.Questions.Count == 0)
        {
            TempData["Error"] = "Course must have a final test with questions.";
            return RedirectToAction(nameof(EditCourseAdmin), new { courseId });
        }

        course.IsPublished = true;
        await dbContext.SaveChangesAsync();
        TempData["Success"] = "Course published successfully!";

        return RedirectToAction(nameof(Courses));
    }
}
