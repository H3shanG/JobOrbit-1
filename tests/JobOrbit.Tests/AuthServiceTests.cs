using JobOrbit.Application.Common.Exceptions;
using JobOrbit.Application.DTOs.Auth;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace JobOrbit.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesCandidateWithHashedPassword()
    {
        var repository = new FakeUserRepository();
        var passwordHasher = new PasswordHasher<User>();
        var service = new AuthService(repository, passwordHasher, new FakeJwtTokenService(), new FakeSettingsProvider());
        const string password = "StrongPassword123!";

        var response = await service.RegisterAsync(new RegisterRequest
        {
            Email = " Candidate@Example.com ",
            FirstName = "Ada",
            LastName = "Lovelace",
            Password = password
        });

        var createdUser = Assert.IsType<User>(repository.User);
        Assert.Equal("candidate@example.com", createdUser.Email);
        Assert.Equal(UserRole.Candidate, createdUser.Role);
        Assert.NotNull(createdUser.CandidateProfile);
        Assert.NotEqual(password, createdUser.PasswordHash);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordHasher.VerifyHashedPassword(createdUser, createdUser.PasswordHash, password));
        Assert.Equal(UserRole.Candidate, response.User.Role);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsDuplicateEmailException()
    {
        var repository = new FakeUserRepository { EmailExists = true };
        var service = new AuthService(
            repository,
            new PasswordHasher<User>(),
            new FakeJwtTokenService(), new FakeSettingsProvider());

        var request = new RegisterRequest
        {
            Email = "candidate@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Password = "StrongPassword123"
        };

        await Assert.ThrowsAsync<DuplicateEmailException>(
            () => service.RegisterAsync(request));
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public AuthResponse GenerateToken(User user)
        {
            return new AuthResponse
            {
                Token = "test-token",
                TokenType = "Bearer",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
                User = new CurrentUserResponse
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Role = user.Role
                }
            };
        }
    }

    private sealed class FakeSettingsProvider : ISystemSettingsProvider
    {
        public Task<JobOrbit.Application.DTOs.AdminSystemSettings.SystemSettingsDto> GetAsync(CancellationToken token=default)=>Task.FromResult(JobOrbit.Application.DTOs.AdminSystemSettings.SystemSettingDefaults.All);
        public Task<object> UpdateSectionAsync(string section,object value,int actorUserId,CancellationToken token=default)=>Task.FromResult(value);
        public Task<object?> ResetSectionAsync(string section,int actorUserId,CancellationToken token=default)=>Task.FromResult<object?>(null);
        public Task SeedDefaultsAsync(CancellationToken token=default)=>Task.CompletedTask;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailExists { get; init; }

        public User? User { get; private set; }

        public Task<bool> EmailExistsAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(EmailExists);

        public Task<User?> GetByEmailAsync(
            string normalizedEmail,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(User);

        public Task<User?> GetByIdAsync(
            int userId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(User);

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            User = user;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            User = user;
            return Task.CompletedTask;
        }
    }
}
