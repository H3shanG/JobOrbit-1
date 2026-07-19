using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Interfaces;

public interface ICandidateProfileRepository
{
    Task<User?> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
