using JobOrbit.Domain.Common;

namespace JobOrbit.Domain.Entities;

public sealed class Resume : BaseEntity
{
    public int CandidateProfileId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsDefault { get; set; }
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public ICollection<JobApplication> JobApplications { get; set; } = [];
}
