using JobOrbit.Application.DTOs.RecruiterSettings;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace JobOrbit.Application.Services;

public sealed class RecruiterSettingsService(
    IRecruiterSettingsRepository repository,
    IPasswordHasher<User> hasher,
    IAuditService audit) : IRecruiterSettingsService
{
    public async Task<RecruiterSettingsDto?> GetAsync(int userId, CancellationToken token = default)
    {
        var profile = await RecruiterAsync(userId, token);
        return profile is null ? null : Map(profile);
    }

    public async Task<RecruiterSettingsDto?> UpdateAsync(int userId, UpdateRecruiterSettingsRequest request, CancellationToken token = default)
    {
        var profile = await RecruiterAsync(userId, token);
        if (profile is null) return null;
        profile.User.FirstName = request.FirstName.Trim();
        profile.User.LastName = request.LastName.Trim();
        profile.PhoneNumber = Clean(request.Phone);
        profile.JobApplicationNotifications = request.JobApplicationNotifications;
        profile.InterviewNotifications = request.InterviewNotifications;
        profile.CandidateStatusNotifications = request.CandidateStatusNotifications;
        profile.EmailNotifications = request.EmailNotifications;
        await repository.SaveAsync(token);
        return Map(profile);
    }

    public async Task<RecruiterPasswordOutcome> ChangePasswordAsync(int userId, ChangeRecruiterPasswordRequest request, CancellationToken token = default)
    {
        var profile = await RecruiterAsync(userId, token);
        if (profile is null) return RecruiterPasswordOutcome.NotFound;
        if (hasher.VerifyHashedPassword(profile.User, profile.User.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
            return RecruiterPasswordOutcome.IncorrectCurrentPassword;
        profile.User.PasswordHash = hasher.HashPassword(profile.User, request.NewPassword);
        await audit.WriteAsync(new JobOrbit.Application.Auditing.AuditEvent(userId,"PasswordChanged",nameof(User),userId,profile.User.Email,"Recruiter changed their password."),token);
        return RecruiterPasswordOutcome.Changed;
    }

    private async Task<RecruiterProfile?> RecruiterAsync(int userId, CancellationToken token)
    {
        var profile = await repository.GetAsync(userId, token);
        return profile?.User.Role == UserRole.Recruiter ? profile : null;
    }

    private static RecruiterSettingsDto Map(RecruiterProfile profile) => new()
    {
        Email = profile.User.Email,
        FirstName = profile.User.FirstName,
        LastName = profile.User.LastName,
        Phone = profile.PhoneNumber,
        JobApplicationNotifications = profile.JobApplicationNotifications,
        InterviewNotifications = profile.InterviewNotifications,
        CandidateStatusNotifications = profile.CandidateStatusNotifications,
        EmailNotifications = profile.EmailNotifications
    };

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
