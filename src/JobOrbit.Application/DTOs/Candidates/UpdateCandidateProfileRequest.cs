using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.Candidates;

public sealed class UpdateCandidateProfileRequest
{
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [MaxLength(30), RegularExpression(@"^[0-9+()\-\s]*$")] public string? Phone { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(200)] public string? ProfessionalTitle { get; set; }
    [MaxLength(2000)] public string? ProfessionalSummary { get; set; }
    [MaxLength(4000)] public string? Education { get; set; }
    [MaxLength(4000)] public string? Experience { get; set; }
    [MaxLength(1000), Url] public string? LinkedInUrl { get; set; }
    [MaxLength(1000), Url] public string? PortfolioUrl { get; set; }
}
