using System.IdentityModel.Tokens.Jwt;
using JobOrbit.Application.Configuration;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.Extensions.Options;

namespace JobOrbit.Tests;

public sealed class JwtTokenServiceTests
{
    [Fact]
    public void GenerateToken_ContainsRequiredIdentityClaims()
    {
        var settings = new JwtSettings
        {
            Issuer = "JobOrbit.Tests",
            Audience = "JobOrbit.Tests.Client",
            Key = "JobOrbit-tests-signing-key-with-at-least-32-characters",
            ExpiryMinutes = 30
        };
        var service = new JwtTokenService(Options.Create(settings));
        var user = new User
        {
            Id = 42,
            Email = "candidate@example.com",
            FirstName = "Ada",
            LastName = "Lovelace",
            Role = UserRole.Candidate
        };

        var response = service.GenerateToken(user);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);

        Assert.Equal("42", token.Claims.Single(claim => claim.Type == "UserId").Value);
        Assert.Equal(user.Email, token.Claims.Single(claim => claim.Type == "Email").Value);
        Assert.Equal("Ada Lovelace", token.Claims.Single(claim => claim.Type == "FullName").Value);
        Assert.Equal("Candidate", token.Claims.Single(claim => claim.Type == "Role").Value);
    }
}
