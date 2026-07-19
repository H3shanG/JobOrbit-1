using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class JobRepository(JobOrbitDbContext dbContext) : IJobRepository
{
    public async Task<PagedResultDto<JobListItemDto>> GetPublishedJobsAsync(
        int userId,
        JobListQueryDto request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var query = dbContext.JobPostings.AsNoTracking().Where(job =>
            job.Status == JobStatus.Published &&
            (!job.ClosingAt.HasValue || job.ClosingAt > now));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(job => job.Title.Contains(search) ||
                job.Organization.Name.Contains(search) ||
                job.Description.Contains(search) ||
                job.Location.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            var location = request.Location.Trim();
            query = query.Where(job => job.Location.Contains(location));
        }

        if (!string.IsNullOrWhiteSpace(request.EmploymentType))
        {
            var employmentType = request.EmploymentType.Trim();
            query = query.Where(job => job.EmploymentType == employmentType);
        }

        var totalItems = await query.CountAsync(cancellationToken);
        query = request.Sort.ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(job => job.PublishedAt ?? job.CreatedAt)
                .ThenByDescending(job => job.Id),
            "closing" => query.OrderBy(job => job.ClosingAt ?? DateTime.MaxValue)
                .ThenByDescending(job => job.Id),
            _ => query.OrderByDescending(job => job.PublishedAt ?? job.CreatedAt)
                .ThenByDescending(job => job.Id)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(job => new JobListItemDto
            {
                JobId = job.Id,
                Title = job.Title,
                CompanyName = job.Organization.Name,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                Summary = job.Description,
                PostedOn = job.PublishedAt ?? job.CreatedAt,
                ClosingDate = job.ClosingAt,
                Skills = job.JobSkills.OrderByDescending(x => x.IsRequired)
                    .ThenBy(x => x.Skill.Name).Select(x => x.Skill.Name).ToList(),
                HasApplied = job.JobApplications.Any(application =>
                    application.CandidateProfile.UserId == userId)
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<JobListItemDto>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }

    public Task<JobDetailsDto?> GetPublishedJobAsync(
        int userId,
        int jobId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return dbContext.JobPostings
            .AsNoTracking()
            .Where(job => job.Id == jobId &&
                job.Status == JobStatus.Published &&
                (!job.ClosingAt.HasValue || job.ClosingAt > now))
            .Select(job => new JobDetailsDto
            {
                JobId = job.Id,
                Title = job.Title,
                CompanyName = job.Organization.Name,
                DepartmentName = job.Department.Name,
                Location = job.Location,
                EmploymentType = job.EmploymentType,
                Description = job.Description,
                Responsibilities = job.Responsibilities,
                Requirements = job.Requirements,
                CompanySummary = job.Organization.Description,
                MinimumSalary = job.SalaryMinimum,
                MaximumSalary = job.SalaryMaximum,
                PostedOn = job.PublishedAt ?? job.CreatedAt,
                ClosingDate = job.ClosingAt,
                Skills = job.JobSkills.OrderByDescending(x => x.IsRequired)
                    .ThenBy(x => x.Skill.Name).Select(x => x.Skill.Name).ToList(),
                HasApplied = job.JobApplications.Any(application =>
                    application.CandidateProfile.UserId == userId),
                ApplicationId = job.JobApplications
                    .Where(application => application.CandidateProfile.UserId == userId)
                    .Select(application => (int?)application.Id)
                    .FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
