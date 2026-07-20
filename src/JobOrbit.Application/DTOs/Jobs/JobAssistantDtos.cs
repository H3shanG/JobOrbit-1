namespace JobOrbit.Application.DTOs.Jobs;

public static class JobAssistantModes
{
    public const string Explain = "explain";
    public const string DailyWork = "daily_work";
    public const string InterviewQuestions = "interview_questions";

    public static bool IsSupported(string? mode)
    {
        return Normalize(mode) is not null;
    }

    public static string? Normalize(string? mode)
    {
        return mode?.Trim().ToLowerInvariant() switch
        {
            Explain => Explain,
            DailyWork => DailyWork,
            InterviewQuestions => InterviewQuestions,
            _ => null
        };
    }
}

public sealed class JobAssistantRequest
{
    public string Mode { get; set; } = JobAssistantModes.Explain;
}

public sealed class JobAssistantResponse
{
    public string Mode { get; set; } = JobAssistantModes.Explain;
    public string Intro { get; set; } = string.Empty;
    public IReadOnlyList<string> Highlights { get; set; } = [];
    public IReadOnlyList<string> InterviewQuestions { get; set; } = [];
    public bool UsedAi { get; set; }
    public string Note { get; set; } = string.Empty;
}
