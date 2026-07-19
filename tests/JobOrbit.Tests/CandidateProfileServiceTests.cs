using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Tests;

public sealed class CandidateProfileServiceTests
{
    [Fact]
    public async Task GetAsync_CalculatesCompletionFromNineProfileAreas()
    {
        var user = new User { Id = 4, FirstName = "Test", LastName = "Candidate", Email = "test@example.com", CandidateProfile = new CandidateProfile { Id = 8, PhoneNumber = "0771234567", Location = "Colombo", Headline = "Developer", Summary = "Summary", Education = "Degree", Experience = "Projects", LinkedInUrl = "https://linkedin.com/in/test" } };
        var service = new CandidateProfileService(new FakeRepository(user));
        var result = await service.GetAsync(4);
        Assert.Equal(100, result!.ProfileCompletionPercentage);
    }

    private sealed class FakeRepository(User user) : ICandidateProfileRepository
    {
        public Task<User?> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<User?>(user);
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
