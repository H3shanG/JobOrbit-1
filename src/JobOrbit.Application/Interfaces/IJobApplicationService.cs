using JobOrbit.Application.DTOs.Applications;

namespace JobOrbit.Application.Interfaces;

public interface IJobApplicationService
{
    Task<CreateApplicationResult> ApplyAsync(
        int userId,
        int jobId,
        CreateJobApplicationRequest request,
        CancellationToken cancellationToken = default);
}
