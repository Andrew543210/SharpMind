using System.ComponentModel.DataAnnotations;

namespace SharpMind.ViewModels.Mentor;

public class CreateModuleViewModel
{
    [Required(ErrorMessage = "Назва модуля обов'язкова")]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Опис модуля обов'язковий")]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public int ModuleOrder { get; set; } = 1;
}

public class AddMaterialViewModel
{
    public int ModuleId { get; set; }
    
    [Required(ErrorMessage = "Посилання обов'язкове")]
    [Url(ErrorMessage = "Введіть корректне посилання")]
    public string MaterialUrl { get; set; } = string.Empty;
}

public class CreateFinalTestQuestionViewModel
{
    [Required]
    public string QuestionText { get; set; } = string.Empty;
    
    [Required]
    public string CorrectAnswer { get; set; } = string.Empty;
    
    [Required]
    public string Option1 { get; set; } = string.Empty;
    
    [Required]
    public string Option2 { get; set; } = string.Empty;
    
    [Required]
    public string Option3 { get; set; } = string.Empty;
}

public class CreateFinalTestViewModel
{
    public int CourseId { get; set; }
    public List<CreateFinalTestQuestionViewModel> Questions { get; set; } = new();
}

