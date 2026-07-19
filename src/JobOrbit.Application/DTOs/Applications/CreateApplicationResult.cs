namespace JobOrbit.Application.DTOs.Applications;

public enum CreateApplicationOutcome
{
    Created,
    JobUnavailable,
    CandidateProfileMissing,
    Duplicate,
    InvalidResume
    ,ProfileIncomplete
}

public sealed record CreateApplicationResult(
    CreateApplicationOutcome Outcome,
    JobApplicationResponse? Application = null);
