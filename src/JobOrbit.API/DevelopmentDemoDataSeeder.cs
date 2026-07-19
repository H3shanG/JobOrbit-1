using JobOrbit.Domain;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using JobOrbit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.API;

internal static class DevelopmentDemoDataSeeder
{
    private const string DemoPassword = "Demo@123";

    public static async Task SeedAsync(
        JobOrbitDbContext db,
        IPasswordHasher<User> passwordHasher,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var organizations = await SeedOrganizationsAsync(db, cancellationToken);
        var departments = await SeedDepartmentsAsync(db, organizations, cancellationToken);
        var skills = await SeedSkillsAsync(db, cancellationToken);
        var recruiters = await SeedRecruitersAsync(db, passwordHasher, organizations, cancellationToken);
        var managers = await SeedManagersAsync(db, passwordHasher, organizations, departments, cancellationToken);
        var candidates = await SeedCandidatesAsync(db, passwordHasher, skills, cancellationToken);
        var jobs = await SeedJobsAsync(db, organizations, departments, recruiters, skills, now, cancellationToken);
        var applications = await SeedApplicationsAsync(db, candidates, jobs, now, cancellationToken);
        var interviews = await SeedInterviewsAsync(db, applications, now, cancellationToken);
        await SeedEvaluationsAsync(db, applications, interviews, managers, cancellationToken);
        await SeedDecisionsAsync(db, applications, managers, now, cancellationToken);
        await SeedAuditLogsAsync(db, recruiters, managers, candidates, applications, now, cancellationToken);
        await SeedNotificationsAsync(db, recruiters, managers, candidates, applications, interviews, now, cancellationToken);
    }

