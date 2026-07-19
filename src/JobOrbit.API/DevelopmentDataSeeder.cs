using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using JobOrbit.Application.Authorization;
using JobOrbit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.API;

public sealed class DevelopmentDataSeeder(
    JobOrbitDbContext dbContext,
    IPasswordHasher<User> passwordHasher)
{
    private const string OrganizationName = "NovaTech Solutions";
    private const string RecruiterEmail = "recruiter@joborbit.test";
    private const string RecruiterPassword = "Recruiter@123";
    private const string ManagerEmail = "manager@joborbit.test";
    private const string ManagerPassword = "Manager@123";
    private const string AdminEmail = "admin@joborbit.test";
    private const string AdminPassword = "Admin@123";

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedPermissionsAsync(cancellationToken);
        var organization = await dbContext.Organizations
            .SingleOrDefaultAsync(x => x.Name == OrganizationName, cancellationToken);

        if (organization is null)
        {
            organization = new Organization
            {
                Name = OrganizationName,
                Code = "NOVATECH",
                Description = "Technology and digital solutions company",
                Location = "Colombo"
            };
            dbContext.Organizations.Add(organization);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var department = await dbContext.Departments.SingleOrDefaultAsync(
            x => x.OrganizationId == organization.Id && x.Name == "Engineering",
            cancellationToken);

        if (department is null)
        {
            department = new Department
            {
                OrganizationId = organization.Id,
                Name = "Engineering"
                ,Code = "ENG"
            };
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var recruiter = await dbContext.Users
            .SingleOrDefaultAsync(x => x.Email == RecruiterEmail, cancellationToken);

        if (recruiter is null)
        {
            recruiter = new User
            {
                FirstName = "Sarah",
                LastName = "Fernando",
                Email = RecruiterEmail,
                Role = UserRole.Recruiter,
                IsActive = true
            };
            recruiter.PasswordHash = passwordHasher.HashPassword(recruiter, RecruiterPassword);
            dbContext.Users.Add(recruiter);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (passwordHasher.VerifyHashedPassword(
                     recruiter,
                     recruiter.PasswordHash,
                     RecruiterPassword) == PasswordVerificationResult.Failed)
        {
            recruiter.PasswordHash = passwordHasher.HashPassword(recruiter, RecruiterPassword);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var recruiterProfile = await dbContext.RecruiterProfiles
            .SingleOrDefaultAsync(x => x.UserId == recruiter.Id, cancellationToken);

        if (recruiterProfile is null)
        {
            recruiterProfile = new RecruiterProfile
            {
                UserId = recruiter.Id,
                OrganizationId = organization.Id,
                JobTitle = "Technical Recruiter"
            };
            dbContext.RecruiterProfiles.Add(recruiterProfile);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var manager = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == ManagerEmail, cancellationToken);
        if (manager is null)
        {
            manager = new User
            {
                FirstName = "Dr. Sampath",
                LastName = "Perera",
                Email = ManagerEmail,
                Role = UserRole.HiringManager,
                IsActive = true
            };
            manager.PasswordHash = passwordHasher.HashPassword(manager, ManagerPassword);
            dbContext.Users.Add(manager);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (passwordHasher.VerifyHashedPassword(manager, manager.PasswordHash, ManagerPassword) == PasswordVerificationResult.Failed)
        {
            manager.PasswordHash = passwordHasher.HashPassword(manager, ManagerPassword);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.HiringManagerProfiles.AnyAsync(x => x.UserId == manager.Id, cancellationToken))
        {
            dbContext.HiringManagerProfiles.Add(new HiringManagerProfile
            {
                UserId = manager.Id,
                OrganizationId = organization.Id,
                DepartmentId = department.Id
                ,JobTitle = "Hiring Manager"
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var admin = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == AdminEmail, cancellationToken);
        if (admin is null)
        {
            admin = new User { FirstName = "System", LastName = "Administrator", Email = AdminEmail, Role = UserRole.Administrator, IsActive = true };
            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            dbContext.Users.Add(admin);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, AdminPassword) == PasswordVerificationResult.Failed)
        {
            admin.PasswordHash = passwordHasher.HashPassword(admin, AdminPassword);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var skillNames = new[]
        {
            "React", "JavaScript", "HTML", "CSS", "C#",
            "ASP.NET Core", "EF Core", "SQL Server", "Git"
        };
        var existingSkills = await dbContext.Skills
            .Where(x => skillNames.Contains(x.Name))
            .ToDictionaryAsync(x => x.Name, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var skillName in skillNames)
        {
            if (!existingSkills.ContainsKey(skillName))
            {
                var skill = new Skill { Name = skillName };
                dbContext.Skills.Add(skill);
                existingSkills[skillName] = skill;
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var jobs = new[]
        {
            new SeedJob("Frontend Developer", "Colombo", "Full-time", JobStatus.Published,
                "Build modern responsive web interfaces using React", now.AddDays(-2), now.AddDays(30),
                ["React", "JavaScript", "HTML", "CSS"]),
            new SeedJob("Backend Developer", "Colombo", "Full-time", JobStatus.Published,
                "Build secure APIs using ASP.NET Core and SQL Server", now.AddDays(-1), now.AddDays(25),
                ["C#", "ASP.NET Core", "EF Core", "SQL Server"]),
            new SeedJob("Junior Software Engineer", "Kandy", "Internship", JobStatus.Published,
                "Assist with full-stack application development", now, now.AddDays(20),
                ["C#", "JavaScript", "Git"]),
            new SeedJob("Draft Platform Engineer", "Colombo", "Full-time", JobStatus.Draft,
                "Internal draft role", null, now.AddDays(45), ["C#", "Git"]),
            new SeedJob("Expired Web Developer", "Colombo", "Contract", JobStatus.Published,
                "Expired test role", now.AddDays(-40), now.AddDays(-1), ["JavaScript", "HTML", "CSS"])
        };

        foreach (var seedJob in jobs)
        {
            var job = await dbContext.JobPostings.SingleOrDefaultAsync(
                x => x.OrganizationId == organization.Id && x.Title == seedJob.Title,
                cancellationToken);

            if (job is null)
            {
                job = new JobPosting
                {
                    OrganizationId = organization.Id,
                    DepartmentId = department.Id,
                    RecruiterProfileId = recruiterProfile.Id,
                    Title = seedJob.Title,
                    Location = seedJob.Location,
                    EmploymentType = seedJob.EmploymentType,
                    Status = seedJob.Status,
                    Description = seedJob.Description,
                    PublishedAt = seedJob.PublishedAt,
                    ClosingAt = seedJob.ClosingAt
                };
                dbContext.JobPostings.Add(job);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var linkedSkillIds = await dbContext.JobSkills
                .Where(x => x.JobPostingId == job.Id)
                .Select(x => x.SkillId)
                .ToListAsync(cancellationToken);

            foreach (var skillName in seedJob.Skills)
            {
                var skill = existingSkills[skillName];
                if (!linkedSkillIds.Contains(skill.Id))
                {
                    dbContext.JobSkills.Add(new JobSkill
                    {
                        JobPostingId = job.Id,
                        SkillId = skill.Id,
                        IsRequired = true
                    });
                }
            }
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await DevelopmentDemoDataSeeder.SeedAsync(
            dbContext,
            passwordHasher,
            cancellationToken);
    }

    public async Task SeedPermissionsAsync(CancellationToken cancellationToken = default)
    {
        const string removedAdminReportsPermission = "admin.reports.view";
        var obsoletePermission = await dbContext.Permissions
            .SingleOrDefaultAsync(x => x.Code == removedAdminReportsPermission, cancellationToken);
        if (obsoletePermission is not null)
        {
            var obsoleteMappings = await dbContext.RolePermissions
                .Where(x => x.PermissionId == obsoletePermission.Id)
                .ToListAsync(cancellationToken);
            dbContext.RolePermissions.RemoveRange(obsoleteMappings);
            dbContext.Permissions.Remove(obsoletePermission);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var existing = await dbContext.Permissions.ToDictionaryAsync(x => x.Code, cancellationToken);
        foreach (var definition in PermissionConstants.All)
        {
            if (!existing.TryGetValue(definition.Code, out var permission))
            {
                permission = new Permission { Code=definition.Code, DisplayName=definition.DisplayName, Description=definition.Description, Category=definition.Category, IsSystemPermission=true };
                dbContext.Permissions.Add(permission);
                existing[definition.Code] = permission;
            }
            else
            {
                permission.DisplayName=definition.DisplayName; permission.Description=definition.Description; permission.Category=definition.Category; permission.IsSystemPermission=true;
            }
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        var mappings=await dbContext.RolePermissions.ToListAsync(cancellationToken);
        foreach(var pair in PermissionConstants.Defaults)
            if(!mappings.Any(x=>x.Role==pair.Key))
                foreach(var code in pair.Value)
                    dbContext.RolePermissions.Add(new RolePermission{Role=pair.Key,PermissionId=existing[code].Id});
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedJob(
        string Title,
        string Location,
        string EmploymentType,
        JobStatus Status,
        string Description,
        DateTime? PublishedAt,
        DateTime? ClosingAt,
        string[] Skills);
}
