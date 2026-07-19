using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class CandidateProfileRepository(JobOrbitDbContext dbContext)
    : ICandidateProfileRepository
{
    public async Task<User?> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.Include(x => x.CandidateProfile)
            .SingleOrDefaultAsync(x => x.Id == userId && x.Role == UserRole.Candidate, cancellationToken);
        if (user is null) return null;
        if (user.CandidateProfile is null)
        {
            user.CandidateProfile = new CandidateProfile();
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        return user;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
