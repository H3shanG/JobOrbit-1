namespace JobOrbit.Application.DTOs.Candidates;

public sealed class CandidateProfileDto
{
    public int CandidateId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? ProfessionalTitle { get; set; }
    public string? ProfessionalSummary { get; set; }
    public string? Education { get; set; }
    public string? Experience { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public int ProfileCompletionPercentage { get; set; }
}
