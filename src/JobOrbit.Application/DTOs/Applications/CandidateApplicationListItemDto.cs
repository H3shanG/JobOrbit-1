namespace JobOrbit.Application.DTOs.Applications;

public sealed class CandidateApplicationListItemDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public DateTime? InterviewDate { get; set; }
}
