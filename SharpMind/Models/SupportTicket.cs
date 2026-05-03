using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class SupportTicket
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public Identity.ApplicationUser? User { get; set; }

    [Required, StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? AdminReply { get; set; }
    public DateTime? ReplyDate { get; set; }
    
    public TicketStatus Status { get; set; } = TicketStatus.Pending;
}

public enum TicketStatus
{
    Pending,
    Resolved
}

