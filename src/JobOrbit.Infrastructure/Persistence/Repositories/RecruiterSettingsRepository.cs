using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class RecruiterSettingsRepository(JobOrbitDbContext db) : IRecruiterSettingsRepository
{
    public Task<RecruiterProfile?> GetAsync(int userId, CancellationToken token = default) =>
        db.RecruiterProfiles.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.UserId == userId && x.User.IsActive, token);

    public Task SaveAsync(CancellationToken token = default) => db.SaveChangesAsync(token);
}
