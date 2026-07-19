using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ICollection<CandidateSkill> CandidateSkills { get; set; } = [];

    public ICollection<JobSkill> JobSkills { get; set; } = [];
}
