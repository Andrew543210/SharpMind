using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class CourseModule
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public ICollection<Material> Materials { get; set; } = new List<Material>();
    public Test? Test { get; set; }
    public PracticalTask? PracticalTask { get; set; }
}

