using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class Question
{
    public int Id { get; set; }

    [Required, StringLength(600)]
    public string Text { get; set; } = string.Empty;

    public int TestId { get; set; }
    public Test? Test { get; set; }

    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}

