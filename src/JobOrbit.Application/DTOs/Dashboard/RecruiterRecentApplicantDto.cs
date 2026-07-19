namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class RecruiterRecentApplicantDto
{
    public int ApplicationId { get; init; }
    public int CandidateId { get; init; }
    public string CandidateName { get; init; } = string.Empty;
    public int JobId { get; init; }
    public string JobTitle { get; init; } = string.Empty;
    public DateTime AppliedOn { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
}
