using JobOrbit.Application.DTOs.Candidates;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateResumeService
{
    Task<IReadOnlyList<CandidateResumeDto>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<CandidateResumeDto?> UploadAsync(int userId, Stream content, string originalFileName, string contentType, long sizeBytes, string? displayName, CancellationToken cancellationToken = default);
    Task<ResumeDownloadDto?> DownloadAsync(int userId, int resumeId, CancellationToken cancellationToken = default);
    Task<DeleteResumeOutcome> DeleteAsync(int userId, int resumeId, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAsync(int userId, int resumeId, CancellationToken cancellationToken = default);
}
