using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using JobOrbit.Domain;
using JobOrbit.Application.DTOs.Notifications;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class JobApplicationRepository(JobOrbitDbContext dbContext,ISystemSettingsProvider systemSettings, INotificationService notifications)
    : IJobApplicationRepository
{
    public async Task<CreateApplicationResult> CreateAsync(
        int userId,
        int jobId,
        string coverLetter,
        int? resumeId,
        CancellationToken cancellationToken = default)
    {
        var candidate = await dbContext.CandidateProfiles
            .AsNoTracking()
            .Where(profile => profile.UserId == userId)
            .Select(profile => new { profile.Id, profile.UserId, CandidateName=profile.User.FirstName+" "+profile.User.LastName, profile.ResumeUrl,profile.PhoneNumber,profile.Headline,profile.Summary,profile.Location,profile.Education,profile.Experience,HasResume=profile.Resumes.Any(),HasSkills=profile.CandidateSkills.Any() })
            .SingleOrDefaultAsync(cancellationToken);

        if (candidate is null)
        {
            return new(CreateApplicationOutcome.CandidateProfileMissing);
        }

        var recruitment=(await systemSettings.GetAsync(cancellationToken)).Recruitment;
        if(recruitment.RequireProfileCompletionBeforeApply)
        {
            var fields=new[]{candidate.PhoneNumber,candidate.Headline,candidate.Summary,candidate.Location,candidate.Education,candidate.Experience};
            var completed=fields.Count(x=>!string.IsNullOrWhiteSpace(x))+(candidate.HasResume||!string.IsNullOrWhiteSpace(candidate.ResumeUrl)?1:0)+(candidate.HasSkills?1:0);
            var percent=completed*100/8;
            if(percent<recruitment.MinimumProfileCompletionPercent)return new(CreateApplicationOutcome.ProfileIncomplete);
        }

        var now = DateTime.UtcNow;
        var job = await dbContext.JobPostings.AsNoTracking().Where(
            job => job.Id == jobId &&
                job.Status == JobStatus.Published &&
                (!job.ClosingAt.HasValue || job.ClosingAt > now)).Select(x=>new {x.Id,x.Title,RecruiterUserId=x.RecruiterProfile.UserId}).SingleOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            return new(CreateApplicationOutcome.JobUnavailable);
        }

        if (resumeId.HasValue && !await dbContext.Resumes.AsNoTracking().AnyAsync(x => x.Id == resumeId && x.CandidateProfileId == candidate.Id, cancellationToken))
            return new(CreateApplicationOutcome.InvalidResume);

        if (await dbContext.JobApplications.AsNoTracking().AnyAsync(
                application => application.CandidateProfileId == candidate.Id &&
                    application.JobPostingId == jobId,
                cancellationToken))
        {
            return new(CreateApplicationOutcome.Duplicate);
        }

        var application = new JobApplication
        {
            CandidateProfileId = candidate.Id,
            JobPostingId = jobId,
            CoverLetter = coverLetter,
            ResumeUrl = candidate.ResumeUrl,
            ResumeId = resumeId,
            Status = ApplicationStatus.Submitted,
            AppliedAt = now
        };
        dbContext.JobApplications.Add(application);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.GetBaseException() is SqlException { Number: 2601 or 2627 })
        {
            return new(CreateApplicationOutcome.Duplicate);
        }

        await notifications.CreateManyAsync([
            new(candidate.UserId, NotificationTypes.ApplicationSubmitted, "Application submitted", $"Your application for {job.Title} was submitted.", nameof(JobApplication), application.Id, $"/candidate/applications/{application.Id}", EventKey:$"application:{application.Id}:candidate:submitted"),
            new(job.RecruiterUserId, NotificationTypes.NewApplicationReceived, "New application received", $"{candidate.CandidateName} applied for {job.Title}.", nameof(JobApplication), application.Id, $"/recruiter/applicants/{application.Id}", EventKey:$"application:{application.Id}:recruiter:new")
        ], cancellationToken);

        return new(CreateApplicationOutcome.Created, new JobApplicationResponse
        {
            ApplicationId = application.Id,
            JobId = application.JobPostingId,
            Status = application.Status.ToString(),
            AppliedOn = application.AppliedAt
        });
    }
}
