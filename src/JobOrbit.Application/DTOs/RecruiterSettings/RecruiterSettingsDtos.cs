using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.RecruiterSettings;

public sealed class RecruiterSettingsDto
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public bool JobApplicationNotifications { get; init; }
    public bool InterviewNotifications { get; init; }
    public bool CandidateStatusNotifications { get; init; }
    public bool EmailNotifications { get; init; }
}

public sealed class UpdateRecruiterSettingsRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string FirstName { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string LastName { get; init; } = string.Empty;

    [StringLength(30)]
    [RegularExpression(@"^[0-9+()\-\s]*$", ErrorMessage = "Phone contains unsupported characters.")]
    public string? Phone { get; init; }

    public bool JobApplicationNotifications { get; init; }
    public bool InterviewNotifications { get; init; }
    public bool CandidateStatusNotifications { get; init; }
    public bool EmailNotifications { get; init; }
}

public sealed class ChangeRecruiterPasswordRequest
{
    [Required, StringLength(128)]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, and a number.")]
    public string NewPassword { get; init; } = string.Empty;

    [Required, Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match.")]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public enum RecruiterPasswordOutcome { Changed, NotFound, IncorrectCurrentPassword }
