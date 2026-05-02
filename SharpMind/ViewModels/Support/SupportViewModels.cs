using System.ComponentModel.DataAnnotations;
using SharpMind.Models;

namespace SharpMind.ViewModels.Support;

public class CreateSupportTicketViewModel
{
    [Required(ErrorMessage = "Тема обов'язкова")]
    [StringLength(120)]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Повідомлення обов'язкове")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Повідомлення має містити від 10 до 2000 символів")]
    public string Message { get; set; } = string.Empty;
}

public class AdminSupportViewModel
{
    public List<SupportTicket> Pending { get; set; } = new();
    public List<SupportTicket> Resolved { get; set; } = new();
}

