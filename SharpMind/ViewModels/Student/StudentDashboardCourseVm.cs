namespace SharpMind.ViewModels.Student;

public class StudentDashboardCourseVm
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string MentorName { get; set; } = string.Empty;
    public decimal ProgressPercent { get; set; }
    public int? CertificateId { get; set; }
    public bool CanGetCertificate { get; set; }
    public decimal RatingPoints { get; set; }
    public int RatingRank { get; set; }
    public int RatingTotal { get; set; }
}
