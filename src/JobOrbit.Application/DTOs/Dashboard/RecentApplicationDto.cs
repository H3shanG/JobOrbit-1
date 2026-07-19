namespace JobOrbit.Application.DTOs.Dashboard;

public sealed class RecentApplicationDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedOn { get; set; }
}
