using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    public int? UserId { get; set; }

    public string? ActorNameSnapshot { get; set; }
    public string? ActorRoleSnapshot { get; set; }

    public string EntityName { get; set; } = string.Empty;

    public int? EntityId { get; set; }
    public string? EntityDisplayName { get; set; }

    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }
    public string? Metadata { get; set; }

    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }
    public JobOrbit.Domain.Enums.AuditSeverity Severity { get; set; } = JobOrbit.Domain.Enums.AuditSeverity.Information;
    public bool IsSuccess { get; set; } = true;

    public User? User { get; set; }
}
