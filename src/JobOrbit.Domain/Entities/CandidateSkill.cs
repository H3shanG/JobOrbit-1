using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class CandidateSkill : BaseEntity
{
    public int CandidateProfileId { get; set; }

    public int SkillId { get; set; }

    public int ProficiencyLevel { get; set; }

    public decimal? YearsOfExperience { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
