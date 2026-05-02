using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class SupportTicket
{
    public int Id { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;
    public Identity.ApplicationUser? Student { get; set; }

    [Required, StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? AdminResponse { get; set; }
    public DateTime? ResponseAt { get; set; }
    
    public bool IsResolved { get; set; }
}

