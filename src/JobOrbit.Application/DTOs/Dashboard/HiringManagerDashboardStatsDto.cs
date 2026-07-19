namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class HiringManagerDashboardStatsDto
{
    public int PendingReviews { get; init; }
    public int TodaysInterviews { get; init; }
    public int TeamFeedback { get; init; }
    public int HiredThisMonth { get; init; }
}
