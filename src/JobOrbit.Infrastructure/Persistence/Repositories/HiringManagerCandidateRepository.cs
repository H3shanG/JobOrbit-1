using JobOrbit.Application.DTOs.HiringManagerCandidates;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class HiringManagerCandidateRepository(JobOrbitDbContext db) : IHiringManagerCandidateRepository
{
    private async Task<(int OrganizationId, int? DepartmentId)?> ScopeAsync(int userId, CancellationToken token)
    {
        var scope = await db.HiringManagerProfiles.AsNoTracking()
            .Where(x => x.UserId == userId && x.User.IsActive && x.User.Role == UserRole.HiringManager)
            .Select(x => new { x.OrganizationId, x.DepartmentId }).SingleOrDefaultAsync(token);
        return scope is null ? null : (scope.OrganizationId, scope.DepartmentId);
    }

    private IQueryable<Domain.Entities.JobApplication> ScopedApplications((int OrganizationId, int? DepartmentId) scope) =>
        db.JobApplications.Where(x => x.JobPosting.OrganizationId == scope.OrganizationId && (!scope.DepartmentId.HasValue || x.JobPosting.DepartmentId == scope.DepartmentId));

    private static IQueryable<Domain.Entities.JobApplication> Reviewable(IQueryable<Domain.Entities.JobApplication> query) =>
        query.Where(x => x.Status == ApplicationStatus.Shortlisted || x.Status == ApplicationStatus.Interviewing);

    public async Task<PagedResultDto<HiringManagerCandidateListItemDto>> ListAsync(int userId, HiringManagerCandidateQuery query, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token);
        if (scope is null) return new() { Items = [], Page = query.Page, PageSize = query.PageSize };
        var applications = Reviewable(ScopedApplications(scope.Value)).AsNoTracking();
        if (query.JobId.HasValue) applications = applications.Where(x => x.JobPostingId == query.JobId);
        if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ApplicationStatus>(query.Status, true, out var status)) applications = applications.Where(x => x.Status == status);
        if (!string.IsNullOrWhiteSpace(query.Search)) { var term = query.Search.Trim(); applications = applications.Where(x => (x.CandidateProfile.User.FirstName + " " + x.CandidateProfile.User.LastName).Contains(term) || (x.CandidateProfile.Headline ?? "").Contains(term) || x.JobPosting.Title.Contains(term)); }
        applications = query.Sort.ToLowerInvariant() switch { "oldest" => applications.OrderBy(x => x.AppliedAt), "name" => applications.OrderBy(x => x.CandidateProfile.User.FirstName).ThenBy(x => x.CandidateProfile.User.LastName), _ => applications.OrderByDescending(x => x.AppliedAt) };
        var total = await applications.CountAsync(token);
        var items = await applications.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(x => new HiringManagerCandidateListItemDto
        {
            ApplicationId = x.Id, CandidateId = x.CandidateProfileId, CandidateName = x.CandidateProfile.User.FirstName + " " + x.CandidateProfile.User.LastName,
            ProfessionalTitle = x.CandidateProfile.Headline, JobId = x.JobPostingId, JobTitle = x.JobPosting.Title, Status = x.Status.ToString(), AppliedOn = x.AppliedAt,
            MatchScore = null, InterviewStatus = x.Interviews.OrderByDescending(i => i.ScheduledAt).Select(i => i.Status.ToString()).FirstOrDefault(),
            EvaluationStatus = x.CandidateEvaluations.Any(e => e.EvaluatorUserId == userId) ? "Completed" : "Pending"
        }).ToListAsync(token);
        return new() { Items = items, Page = query.Page, PageSize = query.PageSize, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)query.PageSize) };
    }

    public async Task<HiringManagerCandidateDetailsDto?> DetailsAsync(int userId, int applicationId, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return null;
        var row = await Reviewable(ScopedApplications(scope.Value)).AsNoTracking().Where(x => x.Id == applicationId).Select(x => new
        {
            x.Id, x.Status, x.AppliedAt, x.CoverLetter, CandidateId = x.CandidateProfileId, FullName = x.CandidateProfile.User.FirstName + " " + x.CandidateProfile.User.LastName,
            x.CandidateProfile.User.Email, Phone = x.CandidateProfile.PhoneNumber, Title = x.CandidateProfile.Headline, Summary = x.CandidateProfile.Summary, x.CandidateProfile.Education, x.CandidateProfile.Experience, x.CandidateProfile.LinkedInUrl, x.CandidateProfile.PortfolioUrl,
            Skills = x.CandidateProfile.CandidateSkills.Select(s => s.Skill.Name).ToList(), JobId = x.JobPostingId, JobTitle = x.JobPosting.Title, Department = x.JobPosting.Department.Name, x.JobPosting.Location, x.JobPosting.EmploymentType, x.JobPosting.Requirements,
            Resume = x.Resume == null ? null : new { x.Resume.Id, x.Resume.DisplayName, x.Resume.OriginalFileName },
            Interview = x.Interviews.OrderByDescending(i => i.ScheduledAt).Select(i => new { i.Id, i.ScheduledAt, i.DurationMinutes, i.Location, i.MeetingUrl, i.Status }).FirstOrDefault(),
            Evaluation = x.CandidateEvaluations.Where(e => e.EvaluatorUserId == userId).Select(e => new { e.Id, e.OverallScore, e.Comments, e.Recommendation, e.CreatedAt, EvaluatorName = e.EvaluatorUser!.FirstName + " " + e.EvaluatorUser.LastName }).FirstOrDefault()
        }).SingleOrDefaultAsync(token);
        if (row is null) return null;
        return new() { ApplicationId = row.Id, Status = row.Status.ToString(), AppliedOn = row.AppliedAt, CoverLetter = row.CoverLetter,
            Candidate = new() { CandidateId = row.CandidateId, FullName = row.FullName, Email = row.Email, Phone = row.Phone, ProfessionalTitle = row.Title, ProfessionalSummary = row.Summary, Education = row.Education, Experience = row.Experience, LinkedInUrl = row.LinkedInUrl, PortfolioUrl = row.PortfolioUrl, Skills = row.Skills },
            Job = new() { JobId = row.JobId, Title = row.JobTitle, DepartmentName = row.Department, Location = row.Location, EmploymentType = row.EmploymentType, Requirements = row.Requirements },
            Resume = row.Resume is null ? null : new() { ResumeId = row.Resume.Id, DisplayName = row.Resume.DisplayName, OriginalFileName = row.Resume.OriginalFileName },
            Interview = row.Interview is null ? null : new() { InterviewId = row.Interview.Id, ScheduledAt = row.Interview.ScheduledAt, DurationMinutes = row.Interview.DurationMinutes, Location = row.Interview.Location, MeetingLink = row.Interview.MeetingUrl, Status = row.Interview.Status.ToString() },
            ExistingEvaluation = row.Evaluation is null ? null : new() { EvaluationId = row.Evaluation.Id, OverallScore = row.Evaluation.OverallScore, Comments = row.Evaluation.Comments, HiringDecision = row.Evaluation.Recommendation?.ToString() ?? "Pending", EvaluatorName = row.Evaluation.EvaluatorName, EvaluatedAt = row.Evaluation.CreatedAt, CanEdit = true } };
    }

    public async Task<HiringManagerResumeFileDto?> ResumeAsync(int userId, int applicationId, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return null;
        return await Reviewable(ScopedApplications(scope.Value)).AsNoTracking().Where(x => x.Id == applicationId && x.Resume != null).Select(x => new HiringManagerResumeFileDto(x.Resume!.StoredFileName, x.Resume.ContentType, x.Resume.OriginalFileName)).SingleOrDefaultAsync(token);
    }

    public async Task<IReadOnlyList<HiringManagerDashboardCandidateDto>> LatestAsync(int userId, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return [];
        return await Reviewable(ScopedApplications(scope.Value)).AsNoTracking().OrderByDescending(x => x.AppliedAt).Take(3).Select(x => new HiringManagerDashboardCandidateDto { ApplicationId = x.Id, CandidateName = x.CandidateProfile.User.FirstName + " " + x.CandidateProfile.User.LastName, ProfessionalTitle = x.CandidateProfile.Headline, JobTitle = x.JobPosting.Title, AppliedOn = x.AppliedAt }).ToListAsync(token);
    }
}
