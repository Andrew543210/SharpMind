using System.ComponentModel.DataAnnotations;
using SharpMind.Models;

namespace SharpMind.ViewModels.Courses;

public class TakeTestViewModel
{
    public int TestId { get; set; }
    public int CourseId { get; set; }
    public string TestTitle { get; set; } = string.Empty;
    public List<QuestionVm> Questions { get; set; } = [];
    public decimal? ScorePercent { get; set; }
    public int? Rank { get; set; }
    public int? TotalStudents { get; set; }
    public bool ShowResults { get; set; }

    public class QuestionVm
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<OptionVm> Options { get; set; } = [];

        [Required]
        public int? SelectedOptionId { get; set; }
    }

    public class OptionVm
    {
        public int OptionId { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}

