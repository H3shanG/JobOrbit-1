using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.API.Services;

public interface IJobAssistantService
{
    Task<JobAssistantResponse> GenerateAsync(
        JobDetailsDto job,
        string mode,
        CancellationToken cancellationToken = default);
}
