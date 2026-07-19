using JobOrbit.Application.DTOs.Candidates;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateProfileService
{
    Task<CandidateProfileDto?> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<CandidateProfileDto?> UpdateAsync(int userId, UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default);
}
