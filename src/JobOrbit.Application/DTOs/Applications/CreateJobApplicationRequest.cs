using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.Applications;

public sealed class CreateJobApplicationRequest
{
    [Required]
    [MinLength(20)]
    [MaxLength(8000)]
    public string CoverLetter { get; set; } = string.Empty;

    public int? ResumeId { get; set; }
}
