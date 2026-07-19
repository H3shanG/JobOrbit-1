using JobOrbit.Application.DTOs.AdminSystemSettings;

namespace JobOrbit.Application.Interfaces;

public sealed record SystemSettingsMutationResult(bool Success, object? Data=null, string? Error=null, bool Conflict=false);
public interface IAdminSystemSettingsService
{
 Task<SystemSettingsDto> GetAsync(CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateGeneralAsync(int actor,UpdateGeneralSettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateRecruitmentAsync(int actor,UpdateRecruitmentSettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateUploadsAsync(int actor,UpdateUploadSettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateSecurityAsync(int actor,UpdateSecuritySettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateNotificationsAsync(int actor,UpdateNotificationSettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> UpdateMaintenanceAsync(int actor,UpdateMaintenanceSettingsRequest request,CancellationToken token=default);
 Task<SystemSettingsMutationResult> ResetAsync(int actor,string section,CancellationToken token=default);
}
