namespace JobOrbit.Application.DTOs.Jobs;

public sealed class JobListItemDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime PostedOn { get; set; }
    public DateTime? ClosingDate { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = [];
    public bool HasApplied { get; set; }
}
