namespace JobOrbit.Application.DTOs.Jobs;

public sealed class JobDetailsDto
{
    public int JobId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }
    public string? CompanySummary { get; set; }
    public decimal? MinimumSalary { get; set; }
    public decimal? MaximumSalary { get; set; }
    public DateTime PostedOn { get; set; }
    public DateTime? ClosingDate { get; set; }
    public IReadOnlyList<string> Skills { get; set; } = [];
    public bool HasApplied { get; set; }
    public int? ApplicationId { get; set; }
}
