using System.ComponentModel.DataAnnotations;
using SharpMind.Models.Identity;

namespace SharpMind.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public CourseTopic Topic { get; set; }

    [Required]
    public CourseLevel Level { get; set; }

    [Range(0, 99999)]
    public decimal Price { get; set; }

    [Required]
    public string MentorId { get; set; } = string.Empty;
    public ApplicationUser? Mentor { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsPublished { get; set; } = true;

    public ICollection<CourseModule> Modules { get; set; } = new List<CourseModule>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Test> Tests { get; set; } = new List<Test>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}

