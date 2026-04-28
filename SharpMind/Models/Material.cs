using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class Material
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Content { get; set; } = string.Empty;

    public int ModuleId { get; set; }
    public CourseModule? Module { get; set; }
}

