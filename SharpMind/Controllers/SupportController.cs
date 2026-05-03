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
    [Authorize(Roles = "Student,Mentor")]
    [HttpGet]
    public async Task<IActionResult> Ask()
    {
        return View(new CreateSupportTicketViewModel());
    }

    [Authorize(Roles = "Student,Mentor")]
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
            UserId = userId!,
            Subject = model.Subject,
            Message = model.Message,
            CreatedAt = DateTime.UtcNow,
            Status = TicketStatus.Pending
        };

        dbContext.SupportTickets.Add(ticket);
        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Ваше питання надіслано адміністратору. Спасибі за звернення!";
        return RedirectToAction("MyTickets");
    }

    [Authorize(Roles = "Student,Mentor")]
    [HttpGet]
    public async Task<IActionResult> MyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var tickets = await dbContext.SupportTickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(tickets);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> AllTickets()
    {
        var tickets = await dbContext.SupportTickets
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var model = new AdminSupportViewModel
        {
            Pending = tickets.Where(t => t.Status == TicketStatus.Pending).ToList(),
            Resolved = tickets.Where(t => t.Status == TicketStatus.Resolved).ToList()
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

        ticket.AdminReply = response;
        ticket.ReplyDate = DateTime.UtcNow;
        ticket.Status = TicketStatus.Resolved;

        await dbContext.SaveChangesAsync();

        TempData["Success"] = "Відповідь надіслана.";
        return RedirectToAction(nameof(AllTickets));
    }
}

