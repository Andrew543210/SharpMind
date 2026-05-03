using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.Models.Identity;
using SharpMind.ViewModels.Admin;

namespace SharpMind.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : Controller
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
}


