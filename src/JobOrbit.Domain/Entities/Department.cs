using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class Department : BaseEntity
{
    public int OrganizationId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivatedReason { get; set; }

    public Organization Organization { get; set; } = null!;

    public ICollection<JobPosting> JobPostings { get; set; } = [];
    public ICollection<HiringManagerProfile> HiringManagers { get; set; } = [];
}
