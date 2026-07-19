using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class JobPosting : BaseEntity
{
    public int OrganizationId { get; set; }

    public int DepartmentId { get; set; }

    public int RecruiterProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string? Responsibilities { get; set; }
    public string? Requirements { get; set; }

    public string Location { get; set; } = string.Empty;

    public string EmploymentType { get; set; } = string.Empty;
    public string? WorkplaceType { get; set; }
    public string Currency { get; set; } = "LKR";
    public string? ExperienceLevel { get; set; }
    public int VacancyCount { get; set; } = 1;
    public bool IsFeatured { get; set; }

    public decimal? SalaryMinimum { get; set; }

    public decimal? SalaryMaximum { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Draft;

    public DateTime? PublishedAt { get; set; }

    public DateTime? ClosingAt { get; set; }

    public Organization Organization { get; set; } = null!;

    public Department Department { get; set; } = null!;

    public RecruiterProfile RecruiterProfile { get; set; } = null!;

    public ICollection<JobApplication> JobApplications { get; set; } = [];

    public ICollection<JobSkill> JobSkills { get; set; } = [];
}
