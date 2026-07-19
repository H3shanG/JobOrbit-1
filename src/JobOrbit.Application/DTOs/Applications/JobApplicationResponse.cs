namespace JobOrbit.Application.DTOs.Applications;

public sealed class JobApplicationResponse
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedOn { get; set; }
}
