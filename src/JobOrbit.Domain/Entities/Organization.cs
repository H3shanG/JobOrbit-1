using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? WebsiteUrl { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivatedReason { get; set; }

    public string? Location { get; set; }

    public ICollection<Department> Departments { get; set; } = [];

    public ICollection<RecruiterProfile> Recruiters { get; set; } = [];
    public ICollection<HiringManagerProfile> HiringManagers { get; set; } = [];

    public ICollection<JobPosting> JobPostings { get; set; } = [];
}
