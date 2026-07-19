using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.DTOs.Auth;

public sealed class CurrentUserResponse
{
    public int UserId { get; init; }

    public required string Email { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string FullName { get; init; }

    public UserRole Role { get; init; }
}
