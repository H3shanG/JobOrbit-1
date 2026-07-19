using System.Text.Json;
using JobOrbit.Application.DTOs.HiringManagerEvaluations;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using JobOrbit.Domain;
using JobOrbit.Application.DTOs.Notifications;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class HiringManagerEvaluationRepository(JobOrbitDbContext db, INotificationService notifications) : IHiringManagerEvaluationRepository
{
    private async Task<(int OrganizationId, int? DepartmentId)?> ScopeAsync(int userId, CancellationToken token)
    {
        var x = await db.HiringManagerProfiles.AsNoTracking().Where(x => x.UserId == userId && x.User.IsActive).Select(x => new { x.OrganizationId, x.DepartmentId }).SingleOrDefaultAsync(token);
        return x is null ? null : (x.OrganizationId, x.DepartmentId);
    }
    private IQueryable<JobApplication> Scoped((int OrganizationId, int? DepartmentId) scope) => db.JobApplications.Where(x => x.JobPosting.OrganizationId == scope.OrganizationId && (!scope.DepartmentId.HasValue || x.JobPosting.DepartmentId == scope.DepartmentId));
    private static bool Reviewable(ApplicationStatus status) => status is ApplicationStatus.Shortlisted or ApplicationStatus.Interviewing;
    private static CandidateEvaluationDto Dto(CandidateEvaluation e, int userId) => new() { EvaluationId = e.Id, ApplicationId = e.JobApplicationId, TechnicalScore = e.TechnicalScore ?? 0, CommunicationScore = e.CommunicationScore ?? 0, ExperienceScore = e.ExperienceScore ?? 0, CultureFitScore = e.CultureFitScore ?? 0, OverallScore = e.OverallScore, OverallComments = e.Comments, Recommendation = e.Recommendation?.ToString() ?? "Hold", EvaluatorName = e.EvaluatorUser == null ? "Legacy evaluator" : e.EvaluatorUser.FirstName + " " + e.EvaluatorUser.LastName, CanEdit = e.EvaluatorUserId == userId, CreatedAt = e.CreatedAt, UpdatedAt = e.UpdatedAt };

    public async Task<EvaluationMutationResult> CreateAsync(int userId, int applicationId, CandidateEvaluationRequest request, EvaluationRecommendation recommendation, decimal overallScore, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return new(EvaluationMutationOutcome.NotFound);
        var application = await Scoped(scope.Value).SingleOrDefaultAsync(x => x.Id == applicationId, token); if (application is null) return new(EvaluationMutationOutcome.NotFound);
        if (!Reviewable(application.Status)) return new(EvaluationMutationOutcome.InvalidState);
        if (await db.CandidateEvaluations.AnyAsync(x => x.JobApplicationId == applicationId && x.EvaluatorUserId == userId, token)) return new(EvaluationMutationOutcome.Duplicate);
        var evaluation = new CandidateEvaluation { JobApplicationId = applicationId, EvaluatorUserId = userId, TechnicalScore = request.TechnicalScore, CommunicationScore = request.CommunicationScore, ExperienceScore = request.ExperienceScore, CultureFitScore = request.CultureFitScore, OverallScore = overallScore, Comments = request.OverallComments?.Trim(), Recommendation = recommendation, HiringDecision = HiringDecision.Pending };
        db.CandidateEvaluations.Add(evaluation); db.AuditLogs.Add(new AuditLog { UserId = userId, EntityName = nameof(CandidateEvaluation), EntityId = applicationId, Action = "CreateEvaluation", NewValues = JsonSerializer.Serialize(new { overallScore, Recommendation = recommendation.ToString() }) }); await db.SaveChangesAsync(token);
        var recruiter=await db.JobApplications.AsNoTracking().Where(x=>x.Id==applicationId).Select(x=>new{x.JobPosting.RecruiterProfile.UserId,x.JobPosting.Title}).SingleAsync(token);await notifications.CreateAsync(new(recruiter.UserId,NotificationTypes.InterviewStatusChanged,"Candidate evaluation completed",$"An evaluation for {recruiter.Title} has been completed.",nameof(CandidateEvaluation),evaluation.Id,$"/recruiter/applicants/{applicationId}",EventKey:$"evaluation:{evaluation.Id}:completed"),token);await db.Entry(evaluation).Reference(x => x.EvaluatorUser).LoadAsync(token); return new(EvaluationMutationOutcome.Success, Dto(evaluation, userId));
    }
    public async Task<IReadOnlyList<CandidateEvaluationDto>?> ListAsync(int userId, int applicationId, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null || !await Scoped(scope.Value).AsNoTracking().AnyAsync(x => x.Id == applicationId, token)) return null;
        var rows = await db.CandidateEvaluations.AsNoTracking().Include(x => x.EvaluatorUser).Where(x => x.JobApplicationId == applicationId && x.EvaluatorUserId != null).OrderByDescending(x => x.CreatedAt).ToListAsync(token); return rows.Select(x => Dto(x, userId)).ToList();
    }
    public async Task<EvaluationMutationResult> UpdateAsync(int userId, int evaluationId, CandidateEvaluationRequest request, EvaluationRecommendation recommendation, decimal overallScore, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return new(EvaluationMutationOutcome.NotFound);
        var evaluation = await db.CandidateEvaluations.Include(x => x.EvaluatorUser).SingleOrDefaultAsync(x => x.Id == evaluationId && x.EvaluatorUserId == userId && x.JobApplication.JobPosting.OrganizationId == scope.Value.OrganizationId && (!scope.Value.DepartmentId.HasValue || x.JobApplication.JobPosting.DepartmentId == scope.Value.DepartmentId), token); if (evaluation is null) return new(EvaluationMutationOutcome.NotFound);
        evaluation.TechnicalScore = request.TechnicalScore; evaluation.CommunicationScore = request.CommunicationScore; evaluation.ExperienceScore = request.ExperienceScore; evaluation.CultureFitScore = request.CultureFitScore; evaluation.OverallScore = overallScore; evaluation.Comments = request.OverallComments?.Trim(); evaluation.Recommendation = recommendation;
        db.AuditLogs.Add(new AuditLog { UserId = userId, EntityName = nameof(CandidateEvaluation), EntityId = evaluation.Id, Action = "UpdateEvaluation", NewValues = JsonSerializer.Serialize(new { overallScore, Recommendation = recommendation.ToString() }) }); await db.SaveChangesAsync(token); return new(EvaluationMutationOutcome.Success, Dto(evaluation, userId));
    }
    public async Task<HiringManagerEvaluationSummaryDto> SummaryAsync(int userId, CancellationToken token = default)
    {
        var scope = await ScopeAsync(userId, token); if (scope is null) return new();
        var reviewable = Scoped(scope.Value).AsNoTracking().Where(x => x.Status == ApplicationStatus.Shortlisted || x.Status == ApplicationStatus.Interviewing);
        var evaluations = db.CandidateEvaluations.AsNoTracking().Where(x => x.EvaluatorUserId == userId && x.JobApplication.JobPosting.OrganizationId == scope.Value.OrganizationId && (!scope.Value.DepartmentId.HasValue || x.JobApplication.JobPosting.DepartmentId == scope.Value.DepartmentId));
        var completed = await evaluations.CountAsync(token); var average = completed == 0 ? 0 : await evaluations.AverageAsync(x => x.OverallScore, token); var pending = await reviewable.CountAsync(x => !x.CandidateEvaluations.Any(e => e.EvaluatorUserId == userId), token);
        var counts = await evaluations.Where(x => x.Recommendation != null).GroupBy(x => x.Recommendation).Select(g => new EvaluationRecommendationCountDto { Recommendation = g.Key!.Value.ToString(), Count = g.Count() }).ToListAsync(token);
        return new() { AverageOverallScore = Math.Round(average, 2), CompletedEvaluations = completed, PendingEvaluations = pending, RecommendationCounts = counts };
    }
}
