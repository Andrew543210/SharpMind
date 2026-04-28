using SharpMind.Models.Identity;

namespace SharpMind.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
    public DateTime? EnrolledAt { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

