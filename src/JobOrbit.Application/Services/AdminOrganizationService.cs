using JobOrbit.Application.DTOs.AdminOrganizations;
using JobOrbit.Application.Interfaces;

namespace JobOrbit.Application.Services;

public sealed class AdminOrganizationService(IAdminOrganizationRepository repository):IAdminOrganizationService
{
    public Task<AdminOrganizationListResult> ListAsync(AdminOrganizationQuery q,CancellationToken t=default)=>repository.ListAsync(q,t);
    public Task<AdminOrganizationDetailsDto?> DetailsAsync(int id,CancellationToken t=default)=>repository.DetailsAsync(id,t);
    public Task<IReadOnlyList<AdminOrganizationLookupDto>> LookupAsync(bool inactive,CancellationToken t=default)=>repository.LookupAsync(inactive,t);
    public Task<AdminOrganizationResult> CreateAsync(int admin,CreateOrganizationRequest r,CancellationToken t=default)=>repository.CreateAsync(admin,r,t);
    public Task<AdminOrganizationResult> UpdateAsync(int admin,int id,UpdateOrganizationRequest r,CancellationToken t=default)=>repository.UpdateAsync(admin,id,r,t);
    public Task<AdminOrganizationResult> StatusAsync(int admin,int id,UpdateOrganizationStatusRequest r,CancellationToken t=default)=>repository.StatusAsync(admin,id,r,t);
}
