namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class CandidateDashboardStatsDto
{
    public int JobsApplied { get; init; }

    public int Interviews { get; init; }

    public int Shortlisted { get; init; }

    public int Pending { get; init; }
}
