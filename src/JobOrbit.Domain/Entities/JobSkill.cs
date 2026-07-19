using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class JobSkill : BaseEntity
{
    public int JobPostingId { get; set; }

    public int SkillId { get; set; }

    public bool IsRequired { get; set; }

    public decimal? MinimumYearsOfExperience { get; set; }

    public JobPosting JobPosting { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
