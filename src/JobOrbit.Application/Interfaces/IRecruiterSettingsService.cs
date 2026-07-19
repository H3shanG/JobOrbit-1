using JobOrbit.Application.DTOs.RecruiterSettings;

namespace JobOrbit.Application.Interfaces;

public interface IRecruiterSettingsService
{
    Task<RecruiterSettingsDto?> GetAsync(int userId, CancellationToken token = default);
    Task<RecruiterSettingsDto?> UpdateAsync(int userId, UpdateRecruiterSettingsRequest request, CancellationToken token = default);
    Task<RecruiterPasswordOutcome> ChangePasswordAsync(int userId, ChangeRecruiterPasswordRequest request, CancellationToken token = default);
}
