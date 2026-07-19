using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class CandidateProfile : BaseEntity
{
    public int UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Headline { get; set; }

    public string? Summary { get; set; }

    public string? Location { get; set; }

    public string? ResumeUrl { get; set; }

    public string? Education { get; set; }

    public string? Experience { get; set; }

    public string? LinkedInUrl { get; set; }

    public string? PortfolioUrl { get; set; }

    public User User { get; set; } = null!;

    public ICollection<JobApplication> JobApplications { get; set; } = [];

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];

    public ICollection<Resume> Resumes { get; set; } = [];
}
