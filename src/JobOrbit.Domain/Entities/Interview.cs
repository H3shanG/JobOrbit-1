using JobOrbit.Domain.Common;
using JobOrbit.Domain.Enums;

namespace JobOrbit.Domain.Entities;

public sealed class Interview : BaseEntity
{
    public int JobApplicationId { get; set; }

    public DateTime ScheduledAt { get; set; }

    public int DurationMinutes { get; set; }

    public string? Location { get; set; }

    public string? MeetingUrl { get; set; }

    public string? Notes { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public JobApplication JobApplication { get; set; } = null!;

    public ICollection<CandidateEvaluation> CandidateEvaluations { get; set; } = [];
}
