namespace JobOrbit.Application.DTOs.Jobs;

public sealed class JobListQueryDto
{
    public string? Search { get; set; }
    public string? Location { get; set; }
    public string? EmploymentType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "newest";
}
