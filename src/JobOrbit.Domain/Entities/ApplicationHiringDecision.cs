using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;
namespace JobOrbit.Domain.Entities;
public sealed class ApplicationHiringDecision : BaseEntity
{
    public int JobApplicationId { get; set; }
    public int DecidedByUserId { get; set; }
    public ManagerHiringDecision Decision { get; set; }
    public string? Notes { get; set; }
    public DateTime DecidedAt { get; set; }
    public JobApplication JobApplication { get; set; } = null!;
    public User DecidedByUser { get; set; } = null!;
}
