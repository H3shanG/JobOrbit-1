using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.RecruiterJobs;

public sealed class CreateRecruiterJobRequest : IValidatableObject
{
    [Required, StringLength(200)] public string Title { get; init; } = string.Empty;
    [Range(1,int.MaxValue)] public int DepartmentId { get; init; }
    [Required, StringLength(200)] public string Location { get; init; } = string.Empty;
    [Required, StringLength(100)] public string EmploymentType { get; init; } = string.Empty;
    [Required, StringLength(8000)] public string Description { get; init; } = string.Empty;
    [StringLength(8000)] public string? Responsibilities { get; init; }
    [StringLength(8000)] public string? Requirements { get; init; }
    [Range(0,double.MaxValue)] public decimal? MinimumSalary { get; init; }
    [Range(0,double.MaxValue)] public decimal? MaximumSalary { get; init; }
    [Required] public DateTime ClosingDate { get; init; }
    public IReadOnlyList<int> SkillIds { get; init; } = [];
    public bool PublishNow { get; init; }
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ClosingDate <= DateTime.UtcNow) yield return new("Closing date must be in the future.",[nameof(ClosingDate)]);
        if (MinimumSalary.HasValue&&MaximumSalary.HasValue&&MinimumSalary>MaximumSalary) yield return new("Minimum salary must not exceed maximum salary.",[nameof(MinimumSalary),nameof(MaximumSalary)]);
        if (SkillIds.Any(x=>x<=0)) yield return new("Skill IDs must be positive.",[nameof(SkillIds)]);
    }
}
