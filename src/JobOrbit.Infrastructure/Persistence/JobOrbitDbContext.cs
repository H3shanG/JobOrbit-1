using JobOrbit.Domain.Common;
using JobOrbit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JobOrbit.Infrastructure.Persistence;

public sealed class JobOrbitDbContext(DbContextOptions<JobOrbitDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();

    public DbSet<RecruiterProfile> RecruiterProfiles => Set<RecruiterProfile>();
    public DbSet<HiringManagerProfile> HiringManagerProfiles => Set<HiringManagerProfile>();

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<JobPosting> JobPostings => Set<JobPosting>();

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    public DbSet<Interview> Interviews => Set<Interview>();

    public DbSet<CandidateEvaluation> CandidateEvaluations => Set<CandidateEvaluation>();
    public DbSet<ApplicationHiringDecision> ApplicationHiringDecisions => Set<ApplicationHiringDecision>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();

    public DbSet<JobSkill> JobSkills => Set<JobSkill>();

    public DbSet<Resume> Resumes => Set<Resume>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public override int SaveChanges()
    {
        SetAuditTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return SaveChangesWithAuditAsync(cancellationToken);
    }

    private async Task<int> SaveChangesWithAuditAsync(CancellationToken cancellationToken)
    {
        var logs=ChangeTracker.Entries<AuditLog>().Where(x=>x.State==EntityState.Added).Select(x=>x.Entity).ToList();
        var actorIds=logs.Where(l=>l.UserId.HasValue).Select(l=>l.UserId!.Value).Distinct().ToList();
        var users=await Users.AsNoTracking().Where(x=>actorIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,cancellationToken);
        foreach(var log in logs)
        {
            if(log.UserId.HasValue&&users.TryGetValue(log.UserId.Value,out var actor)){log.ActorNameSnapshot??=(actor.FirstName+" "+actor.LastName).Trim();log.ActorRoleSnapshot??=actor.Role==Domain.Enums.UserRole.Administrator?"Admin":actor.Role.ToString();}
            log.Description??=log.Action+" on "+log.EntityName;
            log.OldValues=SanitizeJson(log.OldValues);log.NewValues=SanitizeJson(log.NewValues);log.Metadata=SanitizeJson(log.Metadata);
        }
        SetAuditTimestamps();return await base.SaveChangesAsync(cancellationToken);
    }

    private static string? SanitizeJson(string? value)
    {
        if(string.IsNullOrWhiteSpace(value))return value;
        try{var node=JsonNode.Parse(value);Mask(node);return node?.ToJsonString(new JsonSerializerOptions{WriteIndented=false});}catch(JsonException){return value.Length>4000?value[..4000]:value;}
    }
    private static readonly string[] SensitiveKeys=["password","passwordhash","token","accesstoken","refreshtoken","resettoken","connectionstring","storedfilename","physicalpath","secret"];
    private static void Mask(JsonNode? node){if(node is JsonObject obj)foreach(var key in obj.Select(x=>x.Key).ToList()){if(SensitiveKeys.Any(x=>key.Contains(x,StringComparison.OrdinalIgnoreCase)))obj[key]="[REDACTED]";else Mask(obj[key]);}else if(node is JsonArray arr)foreach(var child in arr)Mask(child);}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobOrbitDbContext).Assembly);
    }

    private void SetAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
                entry.Entity.UpdatedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(entity => entity.CreatedAt).IsModified = false;
                entry.Entity.UpdatedAt = utcNow;
            }
        }
    }
}
