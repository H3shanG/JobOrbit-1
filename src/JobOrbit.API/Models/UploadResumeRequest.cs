using System.ComponentModel.DataAnnotations;

namespace JobOrbit.API.Models;

public sealed class UploadResumeRequest
{
    [Required]
    public IFormFile File { get; init; } = null!;

    [StringLength(200)]
    public string? DisplayName { get; init; }
}
