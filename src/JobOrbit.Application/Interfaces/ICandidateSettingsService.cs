using JobOrbit.Application.DTOs.Candidates;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateSettingsService
{
    Task<CandidateSettingsDto?> GetAsync(int userId, CancellationToken token = default);
    Task<CandidateSettingsDto?> UpdateAsync(int userId, UpdateCandidateSettingsRequest request, CancellationToken token = default);
    Task<ChangePasswordOutcome> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken token = default);
}

public enum ChangePasswordOutcome { Changed, NotFound, IncorrectCurrentPassword }
