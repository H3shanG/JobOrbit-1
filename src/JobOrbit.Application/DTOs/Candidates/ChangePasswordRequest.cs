using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.Candidates;

public sealed class ChangePasswordRequest
{
    [Required, StringLength(128)] public string CurrentPassword { get; init; } = string.Empty;
    [Required, StringLength(128, MinimumLength = 10)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, and a number.")]
    public string NewPassword { get; init; } = string.Empty;
    [Required, Compare(nameof(NewPassword), ErrorMessage = "Password confirmation does not match.")]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}
