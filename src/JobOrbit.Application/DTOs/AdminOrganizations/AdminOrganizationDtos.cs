using System.ComponentModel.DataAnnotations;
using JobOrbit.Application.DTOs.Jobs;

namespace JobOrbit.Application.DTOs.AdminOrganizations;

public sealed class AdminOrganizationQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    [Range(1,int.MaxValue)] public int Page { get; set; }=1;
    [Range(1,100)] public int PageSize { get; set; }=10;
    public string Sort { get; set; }="name";
}
public sealed record AdminOrganizationListItemDto(int OrganizationId,string Name,string Code,string? Email,string? Phone,string? City,string? Country,string Status,int DepartmentCount,int RecruiterCount,int HiringManagerCount,int ActiveJobCount,DateTime CreatedAt);
public sealed record AdminOrganizationDetailsDto(int OrganizationId,string Name,string Code,string? Description,string? Email,string? Phone,string? WebsiteUrl,string? AddressLine1,string? AddressLine2,string? City,string? StateOrProvince,string? PostalCode,string? Country,string Status,int DepartmentCount,int RecruiterCount,int HiringManagerCount,int ActiveJobCount,int TotalApplicationCount,DateTime CreatedAt,DateTime UpdatedAt);
public sealed record AdminOrganizationLookupDto(int OrganizationId,string Name,string Code,bool IsActive);

public class SaveOrganizationRequest
{
    [Required,MaxLength(200)] public string Name { get; set; }=string.Empty;
    [Required,MaxLength(50),OrganizationCode] public string Code { get; set; }=string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [OptionalOrganizationEmail,MaxLength(320)] public string? Email { get; set; }
    [OptionalOrganizationPhone,MaxLength(30)] public string? Phone { get; set; }
    [OptionalOrganizationUrl,MaxLength(1000)] public string? WebsiteUrl { get; set; }
    [MaxLength(250)] public string? AddressLine1 { get; set; }
    [MaxLength(250)] public string? AddressLine2 { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? StateOrProvince { get; set; }
    [MaxLength(30)] public string? PostalCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    public bool IsActive { get; set; }=true;
}
public sealed class CreateOrganizationRequest:SaveOrganizationRequest;
public sealed class UpdateOrganizationRequest:SaveOrganizationRequest;
public sealed class UpdateOrganizationStatusRequest
{
    public bool IsActive { get; set; }
    [MaxLength(500)] public string? Reason { get; set; }
}
public enum AdminOrganizationOutcome { Success,NotFound,DuplicateCode,DuplicateName,InvalidCode }
public sealed record AdminOrganizationResult(AdminOrganizationOutcome Outcome,AdminOrganizationDetailsDto? Organization=null);
public sealed record AdminOrganizationListResult(bool Valid,PagedResultDto<AdminOrganizationListItemDto>? Result=null);

public sealed class OrganizationCodeAttribute:ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if(value is not string text)return false;
        text=text.Trim();
        return text.Length>0&&text.All(c=>char.IsLetterOrDigit(c)||c is '-' or '_');
    }
}
public sealed class OptionalOrganizationEmailAttribute:ValidationAttribute{public override bool IsValid(object?value)=>string.IsNullOrWhiteSpace(value as string)||new EmailAddressAttribute().IsValid(value);}
public sealed class OptionalOrganizationUrlAttribute:ValidationAttribute{public override bool IsValid(object?value)=>string.IsNullOrWhiteSpace(value as string)||new UrlAttribute().IsValid(value);}
public sealed class OptionalOrganizationPhoneAttribute:ValidationAttribute{public override bool IsValid(object?value)=>string.IsNullOrWhiteSpace(value as string)||new PhoneAttribute().IsValid(value);}
