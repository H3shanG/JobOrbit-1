using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.DTOs.Applications;

public sealed class CandidateApplicationQueryDto
{
    public ApplicationStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "newest";
}
