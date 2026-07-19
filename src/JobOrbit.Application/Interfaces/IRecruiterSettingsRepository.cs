using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Interfaces;

public interface IRecruiterSettingsRepository
{
    Task<RecruiterProfile?> GetAsync(int userId, CancellationToken token = default);
    Task SaveAsync(CancellationToken token = default);
}
