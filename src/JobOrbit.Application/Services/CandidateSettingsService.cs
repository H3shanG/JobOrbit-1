using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace JobOrbit.Application.Services;

public sealed class CandidateSettingsService(IUserRepository users, IPasswordHasher<User> hasher, IAuditService audit) : ICandidateSettingsService
{
    public async Task<CandidateSettingsDto?> GetAsync(int userId, CancellationToken token = default)
    {
        var user = await CandidateAsync(userId, token); return user is null ? null : Map(user);
    }

    public async Task<CandidateSettingsDto?> UpdateAsync(int userId, UpdateCandidateSettingsRequest request, CancellationToken token = default)
    {
        var user = await CandidateAsync(userId, token); if (user is null) return null;
        user.EmailNotifications = request.EmailNotifications;
        user.ApplicationStatusNotifications = request.ApplicationStatusNotifications;
        user.InterviewReminders = request.InterviewReminders;
        user.JobRecommendationNotifications = request.JobRecommendationNotifications;
        await users.UpdateAsync(user, token); return Map(user);
    }

    public async Task<ChangePasswordOutcome> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken token = default)
    {
        var user = await CandidateAsync(userId, token); if (user is null) return ChangePasswordOutcome.NotFound;
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
            return ChangePasswordOutcome.IncorrectCurrentPassword;
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);
        await users.UpdateAsync(user,token);await audit.WriteAsync(new JobOrbit.Application.Auditing.AuditEvent(userId,"PasswordChanged",nameof(User),userId,user.Email,"Candidate changed their password."),token); return ChangePasswordOutcome.Changed;
    }

    private async Task<User?> CandidateAsync(int id, CancellationToken token)
    {
        var user = await users.GetByIdAsync(id, token);
        return user is { IsActive: true, Role: UserRole.Candidate } ? user : null;
    }
    private static CandidateSettingsDto Map(User u) => new() { Email=u.Email, EmailNotifications=u.EmailNotifications, ApplicationStatusNotifications=u.ApplicationStatusNotifications, InterviewReminders=u.InterviewReminders, JobRecommendationNotifications=u.JobRecommendationNotifications };
}
