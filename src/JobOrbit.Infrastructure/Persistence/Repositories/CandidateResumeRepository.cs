using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class CandidateResumeRepository(JobOrbitDbContext dbContext) : ICandidateResumeRepository
{
    public async Task<IReadOnlyList<Resume>> ListAsync(int userId, CancellationToken cancellationToken = default) => await dbContext.Resumes.AsNoTracking().Where(x => x.CandidateProfile.UserId == userId).OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UploadedAt).ToListAsync(cancellationToken);
    public Task<Resume?> GetAsync(int userId, int resumeId, CancellationToken cancellationToken = default) => dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == resumeId && x.CandidateProfile.UserId == userId, cancellationToken);
    public async Task<Resume?> AddAsync(int userId, Resume resume, CancellationToken cancellationToken = default)
    {
        var candidateId = await dbContext.CandidateProfiles.Where(x => x.UserId == userId).Select(x => (int?)x.Id).SingleOrDefaultAsync(cancellationToken);
        if (!candidateId.HasValue) return null;
        resume.CandidateProfileId = candidateId.Value;
        resume.IsDefault = !await dbContext.Resumes.AnyAsync(x => x.CandidateProfileId == candidateId, cancellationToken);
        dbContext.Resumes.Add(resume); await dbContext.SaveChangesAsync(cancellationToken); return resume;
    }
    public Task<bool> IsReferencedAsync(int resumeId, CancellationToken cancellationToken = default) => dbContext.JobApplications.AnyAsync(x => x.ResumeId == resumeId, cancellationToken);
    public async Task DeleteAsync(Resume resume, CancellationToken cancellationToken = default)
    {
        var wasDefault = resume.IsDefault; var candidateId = resume.CandidateProfileId;
        dbContext.Resumes.Remove(resume); await dbContext.SaveChangesAsync(cancellationToken);
        if (wasDefault)
        {
            var newest = await dbContext.Resumes.Where(x => x.CandidateProfileId == candidateId).OrderByDescending(x => x.UploadedAt).FirstOrDefaultAsync(cancellationToken);
            if (newest is not null) { newest.IsDefault = true; await dbContext.SaveChangesAsync(cancellationToken); }
        }
    }
    public async Task<bool> SetDefaultAsync(int userId, int resumeId, CancellationToken cancellationToken = default)
    {
        var target = await dbContext.Resumes.SingleOrDefaultAsync(x => x.Id == resumeId && x.CandidateProfile.UserId == userId, cancellationToken);
        if (target is null) return false;
        var currentDefaults = await dbContext.Resumes
            .Where(x => x.CandidateProfileId == target.CandidateProfileId && x.IsDefault)
            .ToListAsync(cancellationToken);
        foreach (var resume in currentDefaults) resume.IsDefault = false;
        target.IsDefault = true; await dbContext.SaveChangesAsync(cancellationToken); return true;
    }
}
