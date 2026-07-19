using JobOrbit.Domain.Common;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobOrbit.Infrastructure.Persistence.Configurations;

internal static class EntityConfiguration
{
    public static void ConfigureBase<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
        builder.Property(entity => entity.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();
    }
}

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Users");

        builder.Property(user => user.Email).HasMaxLength(320).IsRequired();
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(user => user.EmailNotifications).HasDefaultValue(true);
        builder.Property(user => user.ApplicationStatusNotifications).HasDefaultValue(true);
        builder.Property(user => user.InterviewReminders).HasDefaultValue(true);
        builder.Property(user => user.JobRecommendationNotifications).HasDefaultValue(true);
        builder.Property(user => user.Role).HasConversion<int>().IsRequired();
        builder.Property(user => user.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(user => user.Email).IsUnique();
    }
}

internal sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Notifications");
        builder.Property(x => x.Type).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(500).IsRequired();
        builder.Property(x => x.RelatedEntityType).HasMaxLength(80);
        builder.Property(x => x.ActionUrl).HasMaxLength(500);
        builder.Property(x => x.EventKey).HasMaxLength(200);
        builder.Property(x => x.Priority).HasConversion<int>().IsRequired();
        builder.HasIndex(x => x.RecipientUserId);
        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => new { x.RecipientUserId, x.EventKey }).IsUnique().HasFilter("[EventKey] IS NOT NULL");
        builder.HasOne(x => x.RecipientUser).WithMany(x => x.Notifications)
            .HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Permissions");
        builder.Property(x => x.Code).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(x => new { x.Role, x.PermissionId });
        builder.Property(x => x.Role).HasConversion<int>();
        builder.HasOne(x => x.Permission).WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CandidateProfileConfiguration
    : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("CandidateProfiles");

        builder.Property(profile => profile.PhoneNumber).HasMaxLength(30);
        builder.Property(profile => profile.Headline).HasMaxLength(200);
        builder.Property(profile => profile.Summary).HasMaxLength(2000);
        builder.Property(profile => profile.Location).HasMaxLength(200);
        builder.Property(profile => profile.ResumeUrl).HasMaxLength(1000);
        builder.Property(profile => profile.Education).HasMaxLength(4000);
        builder.Property(profile => profile.Experience).HasMaxLength(4000);
        builder.Property(profile => profile.LinkedInUrl).HasMaxLength(1000);
        builder.Property(profile => profile.PortfolioUrl).HasMaxLength(1000);

        builder.HasIndex(profile => profile.UserId).IsUnique();
        builder.HasOne(profile => profile.User)
            .WithOne(user => user.CandidateProfile)
            .HasForeignKey<CandidateProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RecruiterProfileConfiguration
    : IEntityTypeConfiguration<RecruiterProfile>
{
    public void Configure(EntityTypeBuilder<RecruiterProfile> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("RecruiterProfiles");

        builder.Property(profile => profile.JobTitle).HasMaxLength(150).IsRequired();
        builder.Property(profile => profile.PhoneNumber).HasMaxLength(30);
        builder.Property(profile => profile.JobApplicationNotifications).HasDefaultValue(true);
        builder.Property(profile => profile.InterviewNotifications).HasDefaultValue(true);
        builder.Property(profile => profile.CandidateStatusNotifications).HasDefaultValue(true);
        builder.Property(profile => profile.EmailNotifications).HasDefaultValue(true);

        builder.HasIndex(profile => profile.UserId).IsUnique();
        builder.HasOne(profile => profile.User)
            .WithOne(user => user.RecruiterProfile)
            .HasForeignKey<RecruiterProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(profile => profile.Organization)
            .WithMany(organization => organization.Recruiters)
            .HasForeignKey(profile => profile.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HiringManagerProfileConfiguration : IEntityTypeConfiguration<HiringManagerProfile>
{
    public void Configure(EntityTypeBuilder<HiringManagerProfile> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("HiringManagerProfiles");
        builder.Property(x => x.JobTitle).HasMaxLength(150).HasDefaultValue("Hiring Manager").IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(30);
        builder.Property(x => x.CandidateReviewNotifications).HasDefaultValue(true);
        builder.Property(x => x.InterviewNotifications).HasDefaultValue(true);
        builder.Property(x => x.EvaluationNotifications).HasDefaultValue(true);
        builder.Property(x => x.DecisionNotifications).HasDefaultValue(true);
        builder.Property(x => x.EmailNotifications).HasDefaultValue(true);
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.HasOne(x => x.User).WithOne(x => x.HiringManagerProfile).HasForeignKey<HiringManagerProfile>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Organization).WithMany(x => x.HiringManagers).HasForeignKey(x => x.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Department).WithMany(x => x.HiringManagers).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class OrganizationConfiguration
    : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Organizations");

        builder.Property(organization => organization.Name).HasMaxLength(200).IsRequired();
        builder.Property(organization => organization.Code).HasMaxLength(50).IsRequired();
        builder.Property(organization => organization.Description).HasMaxLength(2000);
        builder.Property(organization => organization.WebsiteUrl).HasMaxLength(1000);
        builder.Property(organization => organization.Location).HasMaxLength(200);
        builder.Property(organization => organization.Email).HasMaxLength(320);
        builder.Property(organization => organization.Phone).HasMaxLength(30);
        builder.Property(organization => organization.AddressLine1).HasMaxLength(250);
        builder.Property(organization => organization.AddressLine2).HasMaxLength(250);
        builder.Property(organization => organization.City).HasMaxLength(100);
        builder.Property(organization => organization.StateOrProvince).HasMaxLength(100);
        builder.Property(organization => organization.PostalCode).HasMaxLength(30);
        builder.Property(organization => organization.Country).HasMaxLength(100);
        builder.Property(organization => organization.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(organization => organization.DeactivatedReason).HasMaxLength(500);

        builder.HasIndex(organization => organization.Name).IsUnique();
        builder.HasIndex(organization => organization.Code).IsUnique();
    }
}

internal sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Departments");

        builder.Property(department => department.Name).HasMaxLength(150).IsRequired();
        builder.Property(department => department.Code).HasMaxLength(50).IsRequired();
        builder.Property(department => department.Description).HasMaxLength(1000);
        builder.Property(department => department.Email).HasMaxLength(320);
        builder.Property(department => department.Phone).HasMaxLength(30);
        builder.Property(department => department.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(department => department.DeactivatedReason).HasMaxLength(500);

        builder.HasIndex(department => new { department.OrganizationId, department.Name })
            .IsUnique();
        builder.HasIndex(department => new { department.OrganizationId, department.Code }).IsUnique();
        builder.HasOne(department => department.Organization)
            .WithMany(organization => organization.Departments)
            .HasForeignKey(department => department.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("JobPostings", table =>
        {
            table.HasCheckConstraint(
                "CK_JobPostings_SalaryRange",
                "[SalaryMinimum] IS NULL OR [SalaryMaximum] IS NULL OR [SalaryMinimum] <= [SalaryMaximum]");
        });

        builder.Property(job => job.Title).HasMaxLength(200).IsRequired();
        builder.Property(job => job.Description).HasMaxLength(8000).IsRequired();
        builder.Property(job => job.Responsibilities).HasMaxLength(8000);
        builder.Property(job => job.Requirements).HasMaxLength(8000);
        builder.Property(job => job.Location).HasMaxLength(200).IsRequired();
        builder.Property(job => job.EmploymentType).HasMaxLength(100).IsRequired();
        builder.Property(job => job.WorkplaceType).HasMaxLength(50);
        builder.Property(job => job.Currency).HasMaxLength(10).HasDefaultValue("LKR").IsRequired();
        builder.Property(job => job.ExperienceLevel).HasMaxLength(100);
        builder.Property(job => job.VacancyCount).HasDefaultValue(1).IsRequired();
        builder.Property(job => job.IsFeatured).HasDefaultValue(false).IsRequired();
        builder.Property(job => job.SalaryMinimum).HasPrecision(18, 2);
        builder.Property(job => job.SalaryMaximum).HasPrecision(18, 2);
        builder.Property(job => job.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(job => job.Organization)
            .WithMany(organization => organization.JobPostings)
            .HasForeignKey(job => job.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(job => job.Department)
            .WithMany(department => department.JobPostings)
            .HasForeignKey(job => job.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(job => job.RecruiterProfile)
            .WithMany(recruiter => recruiter.JobPostings)
            .HasForeignKey(job => job.RecruiterProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class JobApplicationConfiguration
    : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("JobApplications");

        builder.Property(application => application.CoverLetter).HasMaxLength(8000);
        builder.Property(application => application.ResumeUrl).HasMaxLength(1000);
        builder.Property(application => application.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(application => application.AppliedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .IsRequired();

        builder.HasIndex(application => new
        {
            application.CandidateProfileId,
            application.JobPostingId
        }).IsUnique();

        builder.HasOne(application => application.CandidateProfile)
            .WithMany(candidate => candidate.JobApplications)
            .HasForeignKey(application => application.CandidateProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(application => application.JobPosting)
            .WithMany(job => job.JobApplications)
            .HasForeignKey(application => application.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(application => application.Resume)
            .WithMany(resume => resume.JobApplications)
            .HasForeignKey(application => application.ResumeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Resumes", table => table.HasCheckConstraint("CK_Resumes_SizeBytes", "[SizeBytes] > 0"));
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.StoredFileName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.UploadedAt).IsRequired();
        builder.HasIndex(x => x.StoredFileName).IsUnique();
        builder.HasIndex(x => new { x.CandidateProfileId, x.IsDefault });
        builder.HasOne(x => x.CandidateProfile).WithMany(x => x.Resumes)
            .HasForeignKey(x => x.CandidateProfileId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Interviews", table =>
        {
            table.HasCheckConstraint("CK_Interviews_Duration", "[DurationMinutes] > 0");
        });

        builder.Property(interview => interview.Location).HasMaxLength(300);
        builder.Property(interview => interview.MeetingUrl).HasMaxLength(1000);
        builder.Property(interview => interview.Notes).HasMaxLength(4000);
        builder.Property(interview => interview.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(interview => interview.JobApplication)
            .WithMany(application => application.Interviews)
            .HasForeignKey(interview => interview.JobApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CandidateEvaluationConfiguration
    : IEntityTypeConfiguration<CandidateEvaluation>
{
    public void Configure(EntityTypeBuilder<CandidateEvaluation> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("CandidateEvaluations", table =>
        {
            table.HasCheckConstraint(
                "CK_CandidateEvaluations_OverallScore",
                "[OverallScore] >= 0 AND [OverallScore] <= 10");
            table.HasCheckConstraint("CK_CandidateEvaluations_TechnicalScore", "[TechnicalScore] IS NULL OR ([TechnicalScore] >= 1 AND [TechnicalScore] <= 10)");
            table.HasCheckConstraint("CK_CandidateEvaluations_CommunicationScore", "[CommunicationScore] IS NULL OR ([CommunicationScore] >= 1 AND [CommunicationScore] <= 10)");
            table.HasCheckConstraint("CK_CandidateEvaluations_ExperienceScore", "[ExperienceScore] IS NULL OR ([ExperienceScore] >= 1 AND [ExperienceScore] <= 10)");
            table.HasCheckConstraint("CK_CandidateEvaluations_CultureFitScore", "[CultureFitScore] IS NULL OR ([CultureFitScore] >= 1 AND [CultureFitScore] <= 10)");
        });

        builder.Property(evaluation => evaluation.OverallScore).HasPrecision(3, 2);
        builder.Property(evaluation => evaluation.Comments).HasMaxLength(4000);
        builder.Property(evaluation => evaluation.HiringDecision)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(evaluation => evaluation.Recommendation).HasConversion<int>();
        builder.HasIndex(evaluation => new { evaluation.JobApplicationId, evaluation.EvaluatorUserId }).IsUnique().HasFilter("[EvaluatorUserId] IS NOT NULL");

        builder.HasOne(evaluation => evaluation.JobApplication)
            .WithMany(application => application.CandidateEvaluations)
            .HasForeignKey(evaluation => evaluation.JobApplicationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(evaluation => evaluation.RecruiterProfile)
            .WithMany(recruiter => recruiter.CandidateEvaluations)
            .HasForeignKey(evaluation => evaluation.RecruiterProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(evaluation => evaluation.EvaluatorUser)
            .WithMany(user => user.AuthoredCandidateEvaluations)
            .HasForeignKey(evaluation => evaluation.EvaluatorUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(evaluation => evaluation.Interview)
            .WithMany(interview => interview.CandidateEvaluations)
            .HasForeignKey(evaluation => evaluation.InterviewId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("Skills");

        builder.Property(skill => skill.Name).HasMaxLength(150).IsRequired();
        builder.Property(skill => skill.Description).HasMaxLength(1000);

        builder.HasIndex(skill => skill.Name).IsUnique();
    }
}

internal sealed class ApplicationHiringDecisionConfiguration : IEntityTypeConfiguration<ApplicationHiringDecision>
{
    public void Configure(EntityTypeBuilder<ApplicationHiringDecision> builder)
    {
        EntityConfiguration.ConfigureBase(builder); builder.ToTable("ApplicationHiringDecisions");
        builder.Property(x => x.Decision).HasConversion<int>().IsRequired(); builder.Property(x => x.Notes).HasMaxLength(4000); builder.Property(x => x.DecidedAt).IsRequired();
        builder.HasIndex(x => x.JobApplicationId).IsUnique();
        builder.HasOne(x => x.JobApplication).WithOne(x => x.HiringDecision).HasForeignKey<ApplicationHiringDecision>(x => x.JobApplicationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DecidedByUser).WithMany(x => x.HiringDecisions).HasForeignKey(x => x.DecidedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CandidateSkillConfiguration
    : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("CandidateSkills", table =>
        {
            table.HasCheckConstraint(
                "CK_CandidateSkills_ProficiencyLevel",
                "[ProficiencyLevel] >= 1 AND [ProficiencyLevel] <= 5");
        });

        builder.Property(candidateSkill => candidateSkill.YearsOfExperience)
            .HasPrecision(4, 1);
        builder.HasIndex(candidateSkill => new
        {
            candidateSkill.CandidateProfileId,
            candidateSkill.SkillId
        }).IsUnique();

        builder.HasOne(candidateSkill => candidateSkill.CandidateProfile)
            .WithMany(candidate => candidate.CandidateSkills)
            .HasForeignKey(candidateSkill => candidateSkill.CandidateProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(candidateSkill => candidateSkill.Skill)
            .WithMany(skill => skill.CandidateSkills)
            .HasForeignKey(candidateSkill => candidateSkill.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class JobSkillConfiguration : IEntityTypeConfiguration<JobSkill>
{
    public void Configure(EntityTypeBuilder<JobSkill> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("JobSkills");

        builder.Property(jobSkill => jobSkill.MinimumYearsOfExperience)
            .HasPrecision(4, 1);
        builder.HasIndex(jobSkill => new { jobSkill.JobPostingId, jobSkill.SkillId })
            .IsUnique();

        builder.HasOne(jobSkill => jobSkill.JobPosting)
            .WithMany(job => job.JobSkills)
            .HasForeignKey(jobSkill => jobSkill.JobPostingId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(jobSkill => jobSkill.Skill)
            .WithMany(skill => skill.JobSkills)
            .HasForeignKey(jobSkill => jobSkill.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("AuditLogs");

        builder.Property(log => log.EntityName).HasMaxLength(200).IsRequired();
        builder.Property(log => log.Action).HasMaxLength(100).IsRequired();
        builder.Property(log => log.ActorNameSnapshot).HasMaxLength(250);
        builder.Property(log => log.ActorRoleSnapshot).HasMaxLength(50);
        builder.Property(log => log.EntityDisplayName).HasMaxLength(300);
        builder.Property(log => log.Description).HasMaxLength(1000);
        builder.Property(log => log.IpAddress).HasMaxLength(64);
        builder.Property(log => log.UserAgent).HasMaxLength(500);
        builder.Property(log => log.CorrelationId).HasMaxLength(100);
        builder.Property(log => log.Severity).HasDefaultValue(JobOrbit.Domain.Enums.AuditSeverity.Information);
        builder.Property(log => log.IsSuccess).HasDefaultValue(true);
        builder.Property(log => log.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(log => log.NewValues).HasColumnType("nvarchar(max)");
        builder.Property(log => log.Metadata).HasColumnType("nvarchar(max)");
        builder.HasIndex(log => log.CreatedAt);
        builder.HasIndex(log => log.Action);
        builder.HasIndex(log => new { log.EntityName, log.EntityId });
        builder.HasIndex(log => log.Severity);

        builder.HasOne(log => log.User)
            .WithMany(user => user.AuditLogs)
            .HasForeignKey(log => log.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        EntityConfiguration.ConfigureBase(builder);
        builder.ToTable("SystemSettings");
        builder.Property(x => x.Key).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Section).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.ValueType).HasMaxLength(30).HasDefaultValue("json").IsRequired();
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
