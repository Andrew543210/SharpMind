namespace SharpMind.Services;

public interface IProgressService
{
    Task<decimal> GetCourseProgressPercentAsync(int courseId, string studentId);
}

