namespace SharpMind.Services;

public sealed record CourseRatingEntry(string StudentId, string StudentName, decimal Points, int Rank);
public sealed record CourseRatingSummary(decimal Points, int Rank, int TotalStudents);

