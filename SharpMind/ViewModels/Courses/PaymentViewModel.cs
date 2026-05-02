using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Courses;

public class PaymentViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Номер картки обов'язковий")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Номер картки має містити 16 цифр")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Термін дії обов'язковий")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Формат: МM/РР")]
    public string ExpiryDate { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV обов'язковий")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV має містити 3 цифри")]
    public string CVV { get; set; } = string.Empty;
}