    private static async Task<Dictionary<string, Organization>> SeedOrganizationsAsync(
        JobOrbitDbContext db, CancellationToken ct)
    {
        var seeds = new[]
        {
            new OrganizationSeed("NOVATECH", "NovaTech Solutions", "Technology and digital solutions company", "Colombo"),
            new OrganizationSeed("LANKADIGITAL", "Lanka Digital Systems", "Enterprise software and cloud transformation partner", "Colombo"),
            new OrganizationSeed("GREENWAVE", "GreenWave Technologies", "Sustainable technology products for global teams", "Kandy")
        };
        var result = new Dictionary<string, Organization>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var entity = await db.Organizations.SingleOrDefaultAsync(x => x.Code == seed.Code, ct)
                ?? await db.Organizations.SingleOrDefaultAsync(x => x.Name == seed.Name, ct);
            if (entity is null)
            {
                entity = new Organization
                {
                    Code = seed.Code, Name = seed.Name, Description = seed.Description,
                    Location = seed.Location, City = seed.Location, Country = "Sri Lanka", IsActive = true
                };
                db.Organizations.Add(entity);
                await db.SaveChangesAsync(ct);
            }
            result[seed.Code] = entity;
        }
        return result;
    }

    private static async Task<Dictionary<string, Department>> SeedDepartmentsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, Organization> organizations, CancellationToken ct)
    {
        var seeds = new[]
        {
            new DepartmentSeed("NOVATECH", "ENG", "Engineering"), new DepartmentSeed("NOVATECH", "PROD", "Product"),
            new DepartmentSeed("NOVATECH", "QA", "Quality Assurance"), new DepartmentSeed("LANKADIGITAL", "ENG", "Engineering"),
            new DepartmentSeed("LANKADIGITAL", "HR", "Human Resources"), new DepartmentSeed("LANKADIGITAL", "FIN", "Finance"),
            new DepartmentSeed("GREENWAVE", "ENG", "Engineering"), new DepartmentSeed("GREENWAVE", "MKT", "Marketing"),
            new DepartmentSeed("GREENWAVE", "PROD", "Product")
        };
        var result = new Dictionary<string, Department>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var organization = organizations[seed.OrganizationCode];
            var entity = await db.Departments.SingleOrDefaultAsync(
                x => x.OrganizationId == organization.Id && (x.Code == seed.Code || x.Name == seed.Name), ct);
            if (entity is null)
            {
                entity = new Department
                {
                    OrganizationId = organization.Id, Code = seed.Code, Name = seed.Name,
                    Description = $"{seed.Name} team at {organization.Name}", IsActive = true
                };
                db.Departments.Add(entity);
                await db.SaveChangesAsync(ct);
            }
            result[$"{seed.OrganizationCode}:{seed.Code}"] = entity;
        }
        return result;
    }

    private static async Task<Dictionary<string, Skill>> SeedSkillsAsync(JobOrbitDbContext db, CancellationToken ct)
    {
        var names = new[]
        {
            "React", "JavaScript", "TypeScript", "HTML", "CSS", "C#", "ASP.NET Core", "EF Core",
            "SQL Server", "Git", "Azure", "Docker", "CI/CD", "Figma", "UI/UX", "Selenium",
            "Quality Assurance", "Business Analysis", "Agile", "Product Management", "Communication", "Human Resources"
        };
        var existing = await db.Skills.Where(x => names.Contains(x.Name)).ToListAsync(ct);
        var result = existing.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        foreach (var name in names.Where(name => !result.ContainsKey(name)))
        {
            var skill = new Skill { Name = name, Description = $"Professional experience with {name}" };
            db.Skills.Add(skill);
            result[name] = skill;
        }
        await db.SaveChangesAsync(ct);
        return result;
    }

    private static async Task<Dictionary<string, RecruiterProfile>> SeedRecruitersAsync(
        JobOrbitDbContext db, IPasswordHasher<User> hasher,
        IReadOnlyDictionary<string, Organization> organizations, CancellationToken ct)
    {
        var seeds = new[]
        {
            new PersonSeed("recruiter@joborbit.test", "Sarah", "Fernando", "NOVATECH", "Technical Recruiter"),
            new PersonSeed("recruiter.lanka@joborbit.test", "Nishan", "Perera", "LANKADIGITAL", "Senior Talent Partner"),
            new PersonSeed("recruiter.greenwave@joborbit.test", "Amaya", "Silva", "GREENWAVE", "Talent Acquisition Specialist")
        };
        var result = new Dictionary<string, RecruiterProfile>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var user = await EnsureUserAsync(db, hasher, seed.Email, seed.FirstName, seed.LastName, UserRole.Recruiter, DemoPassword, ct);
            var profile = await db.RecruiterProfiles.SingleOrDefaultAsync(x => x.UserId == user.Id, ct);
            if (profile is null)
            {
                profile = new RecruiterProfile
                {
                    UserId = user.Id, OrganizationId = organizations[seed.OrganizationCode].Id,
                    JobTitle = seed.Title, PhoneNumber = "+94 77 555 0101"
                };
                db.RecruiterProfiles.Add(profile);
                await db.SaveChangesAsync(ct);
            }
            result[seed.Email] = profile;
        }
        return result;
    }

    private static async Task<Dictionary<string, User>> SeedManagersAsync(
        JobOrbitDbContext db, IPasswordHasher<User> hasher,
        IReadOnlyDictionary<string, Organization> organizations,
        IReadOnlyDictionary<string, Department> departments, CancellationToken ct)
    {
        var seeds = new[]
        {
            new PersonSeed("manager@joborbit.test", "Sampath", "Perera", "NOVATECH", "Engineering Manager"),
            new PersonSeed("manager.lanka@joborbit.test", "Maya", "Wijesinghe", "LANKADIGITAL", "Head of Engineering")
        };
        var result = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var user = await EnsureUserAsync(db, hasher, seed.Email, seed.FirstName, seed.LastName, UserRole.HiringManager, DemoPassword, ct);
            if (!await db.HiringManagerProfiles.AnyAsync(x => x.UserId == user.Id, ct))
            {
                db.HiringManagerProfiles.Add(new HiringManagerProfile
                {
                    UserId = user.Id, OrganizationId = organizations[seed.OrganizationCode].Id,
                    DepartmentId = departments[$"{seed.OrganizationCode}:ENG"].Id,
                    JobTitle = seed.Title, PhoneNumber = "+94 71 555 0202"
                });
                await db.SaveChangesAsync(ct);
            }
            result[seed.Email] = user;
        }
        return result;
    }

    private static async Task<Dictionary<string, CandidateProfile>> SeedCandidatesAsync(
        JobOrbitDbContext db, IPasswordHasher<User> hasher,
        IReadOnlyDictionary<string, Skill> skills, CancellationToken ct)
    {
        var seeds = new[]
        {
            new CandidateSeed("demo.candidate@joborbit.test", "Heshan", "Gayantha", "Full-Stack Software Engineer", "Colombo", "BSc in Software Engineering, University of Moratuwa", "2 years building web applications", ["C#", "ASP.NET Core", "React", "SQL Server"]),
            new CandidateSeed("avishka@joborbit.test", "Avishka", "Silva", "Frontend Developer", "Gampaha", "BSc in Information Technology, SLIIT", "3 years developing accessible React interfaces", ["React", "TypeScript", "HTML", "CSS"]),
            new CandidateSeed("dinithi@joborbit.test", "Dinithi", "Wijesinghe", "Quality Assurance Engineer", "Colombo", "BSc in Computer Science, University of Colombo", "2 years in manual and automated testing", ["Quality Assurance", "Selenium", "Git", "Communication"]),
            new CandidateSeed("kavindu@joborbit.test", "Kavindu", "Perera", "Backend Developer", "Kandy", "BEng in Software Engineering, IIT", "4 years delivering cloud-ready APIs", ["C#", "ASP.NET Core", "Azure", "Docker"]),
            new CandidateSeed("shiumi@joborbit.test", "Shiumi", "Jayasekara", "UI/UX Designer", "Negombo", "BA in Design, University of the Visual and Performing Arts", "3 years designing mobile and web products", ["Figma", "UI/UX", "Communication"]),
            new CandidateSeed("amal@joborbit.test", "Amal", "Gunawardena", "Business Analyst", "Kurunegala", "MBA and BSc in Management Information Systems", "5 years translating business needs into digital products", ["Business Analysis", "Agile", "Product Management", "Communication"])
        };
        var result = new Dictionary<string, CandidateProfile>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var user = await EnsureUserAsync(db, hasher, seed.Email, seed.FirstName, seed.LastName, UserRole.Candidate, DemoPassword, ct);
            var profile = await db.CandidateProfiles.SingleOrDefaultAsync(x => x.UserId == user.Id, ct);
            if (profile is null)
            {
                profile = new CandidateProfile
                {
                    UserId = user.Id, Headline = seed.Headline, Location = seed.Location,
                    Summary = $"{seed.Headline} focused on dependable, user-centred delivery.",
                    Education = seed.Education, Experience = seed.Experience,
                    PhoneNumber = "+94 7" + (user.Id % 10) + " 555 " + user.Id.ToString("0000")
                };
                db.CandidateProfiles.Add(profile);
                await db.SaveChangesAsync(ct);
            }
            var linked = await db.CandidateSkills.Where(x => x.CandidateProfileId == profile.Id).Select(x => x.SkillId).ToListAsync(ct);
            foreach (var name in seed.Skills)
            {
                var skill = skills[name];
                if (!linked.Contains(skill.Id))
                    db.CandidateSkills.Add(new CandidateSkill { CandidateProfileId = profile.Id, SkillId = skill.Id, ProficiencyLevel = 3 + (skill.Id % 3), YearsOfExperience = 1 + (skill.Id % 4) });
            }
            await db.SaveChangesAsync(ct);
            result[seed.Email] = profile;
        }
        return result;
    }

    private static async Task<Dictionary<string, JobPosting>> SeedJobsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, Organization> organizations,
        IReadOnlyDictionary<string, Department> departments,
        IReadOnlyDictionary<string, RecruiterProfile> recruiters,
        IReadOnlyDictionary<string, Skill> skills, DateTime now, CancellationToken ct)
    {
        var seeds = new[]
        {
            new JobSeed("NOVATECH", "ENG", "recruiter@joborbit.test", "Junior C# Full-Stack Developer", "Colombo", "Full-time", "Hybrid", "Junior", JobStatus.Published, -18, 32, 120000, 190000, ["C#", "ASP.NET Core", "React", "SQL Server"]),
            new JobSeed("NOVATECH", "ENG", "recruiter@joborbit.test", "ASP.NET Core Backend Developer", "Colombo", "Full-time", "Hybrid", "Mid-level", JobStatus.Published, -14, 28, 180000, 280000, ["C#", "ASP.NET Core", "EF Core", "SQL Server"]),
            new JobSeed("NOVATECH", "PROD", "recruiter@joborbit.test", "React Frontend Developer", "Colombo", "Full-time", "Remote", "Mid-level", JobStatus.Published, -10, 35, 170000, 260000, ["React", "TypeScript", "HTML", "CSS"]),
            new JobSeed("NOVATECH", "QA", "recruiter@joborbit.test", "QA Engineer", "Colombo", "Full-time", "On-site", "Mid-level", JobStatus.Published, -9, 25, 140000, 220000, ["Quality Assurance", "Selenium", "Git"]),
            new JobSeed("LANKADIGITAL", "ENG", "recruiter.lanka@joborbit.test", "DevOps Engineer", "Colombo", "Full-time", "Hybrid", "Senior", JobStatus.Published, -12, 30, 250000, 400000, ["Azure", "Docker", "CI/CD"]),
            new JobSeed("LANKADIGITAL", "ENG", "recruiter.lanka@joborbit.test", "Software Engineering Intern", "Colombo", "Internship", "Hybrid", "Entry-level", JobStatus.Published, -7, 21, 40000, 60000, ["C#", "JavaScript", "Git"]),
            new JobSeed("LANKADIGITAL", "HR", "recruiter.lanka@joborbit.test", "HR Executive", "Colombo", "Full-time", "On-site", "Mid-level", JobStatus.Published, -6, 24, 110000, 170000, ["Human Resources", "Communication"]),
            new JobSeed("GREENWAVE", "PROD", "recruiter.greenwave@joborbit.test", "UI/UX Designer", "Kandy", "Full-time", "Hybrid", "Mid-level", JobStatus.Published, -11, 29, 140000, 230000, ["Figma", "UI/UX", "Communication"]),
            new JobSeed("GREENWAVE", "PROD", "recruiter.greenwave@joborbit.test", "Product Manager", "Remote", "Full-time", "Remote", "Senior", JobStatus.Published, -8, 26, 260000, 420000, ["Product Management", "Agile", "Business Analysis"]),
            new JobSeed("GREENWAVE", "MKT", "recruiter.greenwave@joborbit.test", "Business Analyst", "Kandy", "Contract", "Hybrid", "Mid-level", JobStatus.Paused, -25, 18, 160000, 240000, ["Business Analysis", "Agile", "Communication"]),
            new JobSeed("NOVATECH", "ENG", "recruiter@joborbit.test", "Cloud Platform Architect", "Colombo", "Full-time", "Hybrid", "Lead", JobStatus.Draft, 0, 45, 400000, 600000, ["Azure", "Docker", "CI/CD"]),
            new JobSeed("LANKADIGITAL", "FIN", "recruiter.lanka@joborbit.test", "Finance Systems Analyst", "Colombo", "Full-time", "On-site", "Mid-level", JobStatus.Closed, -70, -5, 150000, 230000, ["Business Analysis", "SQL Server"])
        };
        var result = new Dictionary<string, JobPosting>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var organization = organizations[seed.OrganizationCode];
            var entity = await db.JobPostings.SingleOrDefaultAsync(x => x.OrganizationId == organization.Id && x.Title == seed.Title, ct);
            if (entity is null)
            {
                entity = new JobPosting
                {
                    OrganizationId = organization.Id, DepartmentId = departments[$"{seed.OrganizationCode}:{seed.DepartmentCode}"].Id,
                    RecruiterProfileId = recruiters[seed.RecruiterEmail].Id, Title = seed.Title,
                    Description = $"Join {organization.Name} as a {seed.Title} and deliver practical, high-quality digital solutions.",
                    Responsibilities = "Collaborate with cross-functional teams, deliver measurable outcomes, and continuously improve quality.",
                    Requirements = string.Join(", ", seed.Skills), Location = seed.Location, EmploymentType = seed.EmploymentType,
                    WorkplaceType = seed.WorkplaceType, ExperienceLevel = seed.ExperienceLevel,
                    SalaryMinimum = seed.MinimumSalary, SalaryMaximum = seed.MaximumSalary,
                    Status = seed.Status, PublishedAt = seed.Status == JobStatus.Draft ? null : now.AddDays(seed.PublishedDays),
                    ClosingAt = now.AddDays(seed.ClosingDays), VacancyCount = seed.Title.Contains("Intern", StringComparison.Ordinal) ? 3 : 1,
                    IsFeatured = seed.Title is "Junior C# Full-Stack Developer" or "DevOps Engineer"
                };
                db.JobPostings.Add(entity);
                await db.SaveChangesAsync(ct);
            }
            var linked = await db.JobSkills.Where(x => x.JobPostingId == entity.Id).Select(x => x.SkillId).ToListAsync(ct);
            foreach (var name in seed.Skills)
                if (!linked.Contains(skills[name].Id))
                    db.JobSkills.Add(new JobSkill { JobPostingId = entity.Id, SkillId = skills[name].Id, IsRequired = true });
            await db.SaveChangesAsync(ct);
            result[seed.Title] = entity;
        }
        return result;
    }

    private static async Task<Dictionary<string, JobApplication>> SeedApplicationsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, CandidateProfile> candidates,
        IReadOnlyDictionary<string, JobPosting> jobs, DateTime now, CancellationToken ct)
    {
        var seeds = new[]
        {
            A("demo.candidate@joborbit.test", "Junior C# Full-Stack Developer", ApplicationStatus.Shortlisted, 17), A("demo.candidate@joborbit.test", "ASP.NET Core Backend Developer", ApplicationStatus.Interviewing, 12), A("demo.candidate@joborbit.test", "DevOps Engineer", ApplicationStatus.Submitted, 3),
            A("avishka@joborbit.test", "React Frontend Developer", ApplicationStatus.Interviewing, 14), A("avishka@joborbit.test", "Junior C# Full-Stack Developer", ApplicationStatus.UnderReview, 7), A("avishka@joborbit.test", "UI/UX Designer", ApplicationStatus.Rejected, 21),
            A("dinithi@joborbit.test", "QA Engineer", ApplicationStatus.Interviewing, 10), A("dinithi@joborbit.test", "Software Engineering Intern", ApplicationStatus.Shortlisted, 5), A("dinithi@joborbit.test", "ASP.NET Core Backend Developer", ApplicationStatus.Rejected, 16),
            A("kavindu@joborbit.test", "ASP.NET Core Backend Developer", ApplicationStatus.Hired, 24), A("kavindu@joborbit.test", "DevOps Engineer", ApplicationStatus.Interviewing, 9), A("kavindu@joborbit.test", "Junior C# Full-Stack Developer", ApplicationStatus.Rejected, 19),
            A("shiumi@joborbit.test", "UI/UX Designer", ApplicationStatus.Shortlisted, 8), A("shiumi@joborbit.test", "React Frontend Developer", ApplicationStatus.Submitted, 2), A("shiumi@joborbit.test", "Product Manager", ApplicationStatus.UnderReview, 6),
            A("amal@joborbit.test", "Product Manager", ApplicationStatus.Interviewing, 15), A("amal@joborbit.test", "HR Executive", ApplicationStatus.Rejected, 20), A("amal@joborbit.test", "Software Engineering Intern", ApplicationStatus.Submitted, 4), A("amal@joborbit.test", "Junior C# Full-Stack Developer", ApplicationStatus.UnderReview, 1), A("demo.candidate@joborbit.test", "Software Engineering Intern", ApplicationStatus.Hired, 27)
        };
        var result = new Dictionary<string, JobApplication>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var candidate = candidates[seed.CandidateEmail];
            var job = jobs[seed.JobTitle];
            var entity = await db.JobApplications.SingleOrDefaultAsync(x => x.CandidateProfileId == candidate.Id && x.JobPostingId == job.Id, ct);
            if (entity is null)
            {
                entity = new JobApplication
                {
                    CandidateProfileId = candidate.Id, JobPostingId = job.Id, Status = seed.Status,
                    AppliedAt = now.AddDays(-seed.DaysAgo), CoverLetter = $"I am excited to apply for the {job.Title} role and contribute my relevant experience."
                };
                db.JobApplications.Add(entity);
                await db.SaveChangesAsync(ct);
            }
            result[$"{seed.CandidateEmail}|{seed.JobTitle}"] = entity;
        }
        return result;
    }

    private static async Task<Dictionary<string, Interview>> SeedInterviewsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, JobApplication> applications, DateTime now, CancellationToken ct)
    {
        var seeds = new[]
        {
            I("demo.candidate@joborbit.test", "ASP.NET Core Backend Developer", InterviewStatus.Scheduled, 3, "demo-interview-backend"),
            I("avishka@joborbit.test", "React Frontend Developer", InterviewStatus.Rescheduled, 7, "demo-interview-react"),
            I("dinithi@joborbit.test", "QA Engineer", InterviewStatus.Completed, -6, "demo-interview-qa"),
            I("kavindu@joborbit.test", "DevOps Engineer", InterviewStatus.Cancelled, 5, "demo-interview-devops"),
            I("amal@joborbit.test", "Product Manager", InterviewStatus.Scheduled, 11, "demo-interview-product")
        };
        var result = new Dictionary<string, Interview>(StringComparer.OrdinalIgnoreCase);
        foreach (var seed in seeds)
        {
            var application = applications[$"{seed.CandidateEmail}|{seed.JobTitle}"];
            var entity = await db.Interviews.SingleOrDefaultAsync(x => x.JobApplicationId == application.Id && x.Notes == seed.Key, ct);
            if (entity is null)
            {
                entity = new Interview
                {
                    JobApplicationId = application.Id, ScheduledAt = now.Date.AddDays(seed.DayOffset).AddHours(4),
                    DurationMinutes = 60, Location = seed.Status == InterviewStatus.Completed ? "NovaTech Colombo Office" : "Online",
                    MeetingUrl = seed.Status is InterviewStatus.Scheduled or InterviewStatus.Rescheduled ? "https://meet.example.test/joborbit-demo" : null,
                    Status = seed.Status, Notes = seed.Key
                };
                db.Interviews.Add(entity);
                await db.SaveChangesAsync(ct);
            }
            result[seed.Key] = entity;
        }
        return result;
    }

    private static async Task SeedEvaluationsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, JobApplication> applications,
        IReadOnlyDictionary<string, Interview> interviews, IReadOnlyDictionary<string, User> managers, CancellationToken ct)
    {
        var seeds = new[]
        {
            new EvaluationSeed("dinithi@joborbit.test", "QA Engineer", "manager@joborbit.test", "demo-interview-qa", 8.6m, 9, 8, 8, 9, EvaluationRecommendation.Proceed, HiringDecision.StrongHire),
            new EvaluationSeed("demo.candidate@joborbit.test", "ASP.NET Core Backend Developer", "manager@joborbit.test", "demo-interview-backend", 7.2m, 8, 7, 7, 7, EvaluationRecommendation.Proceed, HiringDecision.Hire),
            new EvaluationSeed("avishka@joborbit.test", "React Frontend Developer", "manager@joborbit.test", "demo-interview-react", 6.5m, 7, 7, 6, 6, EvaluationRecommendation.Hold, HiringDecision.Pending),
            new EvaluationSeed("kavindu@joborbit.test", "DevOps Engineer", "manager.lanka@joborbit.test", "demo-interview-devops", 4.4m, 5, 4, 5, 4, EvaluationRecommendation.Reject, HiringDecision.NoHire),
            new EvaluationSeed("amal@joborbit.test", "Product Manager", "manager.lanka@joborbit.test", "demo-interview-product", 8.1m, 8, 9, 8, 8, EvaluationRecommendation.Proceed, HiringDecision.Hire)
        };
        foreach (var seed in seeds)
        {
            var application = applications[$"{seed.CandidateEmail}|{seed.JobTitle}"];
            var evaluator = managers[seed.ManagerEmail];
            if (await db.CandidateEvaluations.AnyAsync(x => x.JobApplicationId == application.Id && x.EvaluatorUserId == evaluator.Id, ct)) continue;
            db.CandidateEvaluations.Add(new CandidateEvaluation
            {
                JobApplicationId = application.Id, EvaluatorUserId = evaluator.Id, InterviewId = interviews[seed.InterviewKey].Id,
                OverallScore = seed.Overall, TechnicalScore = seed.Technical, CommunicationScore = seed.Communication,
                ExperienceScore = seed.Experience, CultureFitScore = seed.Culture, Recommendation = seed.Recommendation,
                HiringDecision = seed.HiringDecision, Comments = "Structured demo evaluation based on interview evidence."
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedDecisionsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, JobApplication> applications,
        IReadOnlyDictionary<string, User> managers, DateTime now, CancellationToken ct)
    {
        var seeds = new[]
        {
            new DecisionSeed("kavindu@joborbit.test", "ASP.NET Core Backend Developer", "manager@joborbit.test", ManagerHiringDecision.Hire, 4),
            new DecisionSeed("demo.candidate@joborbit.test", "Software Engineering Intern", "manager.lanka@joborbit.test", ManagerHiringDecision.Hire, 8),
            new DecisionSeed("kavindu@joborbit.test", "Junior C# Full-Stack Developer", "manager@joborbit.test", ManagerHiringDecision.Reject, 12),
            new DecisionSeed("avishka@joborbit.test", "React Frontend Developer", "manager@joborbit.test", ManagerHiringDecision.Hold, 2)
        };
        foreach (var seed in seeds)
        {
            var application = applications[$"{seed.CandidateEmail}|{seed.JobTitle}"];
            if (await db.ApplicationHiringDecisions.AnyAsync(x => x.JobApplicationId == application.Id, ct)) continue;
            db.ApplicationHiringDecisions.Add(new ApplicationHiringDecision
            {
                JobApplicationId = application.Id, DecidedByUserId = managers[seed.ManagerEmail].Id,
                Decision = seed.Decision, DecidedAt = now.AddDays(-seed.DaysAgo),
                Notes = "Demo decision recorded after structured review."
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedAuditLogsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, RecruiterProfile> recruiters,
        IReadOnlyDictionary<string, User> managers, IReadOnlyDictionary<string, CandidateProfile> candidates,
        IReadOnlyDictionary<string, JobApplication> applications, DateTime now, CancellationToken ct)
    {
        var actors = new[]
        {
            managers["manager@joborbit.test"].Id, managers["manager.lanka@joborbit.test"].Id,
            candidates["demo.candidate@joborbit.test"].UserId, candidates["avishka@joborbit.test"].UserId,
            recruiters["recruiter@joborbit.test"].UserId, recruiters["recruiter.lanka@joborbit.test"].UserId
        };
        for (var index = 1; index <= 20; index++)
        {
            var key = $"demo-audit-{index:00}";
            if (await db.AuditLogs.AnyAsync(x => x.CorrelationId == key, ct)) continue;
            var application = applications.Values.ElementAt((index - 1) % applications.Count);
            db.AuditLogs.Add(new AuditLog
            {
                UserId = actors[(index - 1) % actors.Length], EntityName = index % 3 == 0 ? "JobPosting" : "JobApplication",
                EntityId = index % 3 == 0 ? application.JobPostingId : application.Id,
                EntityDisplayName = index % 3 == 0 ? "Demo job posting" : $"Application #{application.Id}",
                Action = (index % 4) switch { 0 => "ApplicationStatusChanged", 1 => "UserLoginSucceeded", 2 => "JobViewed", _ => "CandidateReviewed" },
                Description = (index % 4) switch { 0 => "Application status updated successfully.", 1 => "User signed in successfully.", 2 => "Published job viewed.", _ => "Candidate profile reviewed." },
                Severity = index == 19 ? AuditSeverity.Warning : AuditSeverity.Information,
                IsSuccess = true, CorrelationId = key, IpAddress = "127.0.0.1",
                Metadata = $"{{\"seedEvent\":\"{key}\",\"occurredAt\":\"{now.AddDays(-index):O}\"}}"
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedNotificationsAsync(
        JobOrbitDbContext db, IReadOnlyDictionary<string, RecruiterProfile> recruiters,
        IReadOnlyDictionary<string, User> managers, IReadOnlyDictionary<string, CandidateProfile> candidates,
        IReadOnlyDictionary<string, JobApplication> applications, IReadOnlyDictionary<string, Interview> interviews,
        DateTime now, CancellationToken ct)
    {
        var app = applications["demo.candidate@joborbit.test|ASP.NET Core Backend Developer"];
        var interview = interviews["demo-interview-backend"];
        var seeds = new[]
        {
            new NotificationSeed(candidates["demo.candidate@joborbit.test"].UserId, NotificationTypes.InterviewScheduled, "Interview scheduled", "Your backend developer interview is scheduled.", $"/candidate/applications/{app.Id}", app.Id, false),
            new NotificationSeed(candidates["avishka@joborbit.test"].UserId, NotificationTypes.ApplicationStatusChanged, "Application under review", "Your application is being reviewed.", "/candidate/applications", app.Id, true),
            new NotificationSeed(recruiters["recruiter@joborbit.test"].UserId, NotificationTypes.NewApplicationReceived, "New candidate application", "A candidate applied to one of your jobs.", "/recruiter/applicants", app.Id, false),
            new NotificationSeed(recruiters["recruiter.lanka@joborbit.test"].UserId, NotificationTypes.InterviewReminder, "Interview reminder", "An upcoming candidate interview needs your attention.", "/recruiter/interviews", interview.Id, true),
            new NotificationSeed(managers["manager@joborbit.test"].Id, NotificationTypes.EvaluationRequired, "Evaluation required", "Please complete a candidate interview evaluation.", "/manager/interviews", interview.Id, false),
            new NotificationSeed(managers["manager.lanka@joborbit.test"].Id, NotificationTypes.HiringDecisionRequired, "Hiring decision required", "A reviewed candidate is ready for a decision.", "/manager/hiring-decisions", app.Id, false)
        };
        for (var index = 0; index < seeds.Length; index++)
        {
            var seed = seeds[index];
            var eventKey = $"demo-notification-{index + 1:00}";
            if (await db.Notifications.AnyAsync(x => x.EventKey == eventKey, ct)) continue;
            db.Notifications.Add(new Notification
            {
                RecipientUserId = seed.UserId, Type = seed.Type, Title = seed.Title, Message = seed.Message,
                ActionUrl = seed.ActionUrl, RelatedEntityType = "JobApplication", RelatedEntityId = seed.EntityId,
                IsRead = seed.IsRead, ReadAtUtc = seed.IsRead ? now.AddDays(-1) : null,
                Priority = NotificationPriority.Normal, EventKey = eventKey
            });
        }
        await db.SaveChangesAsync(ct);
    }

    private static async Task<User> EnsureUserAsync(
        JobOrbitDbContext db, IPasswordHasher<User> hasher, string email, string firstName,
        string lastName, UserRole role, string password, CancellationToken ct)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is not null) return user;
        user = new User { Email = email, FirstName = firstName, LastName = lastName, Role = role, IsActive = true };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }

    private static ApplicationSeed A(string email, string title, ApplicationStatus status, int daysAgo) => new(email, title, status, daysAgo);
    private static InterviewSeed I(string email, string title, InterviewStatus status, int offset, string key) => new(email, title, status, offset, key);

    private sealed record OrganizationSeed(string Code, string Name, string Description, string Location);
    private sealed record DepartmentSeed(string OrganizationCode, string Code, string Name);
    private sealed record PersonSeed(string Email, string FirstName, string LastName, string OrganizationCode, string Title);
    private sealed record CandidateSeed(string Email, string FirstName, string LastName, string Headline, string Location, string Education, string Experience, string[] Skills);
    private sealed record JobSeed(string OrganizationCode, string DepartmentCode, string RecruiterEmail, string Title, string Location, string EmploymentType, string WorkplaceType, string ExperienceLevel, JobStatus Status, int PublishedDays, int ClosingDays, decimal MinimumSalary, decimal MaximumSalary, string[] Skills);
    private sealed record ApplicationSeed(string CandidateEmail, string JobTitle, ApplicationStatus Status, int DaysAgo);
    private sealed record InterviewSeed(string CandidateEmail, string JobTitle, InterviewStatus Status, int DayOffset, string Key);
    private sealed record EvaluationSeed(string CandidateEmail, string JobTitle, string ManagerEmail, string InterviewKey, decimal Overall, int Technical, int Communication, int Experience, int Culture, EvaluationRecommendation Recommendation, HiringDecision HiringDecision);
    private sealed record DecisionSeed(string CandidateEmail, string JobTitle, string ManagerEmail, ManagerHiringDecision Decision, int DaysAgo);
    private sealed record NotificationSeed(int UserId, string Type, string Title, string Message, string ActionUrl, int EntityId, bool IsRead);
}
