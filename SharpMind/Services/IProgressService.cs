namespace SharpMind.Services;

public interface IProgressService
{
    Task<decimal> GetCourseProgressPercentAsync(int courseId, string studentId);
    Task<bool> CanIssueCertificateAsync(int courseId, string studentId);
    Task<int?> GetCertificateIdAsync(int courseId, string studentId);
    Task<int> IssueCertificateAsync(int courseId, string studentId);
}

