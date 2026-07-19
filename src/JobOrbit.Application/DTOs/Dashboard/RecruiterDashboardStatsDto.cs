namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class RecruiterDashboardStatsDto
{
    public int TotalJobs { get; init; }
    public int TotalApplications { get; init; }
    public int TotalCandidates { get; init; }
    public int InterviewsThisMonth { get; init; }
}
