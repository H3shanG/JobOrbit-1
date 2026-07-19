using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateResumeRepository
{
    Task<IReadOnlyList<Resume>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<Resume?> GetAsync(int userId, int resumeId, CancellationToken cancellationToken = default);
    Task<Resume?> AddAsync(int userId, Resume resume, CancellationToken cancellationToken = default);
    Task<bool> IsReferencedAsync(int resumeId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Resume resume, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAsync(int userId, int resumeId, CancellationToken cancellationToken = default);
}
