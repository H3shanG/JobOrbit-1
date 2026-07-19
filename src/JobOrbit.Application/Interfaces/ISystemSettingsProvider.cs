using JobOrbit.Application.DTOs.AdminSystemSettings;

namespace JobOrbit.Application.Interfaces;

public interface ISystemSettingsProvider
{
    Task<SystemSettingsDto> GetAsync(CancellationToken token = default);
    Task<object> UpdateSectionAsync(string section, object value, int actorUserId, CancellationToken token = default);
    Task<object?> ResetSectionAsync(string section, int actorUserId, CancellationToken token = default);
    Task SeedDefaultsAsync(CancellationToken token = default);
}
