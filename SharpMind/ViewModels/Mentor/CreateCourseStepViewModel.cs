using SharpMind.Models;
using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Mentor;

public class CreateCourseStepViewModel
{
    [Required(ErrorMessage = "Назва курсу обов'язкова")]
    [StringLength(120, ErrorMessage = "Назва не повинна перевищувати 120 символів")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опис курсу обов'язковий")]
    [StringLength(1200, ErrorMessage = "Опис не повинен перевищувати 1200 символів")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Тема курсу обов'язкова")]
    public CourseTopic Topic { get; set; }

    [Required(ErrorMessage = "Рівень курсу обов'язковий")]
    public CourseLevel Level { get; set; }

    [Required(ErrorMessage = "Ціна обов'язкова")]
    [Range(0, 99999, ErrorMessage = "Ціна має бути від 0 до 99999")]
    public decimal Price { get; set; }
}

