using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMind.Data;
using SharpMind.Models;
using SharpMind.ViewModels.Support;

namespace SharpMind.Controllers;

public class SupportController(ApplicationDbContext dbContext) : Controller
{
    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> Ask()
    {
        return View(new CreateSupportTicketViewModel());
    }

    [Authorize(Roles = "Student")]
    [HttpPost]
    public async Task<IActionResult> Ask(CreateSupportTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var ticket = new SupportTicket
        {
            StudentId = userId!,
            Subject = model.Subject,
            Message = model.Message,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        dbContext.SupportTickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Ваше питання надіслано адміністратору. Спасибі за звернення!";
        return RedirectToAction("MyTickets");
    }

    [Authorize(Roles = "Student")]
    [HttpGet]
    public async Task<IActionResult> MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tickets = await dbContext.SupportTickets
            .Where(t => t.StudentId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> AllTickets()
    {
        var tickets = await dbContext.SupportTickets
            .Include(t => t.Student)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var model = new AdminSupportViewModel
        {
            Pending = tickets.Where(t => !t.IsResolved).ToList(),
            Resolved = tickets.Where(t => t.IsResolved).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RespondTicket(int ticketId, string response)
    {
        var ticket = await dbContext.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket is null)
        {
            return NotFound();
        }

        ticket.AdminResponse = response;
        ticket.ResponseAt = DateTime.UtcNow;
        ticket.IsResolved = true;

        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Відповідь надіслана студенту.";
        return RedirectToAction(nameof(AllTickets));
    }
}

