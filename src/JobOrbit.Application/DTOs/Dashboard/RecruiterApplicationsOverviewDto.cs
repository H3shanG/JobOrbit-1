namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class RecruiterApplicationsOverviewDto
{
    public IReadOnlyList<RecruiterApplicationsOverviewMonthDto> Months { get; init; } = [];
}

public sealed class RecruiterApplicationsOverviewMonthDto
{
    public string Month { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public int TotalApplications { get; init; }
    public int Shortlisted { get; init; }
    public int Rejected { get; init; }
    public int InterviewScheduled { get; init; }
    public int Hired { get; init; }
}
