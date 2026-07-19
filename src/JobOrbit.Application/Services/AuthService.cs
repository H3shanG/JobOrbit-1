using JobOrbit.Application.Common.Exceptions;
using JobOrbit.Application.DTOs.Auth;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace JobOrbit.Application.Services;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenService jwtTokenService,
    ISystemSettingsProvider systemSettings) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await systemSettings.GetAsync(cancellationToken);
        if (!settings.Recruitment.AllowCandidateSelfRegistration)
            throw new InvalidOperationException("Candidate self-registration is currently disabled.");
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new DuplicateEmailException(normalizedEmail);
        }
        ValidatePassword(request.Password, settings.Security);

        var user = new User
        {
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.Candidate,
            IsActive = true,
            CandidateProfile = new CandidateProfile()
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        await userRepository.AddAsync(user, cancellationToken);
        return jwtTokenService.GenerateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(
            NormalizeEmail(request.Email),
            cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
            await userRepository.UpdateAsync(user, cancellationToken);
        }

        return jwtTokenService.GenerateToken(user);
    }

    public async Task<CurrentUserResponse?> GetCurrentUserAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null || !user.IsActive ? null : MapUser(user);
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static void ValidatePassword(string password, DTOs.AdminSystemSettings.SecuritySettingsDto settings)
    {
        if (password.Length < settings.MinimumPasswordLength ||
            settings.RequireUppercase && !password.Any(char.IsUpper) ||
            settings.RequireLowercase && !password.Any(char.IsLower) ||
            settings.RequireNumber && !password.Any(char.IsDigit) ||
            settings.RequireSpecialCharacter && password.All(char.IsLetterOrDigit))
            throw new ArgumentException("Password does not meet the current platform password policy.");
    }

    internal static CurrentUserResponse MapUser(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();

        return new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = fullName,
            Role = user.Role
        };
    }
}
