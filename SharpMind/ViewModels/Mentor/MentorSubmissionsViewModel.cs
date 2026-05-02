using SharpMind.Models;

namespace SharpMind.ViewModels.Mentor;

public class MentorSubmissionsViewModel
{
    public List<PracticalSubmission> Graded { get; set; } = new();
    public List<PracticalSubmission> Pending { get; set; } = new();
}
