using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class JobApplicationService(IJobApplicationRepository repository)
    : IJobApplicationService
{
    public Task<CreateApplicationResult> ApplyAsync(
        int userId,
        int jobId,
        CreateJobApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        return repository.CreateAsync(
            userId,
            jobId,
            request.CoverLetter.Trim(),
            request.ResumeId,
            cancellationToken);
    }
}
