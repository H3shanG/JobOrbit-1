using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Entities;

namespace JobOrbit.Tests;

public sealed class CandidateResumeServiceTests
{
    [Fact]
    public async Task UploadAsync_RejectsUnsupportedExtensionBeforeStorage()
    {
        var service = new CandidateResumeService(new FakeRepository(), new FakeStorage(), new ResumeFileValidator(), new FakeSettingsProvider());
        await Assert.ThrowsAsync<ArgumentException>(() => service.UploadAsync(1, new MemoryStream([1]), "resume.exe", "application/octet-stream", 1, null));
    }

    private sealed class FakeStorage : IFileStorageService
    {
        public Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default) => Task.FromResult("stored" + extension);
        public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default) => Task.FromResult<Stream?>(null);
        public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
    private sealed class FakeRepository : ICandidateResumeRepository
    {
        public Task<IReadOnlyList<Resume>> ListAsync(int userId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Resume>>([]);
        public Task<Resume?> GetAsync(int userId, int resumeId, CancellationToken cancellationToken = default) => Task.FromResult<Resume?>(null);
        public Task<Resume?> AddAsync(int userId, Resume resume, CancellationToken cancellationToken = default) => Task.FromResult<Resume?>(resume);
        public Task<bool> IsReferencedAsync(int resumeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task DeleteAsync(Resume resume, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> SetDefaultAsync(int userId, int resumeId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
    private sealed class FakeSettingsProvider : ISystemSettingsProvider
    {
        public Task<JobOrbit.Application.DTOs.AdminSystemSettings.SystemSettingsDto> GetAsync(CancellationToken token=default)=>Task.FromResult(JobOrbit.Application.DTOs.AdminSystemSettings.SystemSettingDefaults.All);
        public Task<object> UpdateSectionAsync(string section,object value,int actorUserId,CancellationToken token=default)=>Task.FromResult(value);
        public Task<object?> ResetSectionAsync(string section,int actorUserId,CancellationToken token=default)=>Task.FromResult<object?>(null);
        public Task SeedDefaultsAsync(CancellationToken token=default)=>Task.CompletedTask;
    }
}
