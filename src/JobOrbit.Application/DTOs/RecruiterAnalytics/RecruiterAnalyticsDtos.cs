namespace JobOrbit.Application.DTOs.RecruiterAnalytics;

public sealed class RecruiterAnalyticsDto
{
    public RecruiterAnalyticsSummaryDto Summary { get; init; } = new();
    public RecruiterConversionRatesDto ConversionRates { get; init; } = new();
    public IReadOnlyList<RecruiterAnalyticsTrendDto> ApplicationsTrend { get; init; } = [];
    public IReadOnlyList<RecruiterApplicationStatusCountDto> ApplicationsByStatus { get; init; } = [];
    public IReadOnlyList<RecruiterTopJobDto> TopJobs { get; init; } = [];
}

public sealed class RecruiterAnalyticsSummaryDto
{
    public int TotalJobs { get; init; }
    public int PublishedJobs { get; init; }
    public int TotalApplications { get; init; }
    public int ShortlistedCandidates { get; init; }
    public int InterviewsScheduled { get; init; }
    public int OffersMade { get; init; }
    public int HiredCandidates { get; init; }
    public int RejectedApplications { get; init; }
}

public sealed class RecruiterConversionRatesDto
{
    public decimal ApplicationToShortlistRate { get; init; }
    public decimal ShortlistToInterviewRate { get; init; }
    public decimal InterviewToHireRate { get; init; }
}

public sealed class RecruiterAnalyticsTrendDto
{
    public string Period { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int Applications { get; init; }
    public int Shortlisted { get; init; }
    public int Interviews { get; init; }
    public int Hired { get; init; }
}

public sealed class RecruiterApplicationStatusCountDto
{
    public string Status { get; init; } = string.Empty;
    public int Count { get; init; }
}

public sealed class RecruiterTopJobDto
{
    public int JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public int ApplicationCount { get; init; }
    public int ShortlistedCount { get; init; }
    public int InterviewCount { get; init; }
    public int HiredCount { get; init; }
}
