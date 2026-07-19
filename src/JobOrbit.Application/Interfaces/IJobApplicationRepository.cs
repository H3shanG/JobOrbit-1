using JobOrbit.Application.DTOs.Applications;

namespace JobOrbit.Application.Interfaces;

public interface IJobApplicationRepository
{
    Task<CreateApplicationResult> CreateAsync(
        int userId,
        int jobId,
        string coverLetter,
        int? resumeId,
        CancellationToken cancellationToken = default);
}
