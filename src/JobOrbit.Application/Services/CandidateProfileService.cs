using JobOrbit.Application.DTOs.Candidates;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Services;

public sealed class CandidateProfileService(ICandidateProfileRepository repository)
    : ICandidateProfileService
{
    public async Task<CandidateProfileDto?> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetOrCreateAsync(userId, cancellationToken);
        return user is null ? null : Map(user);
    }

    public async Task<CandidateProfileDto?> UpdateAsync(int userId, UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await repository.GetOrCreateAsync(userId, cancellationToken);
        if (user is null) return null;
        var profile = user.CandidateProfile!;
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        profile.PhoneNumber = Clean(request.Phone);
        profile.Location = Clean(request.Address);
        profile.Headline = Clean(request.ProfessionalTitle);
        profile.Summary = Clean(request.ProfessionalSummary);
        profile.Education = Clean(request.Education);
        profile.Experience = Clean(request.Experience);
        profile.LinkedInUrl = Clean(request.LinkedInUrl);
        profile.PortfolioUrl = Clean(request.PortfolioUrl);
        await repository.SaveChangesAsync(cancellationToken);
        return Map(user);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    internal static CandidateProfileDto Map(User user)
    {
        var p = user.CandidateProfile!;
        var completed = new[]
        {
            user.FirstName, user.LastName, p.PhoneNumber, p.Location, p.Headline,
            p.Summary, p.Education, p.Experience,
            !string.IsNullOrWhiteSpace(p.LinkedInUrl) || !string.IsNullOrWhiteSpace(p.PortfolioUrl) ? "yes" : null
        }.Count(x => !string.IsNullOrWhiteSpace(x));
        return new CandidateProfileDto
        {
            CandidateId = p.Id, UserId = user.Id, FirstName = user.FirstName,
            LastName = user.LastName, Email = user.Email, Phone = p.PhoneNumber,
            Address = p.Location, ProfessionalTitle = p.Headline,
            ProfessionalSummary = p.Summary, Education = p.Education,
            Experience = p.Experience, LinkedInUrl = p.LinkedInUrl,
            PortfolioUrl = p.PortfolioUrl,
            ProfileCompletionPercentage = (int)Math.Round(completed / 9d * 100)
        };
    }
}
