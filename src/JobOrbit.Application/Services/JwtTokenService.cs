using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobOrbit.Application.Configuration;
using JobOrbit.Application.DTOs.Auth;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JobOrbit.Application.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions)
    : IJwtTokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public AuthResponse GenerateToken(User user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        var claims = new List<Claim>
        {
            new("UserId", user.Id.ToString()),
            new("Email", user.Email),
            new("FullName", fullName),
            new("Role", user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiresAtUtc = expiresAtUtc,
            User = AuthService.MapUser(user)
        };
    }
}
