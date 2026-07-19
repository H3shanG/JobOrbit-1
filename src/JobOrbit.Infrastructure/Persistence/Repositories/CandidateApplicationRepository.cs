using JobOrbit.Application.DTOs.Applications;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class CandidateApplicationRepository(JobOrbitDbContext dbContext)
    : ICandidateApplicationRepository
{
    public async Task<CandidateApplicationsPageDto> GetApplicationsAsync(
        int userId,
        CandidateApplicationQueryDto request,
        CancellationToken cancellationToken = default)
    {
        var ownApplications = dbContext.JobApplications.AsNoTracking()
            .Where(application => application.CandidateProfile.UserId == userId);

        var summary = await ownApplications.GroupBy(_ => 1).Select(group =>
            new CandidateApplicationSummaryDto
            {
                Total = group.Count(),
                Pending = group.Count(x => x.Status == ApplicationStatus.Submitted || x.Status == ApplicationStatus.UnderReview),
                Shortlisted = group.Count(x => x.Status == ApplicationStatus.Shortlisted),
                Interviews = group.Count(x => x.Status == ApplicationStatus.Interviewing || x.Interviews.Any(i => i.Status == InterviewStatus.Scheduled)),
                Rejected = group.Count(x => x.Status == ApplicationStatus.Rejected)
            }).SingleOrDefaultAsync(cancellationToken) ?? new CandidateApplicationSummaryDto();

        var filtered = ownApplications;
        if (request.Status.HasValue) filtered = filtered.Where(x => x.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            filtered = filtered.Where(x => x.JobPosting.Title.Contains(search) ||
                x.JobPosting.Organization.Name.Contains(search) || x.JobPosting.Location.Contains(search));
        }

        var totalItems = await filtered.CountAsync(cancellationToken);
        filtered = request.Sort.ToLowerInvariant() == "oldest"
            ? filtered.OrderBy(x => x.AppliedAt).ThenBy(x => x.Id)
            : filtered.OrderByDescending(x => x.AppliedAt).ThenByDescending(x => x.Id);

        var rows = await filtered.Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize).Select(x => new
            {
                x.Id,
                x.JobPostingId,
                x.JobPosting.Title,
                CompanyName = x.JobPosting.Organization.Name,
                x.JobPosting.Location,
                x.JobPosting.EmploymentType,
                x.Status,
                x.AppliedAt,
                x.UpdatedAt,
                InterviewDate = x.Interviews.Where(i => i.Status == InterviewStatus.Scheduled)
                    .OrderBy(i => i.ScheduledAt).Select(i => (DateTime?)i.ScheduledAt).FirstOrDefault()
            }).ToListAsync(cancellationToken);

        return new CandidateApplicationsPageDto
        {
            Items = rows.Select(x => new CandidateApplicationListItemDto
            {
                ApplicationId = x.Id,
                JobId = x.JobPostingId,
                JobTitle = x.Title,
                CompanyName = x.CompanyName,
                Location = x.Location,
                EmploymentType = x.EmploymentType,
                Status = x.Status.ToString(),
                AppliedOn = x.AppliedAt,
                UpdatedOn = x.UpdatedAt,
                InterviewDate = x.InterviewDate
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize),
            Summary = summary
        };
    }

    public async Task<CandidateApplicationDetailsDto?> GetApplicationAsync(
        int userId,
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        var row = await dbContext.JobApplications.AsNoTracking()
            .Where(x => x.Id == applicationId && x.CandidateProfile.UserId == userId)
            .Select(x => new
            {
                x.Id,
                x.JobPostingId,
                JobTitle = x.JobPosting.Title,
                CompanyName = x.JobPosting.Organization.Name,
                DepartmentName = x.JobPosting.Department.Name,
                x.JobPosting.Location,
                x.JobPosting.EmploymentType,
                x.Status,
                x.CoverLetter,
                x.AppliedAt,
                x.UpdatedAt,
                Interview = x.Interviews.OrderByDescending(i => i.ScheduledAt).Select(i => new
                {
                    i.Id,
                    i.ScheduledAt,
                    i.Location,
                    i.MeetingUrl,
                    i.Status
                }).FirstOrDefault()
            }).SingleOrDefaultAsync(cancellationToken);

        if (row is null) return null;

        var timeline = new List<ApplicationTimelineItemDto>
        {
            new() { Status = ApplicationStatus.Submitted.ToString(), Date = row.AppliedAt, Description = "Application submitted" }
        };

        if (row.Status != ApplicationStatus.Submitted)
        {
            timeline.Add(new ApplicationTimelineItemDto
            {
                Status = row.Status.ToString(),
                Date = row.UpdatedAt,
                Description = DescribeStatus(row.Status)
            });
        }

        if (row.Interview is not null && row.Interview.Status is InterviewStatus.Scheduled or InterviewStatus.Rescheduled)
        {
            timeline.Add(new ApplicationTimelineItemDto
            {
                Status = "InterviewScheduled",
                Date = row.Interview.ScheduledAt,
                Description = "Interview scheduled"
            });
        }

        return new CandidateApplicationDetailsDto
        {
            ApplicationId = row.Id,
            JobId = row.JobPostingId,
            JobTitle = row.JobTitle,
            CompanyName = row.CompanyName,
            DepartmentName = row.DepartmentName,
            Location = row.Location,
            EmploymentType = row.EmploymentType,
            Status = row.Status.ToString(),
            CoverLetter = row.CoverLetter,
            AppliedOn = row.AppliedAt,
            UpdatedOn = row.UpdatedAt,
            Interview = row.Interview is null ? null : new CandidateInterviewSummaryDto
            {
                InterviewId = row.Interview.Id,
                ScheduledAt = row.Interview.ScheduledAt,
                Location = row.Interview.Location,
                MeetingLink = row.Interview.MeetingUrl,
                Status = row.Interview.Status.ToString()
            },
            Timeline = timeline.OrderBy(x => x.Date).ToList()
        };
    }

    private static string DescribeStatus(ApplicationStatus status) => status switch
    {
        ApplicationStatus.UnderReview => "Application under review",
        ApplicationStatus.Shortlisted => "Application shortlisted",
        ApplicationStatus.Interviewing => "Interview stage reached",
        ApplicationStatus.Offered => "Offer received",
        ApplicationStatus.Hired => "Application successful",
        ApplicationStatus.Rejected => "Application not selected",
        ApplicationStatus.Withdrawn => "Application withdrawn",
        _ => "Application updated"
    };
}
