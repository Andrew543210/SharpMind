using SharpMind.Models;

namespace SharpMind.Services;

public interface ITestShufflingService
{
    List<(int OriginalId, string Text)> ShuffleAnswerOptions(List<AnswerOption> options);
}

public class TestShufflingService : ITestShufflingService
{
    /// <summary>
    /// Перемішує варіанти відповідей та повертає список з оригінальним ID та текстом
    /// </summary>
    public List<(int OriginalId, string Text)> ShuffleAnswerOptions(List<AnswerOption> options)
    {
        var random = new Random();
        
        var answerList = options
            .Select(o => (OriginalId: o.Id, Text: o.Text))
            .OrderBy(_ => random.Next())
            .ToList();
        
        return answerList;
    }
}

