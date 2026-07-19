using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Services;

public sealed class CandidateResumeService(
    ICandidateResumeRepository repository,
    IFileStorageService storage,
    IResumeFileValidator fileValidator,
    ISystemSettingsProvider systemSettings) : ICandidateResumeService
{
    public const long MaximumBytes = 5 * 1024 * 1024;
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".doc"] = ["application/msword"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"]
    };

    public async Task<IReadOnlyList<CandidateResumeDto>> ListAsync(int userId, CancellationToken cancellationToken = default) =>
        (await repository.ListAsync(userId, cancellationToken)).Select(Map).ToList();

    public async Task<CandidateResumeDto?> UploadAsync(int userId, Stream content, string originalFileName, string contentType, long sizeBytes, string? displayName, CancellationToken cancellationToken = default)
    {
        var safeOriginal = Path.GetFileName(originalFileName);
        var extension = Path.GetExtension(safeOriginal).ToLowerInvariant();
        var uploadSettings=(await systemSettings.GetAsync(cancellationToken)).Uploads;
        var maximumBytes=uploadSettings.MaximumResumeSizeMb*1024L*1024L;
        if (sizeBytes <= 0 || sizeBytes > maximumBytes || !uploadSettings.AllowedResumeExtensions.Contains(extension,StringComparer.OrdinalIgnoreCase) || !Allowed.TryGetValue(extension, out var mimeTypes) || !mimeTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Only approved resume formats up to {uploadSettings.MaximumResumeSizeMb} MB are allowed.");
        if (!await fileValidator.IsValidAsync(content, extension, cancellationToken))
            throw new ArgumentException("The file content does not match the selected resume format.");
        var cleanDisplayName = string.IsNullOrWhiteSpace(displayName) ? Path.GetFileNameWithoutExtension(safeOriginal) : displayName.Trim();
        if (cleanDisplayName.Length is 0 or > 200)
            throw new ArgumentException("Display name must be between 1 and 200 characters.");

        var stored = await storage.SaveAsync(content, extension, cancellationToken);
        var resume = new Resume
        {
            DisplayName = cleanDisplayName,
            OriginalFileName = safeOriginal,
            StoredFileName = stored,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            UploadedAt = DateTime.UtcNow
        };
        try
        {
            var saved = await repository.AddAsync(userId, resume, cancellationToken);
            return saved is null ? null : Map(saved);
        }
        catch { await storage.DeleteAsync(stored, cancellationToken); throw; }
    }

    public async Task<ResumeDownloadDto?> DownloadAsync(int userId, int resumeId, CancellationToken cancellationToken = default)
    {
        var resume = await repository.GetAsync(userId, resumeId, cancellationToken);
        if (resume is null) return null;
        var stream = await storage.OpenReadAsync(resume.StoredFileName, cancellationToken);
        return stream is null ? null : new(stream, resume.ContentType, resume.OriginalFileName);
    }

    public async Task<DeleteResumeOutcome> DeleteAsync(int userId, int resumeId, CancellationToken cancellationToken = default)
    {
        var resume = await repository.GetAsync(userId, resumeId, cancellationToken);
        if (resume is null) return DeleteResumeOutcome.NotFound;
        if (await repository.IsReferencedAsync(resume.Id, cancellationToken)) return DeleteResumeOutcome.Referenced;
        await repository.DeleteAsync(resume, cancellationToken);
        await storage.DeleteAsync(resume.StoredFileName, cancellationToken);
        return DeleteResumeOutcome.Deleted;
    }

    public Task<bool> SetDefaultAsync(int userId, int resumeId, CancellationToken cancellationToken = default) => repository.SetDefaultAsync(userId, resumeId, cancellationToken);
    private static CandidateResumeDto Map(Resume x) => new() { ResumeId = x.Id, DisplayName = x.DisplayName, OriginalFileName = x.OriginalFileName, ContentType = x.ContentType, SizeBytes = x.SizeBytes, UploadedOn = x.UploadedAt, IsDefault = x.IsDefault };
}
