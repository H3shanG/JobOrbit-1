using System.Text.Json;
using JobOrbit.Application.DTOs.AdminOrganizations;
using JobOrbit.Application.DTOs.Jobs;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobOrbit.Infrastructure.Persistence.Repositories;

public sealed class AdminOrganizationRepository(JobOrbitDbContext db):IAdminOrganizationRepository
{
    private static string? Clean(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
    public async Task<AdminOrganizationListResult> ListAsync(AdminOrganizationQuery q,CancellationToken t=default)
    {
        bool? active=q.Status?.Trim().ToLowerInvariant() switch{"active"=>true,"inactive"=>false,"" or null=>null,_=>(bool?)null};
        if(!string.IsNullOrWhiteSpace(q.Status)&&active is null)return new(false);
        var rows=db.Organizations.AsNoTracking();
        if(active.HasValue)rows=rows.Where(x=>x.IsActive==active);
        if(!string.IsNullOrWhiteSpace(q.Search)){var s=q.Search.Trim();rows=rows.Where(x=>x.Name.Contains(s)||x.Code.Contains(s)||(x.Email!=null&&x.Email.Contains(s))||(x.Phone!=null&&x.Phone.Contains(s))||(x.City!=null&&x.City.Contains(s))||(x.Country!=null&&x.Country.Contains(s)));}
        rows=q.Sort.Trim().ToLowerInvariant() switch{"newest"=>rows.OrderByDescending(x=>x.CreatedAt),"oldest"=>rows.OrderBy(x=>x.CreatedAt),"code"=>rows.OrderBy(x=>x.Code),_=>rows.OrderBy(x=>x.Name)};
        var total=await rows.CountAsync(t);var now=DateTime.UtcNow;
        var items=await rows.Skip((q.Page-1)*q.PageSize).Take(q.PageSize).Select(x=>new AdminOrganizationListItemDto(x.Id,x.Name,x.Code,x.Email,x.Phone,x.City??x.Location,x.Country,x.IsActive?"Active":"Inactive",x.Departments.Count,x.Recruiters.Count,x.HiringManagers.Count,x.JobPostings.Count(j=>j.Status==JobStatus.Published&&(!j.ClosingAt.HasValue||j.ClosingAt>now)),x.CreatedAt)).ToListAsync(t);
        return new(true,new PagedResultDto<AdminOrganizationListItemDto>{Items=items,Page=q.Page,PageSize=q.PageSize,TotalItems=total,TotalPages=(int)Math.Ceiling(total/(double)q.PageSize)});
    }
    public Task<AdminOrganizationDetailsDto?> DetailsAsync(int id,CancellationToken t=default)
    {var now=DateTime.UtcNow;return db.Organizations.AsNoTracking().Where(x=>x.Id==id).Select(x=>new AdminOrganizationDetailsDto(x.Id,x.Name,x.Code,x.Description,x.Email,x.Phone,x.WebsiteUrl,x.AddressLine1,x.AddressLine2,x.City??x.Location,x.StateOrProvince,x.PostalCode,x.Country,x.IsActive?"Active":"Inactive",x.Departments.Count,x.Recruiters.Count,x.HiringManagers.Count,x.JobPostings.Count(j=>j.Status==JobStatus.Published&&(!j.ClosingAt.HasValue||j.ClosingAt>now)),x.JobPostings.SelectMany(j=>j.JobApplications).Count(),x.CreatedAt,x.UpdatedAt)).SingleOrDefaultAsync(t);}
    public async Task<IReadOnlyList<AdminOrganizationLookupDto>> LookupAsync(bool inactive,CancellationToken t=default)=>await db.Organizations.AsNoTracking().Where(x=>inactive||x.IsActive).OrderBy(x=>x.Name).Select(x=>new AdminOrganizationLookupDto(x.Id,x.Name,x.Code,x.IsActive)).ToListAsync(t);
    public async Task<AdminOrganizationResult>CreateAsync(int admin,CreateOrganizationRequest r,CancellationToken t=default)
    {var code=r.Code.Trim().ToUpperInvariant();var name=r.Name.Trim();if(await db.Organizations.AnyAsync(x=>x.Code.ToLower()==code.ToLower(),t))return new(AdminOrganizationOutcome.DuplicateCode);if(await db.Organizations.AnyAsync(x=>x.Name.ToLower()==name.ToLower(),t))return new(AdminOrganizationOutcome.DuplicateName);var x=new Organization{Name=name,Code=code};Apply(x,r);db.Organizations.Add(x);await db.SaveChangesAsync(t);db.AuditLogs.Add(new AuditLog{UserId=admin,EntityName=nameof(Organization),EntityId=x.Id,Action="AdminCreateOrganization",NewValues=JsonSerializer.Serialize(new{x.Name,x.Code,x.IsActive})});await db.SaveChangesAsync(t);return new(AdminOrganizationOutcome.Success,await DetailsAsync(x.Id,t));}
    public async Task<AdminOrganizationResult>UpdateAsync(int admin,int id,UpdateOrganizationRequest r,CancellationToken t=default)
    {var x=await db.Organizations.SingleOrDefaultAsync(x=>x.Id==id,t);if(x is null)return new(AdminOrganizationOutcome.NotFound);var code=r.Code.Trim().ToUpperInvariant();var name=r.Name.Trim();if(await db.Organizations.AnyAsync(o=>o.Id!=id&&o.Code.ToLower()==code.ToLower(),t))return new(AdminOrganizationOutcome.DuplicateCode);if(await db.Organizations.AnyAsync(o=>o.Id!=id&&o.Name.ToLower()==name.ToLower(),t))return new(AdminOrganizationOutcome.DuplicateName);var old=JsonSerializer.Serialize(new{x.Name,x.Code,x.IsActive});x.Name=name;x.Code=code;Apply(x,r);db.AuditLogs.Add(new AuditLog{UserId=admin,EntityName=nameof(Organization),EntityId=id,Action="AdminUpdateOrganization",OldValues=old,NewValues=JsonSerializer.Serialize(new{x.Name,x.Code,x.IsActive})});await db.SaveChangesAsync(t);return new(AdminOrganizationOutcome.Success,await DetailsAsync(id,t));}
    public async Task<AdminOrganizationResult>StatusAsync(int admin,int id,UpdateOrganizationStatusRequest r,CancellationToken t=default)
    {var x=await db.Organizations.SingleOrDefaultAsync(x=>x.Id==id,t);if(x is null)return new(AdminOrganizationOutcome.NotFound);x.IsActive=r.IsActive;x.DeactivatedAt=r.IsActive?null:DateTime.UtcNow;x.DeactivatedReason=r.IsActive?null:Clean(r.Reason);db.AuditLogs.Add(new AuditLog{UserId=admin,EntityName=nameof(Organization),EntityId=id,Action=r.IsActive?"AdminActivateOrganization":"AdminDeactivateOrganization",NewValues=JsonSerializer.Serialize(new{r.IsActive,Reason=Clean(r.Reason)})});await db.SaveChangesAsync(t);return new(AdminOrganizationOutcome.Success,await DetailsAsync(id,t));}
    private static void Apply(Organization x,SaveOrganizationRequest r){x.Description=Clean(r.Description);x.Email=Clean(r.Email)?.ToLowerInvariant();x.Phone=Clean(r.Phone);x.WebsiteUrl=Clean(r.WebsiteUrl);x.AddressLine1=Clean(r.AddressLine1);x.AddressLine2=Clean(r.AddressLine2);x.City=Clean(r.City);x.Location=x.City;x.StateOrProvince=Clean(r.StateOrProvince);x.PostalCode=Clean(r.PostalCode);x.Country=Clean(r.Country);x.IsActive=r.IsActive;if(r.IsActive){x.DeactivatedAt=null;x.DeactivatedReason=null;}else if(!x.DeactivatedAt.HasValue)x.DeactivatedAt=DateTime.UtcNow;}
}
