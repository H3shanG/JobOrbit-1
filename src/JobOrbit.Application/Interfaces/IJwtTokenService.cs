using JobOrbit.Application.DTOs.Auth;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Application.Interfaces;

public interface IJwtTokenService
{
    AuthResponse GenerateToken(User user);
}
