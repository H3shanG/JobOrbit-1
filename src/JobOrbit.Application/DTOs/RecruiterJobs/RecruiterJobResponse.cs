namespace JobOrbit.Application.DTOs.RecruiterJobs;
public sealed class RecruiterJobResponse { public int JobId { get; init; } public string Title { get; init; }=string.Empty; public string Status { get; init; }=string.Empty; public DateTime CreatedAt { get; init; } }
public sealed class RecruiterReferenceDto { public int Id { get; init; } public string Name { get; init; }=string.Empty; }
public sealed record CreateRecruiterJobResult(CreateRecruiterJobOutcome Outcome, RecruiterJobResponse? Job=null);
public enum CreateRecruiterJobOutcome { Created, RecruiterProfileMissing, InvalidDepartment, InvalidSkills }
