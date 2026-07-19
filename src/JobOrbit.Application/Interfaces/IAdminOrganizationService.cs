using JobOrbit.Application.DTOs.AdminOrganizations;

namespace JobOrbit.Application.Interfaces;

public interface IAdminOrganizationService
{
    Task<AdminOrganizationListResult> ListAsync(AdminOrganizationQuery query,CancellationToken token=default);
    Task<AdminOrganizationDetailsDto?> DetailsAsync(int id,CancellationToken token=default);
    Task<IReadOnlyList<AdminOrganizationLookupDto>> LookupAsync(bool includeInactive,CancellationToken token=default);
    Task<AdminOrganizationResult> CreateAsync(int adminId,CreateOrganizationRequest request,CancellationToken token=default);
    Task<AdminOrganizationResult> UpdateAsync(int adminId,int id,UpdateOrganizationRequest request,CancellationToken token=default);
    Task<AdminOrganizationResult> StatusAsync(int adminId,int id,UpdateOrganizationStatusRequest request,CancellationToken token=default);
}
