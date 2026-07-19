namespace JobOrbit.Application.DTOs.Auth;

public sealed class AuthResponse
{
    public required string Token { get; init; }

    public required string TokenType { get; init; }

    public DateTime ExpiresAtUtc { get; init; }

    public required CurrentUserResponse User { get; init; }
}
