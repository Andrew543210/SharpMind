using System.ComponentModel.DataAnnotations;

namespace SharpMind.Models;

public class AnswerOption
{
    public int Id { get; set; }

    [Required, StringLength(600)]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }
}

