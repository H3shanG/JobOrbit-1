namespace JobOrbit.Application.Interfaces;
public interface IResumeFileValidator
{
    Task<bool> IsValidAsync(Stream content, string extension, CancellationToken cancellationToken = default);
}
