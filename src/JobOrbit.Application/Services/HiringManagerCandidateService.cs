using JobOrbit.Application.DTOs.HiringManagerCandidates;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class HiringManagerCandidateService(
    IHiringManagerCandidateRepository repository,
    IFileStorageService storage) : IHiringManagerCandidateService
{
    public Task<PagedResultDto<HiringManagerCandidateListItemDto>> ListAsync(int userId, HiringManagerCandidateQuery query, CancellationToken token = default)
    {
        query.Page = Math.Max(1, query.Page);
        query.PageSize = Math.Clamp(query.PageSize, 1, 50);
        query.Sort = string.IsNullOrWhiteSpace(query.Sort) ? "newest" : query.Sort.Trim();
        return repository.ListAsync(userId, query, token);
    }

    public Task<HiringManagerCandidateDetailsDto?> DetailsAsync(int userId, int applicationId, CancellationToken token = default) =>
        repository.DetailsAsync(userId, applicationId, token);

    public Task<IReadOnlyList<HiringManagerDashboardCandidateDto>> LatestAsync(int userId, CancellationToken token = default) =>
        repository.LatestAsync(userId, token);

    public async Task<HiringManagerResumeDownloadDto?> DownloadResumeAsync(int userId, int applicationId, CancellationToken token = default)
    {
        var resume = await repository.ResumeAsync(userId, applicationId, token);
        if (resume is null) return null;
        var stream = await storage.OpenReadAsync(resume.StoredFileName, token);
        return stream is null ? null : new(stream, resume.ContentType, resume.OriginalFileName);
    }
}
