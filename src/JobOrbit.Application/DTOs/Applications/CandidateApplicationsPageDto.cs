namespace JobOrbit.Application.DTOs.Applications;

public sealed class CandidateApplicationsPageDto
{
    public IReadOnlyList<CandidateApplicationListItemDto> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public CandidateApplicationSummaryDto Summary { get; set; } = new();
}

public sealed class CandidateApplicationSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Shortlisted { get; set; }
    public int Interviews { get; set; }
    public int Rejected { get; set; }
}
