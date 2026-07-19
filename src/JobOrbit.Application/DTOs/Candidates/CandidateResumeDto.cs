namespace JobOrbit.Application.DTOs.Candidates;

public sealed class CandidateResumeDto
{
    public int ResumeId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedOn { get; set; }
    public bool IsDefault { get; set; }
}

public sealed record ResumeDownloadDto(Stream Content, string ContentType, string OriginalFileName);
public enum DeleteResumeOutcome { Deleted, NotFound, Referenced }
