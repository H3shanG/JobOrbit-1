using JobOrbit.Application.Interfaces;

namespace JobOrbit.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _root = Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads", "resumes");
    public LocalFileStorageService() => Directory.CreateDirectory(_root);
    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default)
    {
        var name = $"{Guid.NewGuid():N}{extension}";
        await using var output = new FileStream(Path.Combine(_root, name), FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(output, cancellationToken);
        return name;
    }
    public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(storedFileName);
        var path = Path.Combine(_root, safeName);
        return Task.FromResult<Stream?>(File.Exists(path) ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true) : null);
    }
    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_root, Path.GetFileName(storedFileName));
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
