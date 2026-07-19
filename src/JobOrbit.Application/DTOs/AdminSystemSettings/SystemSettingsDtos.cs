using System.ComponentModel.DataAnnotations;

namespace JobOrbit.Application.DTOs.AdminSystemSettings;

public sealed record GeneralSettingsDto(string PlatformName, string SupportEmail, string DefaultTimeZone, string DefaultCurrency, string DateFormat);
public sealed record RecruitmentSettingsDto(bool AllowCandidateSelfRegistration, bool AllowMultipleApplicationsPerJob, bool RequireProfileCompletionBeforeApply, int MinimumProfileCompletionPercent, int DefaultJobClosingDays, bool RequireEvaluationBeforeHiringDecision, bool RequireInterviewBeforeHiringDecision);
public sealed record UploadSettingsDto(int MaximumResumeSizeMb, string[] AllowedResumeExtensions, int MaximumProfileImageSizeMb, string[] AllowedProfileImageExtensions);
public sealed record SecuritySettingsDto(int MinimumPasswordLength, bool RequireUppercase, bool RequireLowercase, bool RequireNumber, bool RequireSpecialCharacter, int MaximumFailedLoginAttempts, int AccountLockoutMinutes, int SessionTimeoutMinutes, bool LoginLockoutEnforced = false);
public sealed record NotificationSettingsDto(bool EnableNotifications, bool NotifyCandidateOnStatusChange, bool NotifyRecruiterOnNewApplication, bool NotifyManagerOnEvaluationRequired, bool NotifyParticipantsOnInterviewChange, bool ExternalEmailDeliveryConfigured = false);
public sealed record MaintenanceSettingsDto(bool MaintenanceModeEnabled, string MaintenanceMessage, bool AllowAdminLoginDuringMaintenance);
public sealed record SystemSettingsDto(GeneralSettingsDto General, RecruitmentSettingsDto Recruitment, UploadSettingsDto Uploads, SecuritySettingsDto Security, NotificationSettingsDto Notifications, MaintenanceSettingsDto Maintenance);

public sealed class UpdateGeneralSettingsRequest { [Required, StringLength(100)] public string PlatformName { get; set; }=""; [Required, EmailAddress, StringLength(320)] public string SupportEmail { get; set; }=""; [Required, StringLength(100)] public string DefaultTimeZone { get; set; }=""; [Required, StringLength(3,MinimumLength=3)] public string DefaultCurrency { get; set; }=""; [Required] public string DateFormat { get; set; }=""; }
public sealed class UpdateRecruitmentSettingsRequest { public bool AllowCandidateSelfRegistration { get; set; } public bool AllowMultipleApplicationsPerJob { get; set; } public bool RequireProfileCompletionBeforeApply { get; set; } public int MinimumProfileCompletionPercent { get; set; } public int DefaultJobClosingDays { get; set; } public bool RequireEvaluationBeforeHiringDecision { get; set; } public bool RequireInterviewBeforeHiringDecision { get; set; } }
public sealed class UpdateUploadSettingsRequest { public int MaximumResumeSizeMb { get; set; } public string[] AllowedResumeExtensions { get; set; }=[]; public int MaximumProfileImageSizeMb { get; set; } public string[] AllowedProfileImageExtensions { get; set; }=[]; }
public sealed class UpdateSecuritySettingsRequest { public int MinimumPasswordLength { get; set; } public bool RequireUppercase { get; set; } public bool RequireLowercase { get; set; } public bool RequireNumber { get; set; } public bool RequireSpecialCharacter { get; set; } public int MaximumFailedLoginAttempts { get; set; } public int AccountLockoutMinutes { get; set; } public int SessionTimeoutMinutes { get; set; } }
public sealed class UpdateNotificationSettingsRequest { public bool EnableNotifications { get; set; } public bool NotifyCandidateOnStatusChange { get; set; } public bool NotifyRecruiterOnNewApplication { get; set; } public bool NotifyManagerOnEvaluationRequired { get; set; } public bool NotifyParticipantsOnInterviewChange { get; set; } }
public sealed record PublicPlatformSettingsDto(string PlatformName,bool AllowCandidateSelfRegistration,int DefaultJobClosingDays,bool MaintenanceModeEnabled,string? MaintenanceMessage);
public sealed class UpdateMaintenanceSettingsRequest { public bool MaintenanceModeEnabled { get; set; } public string MaintenanceMessage { get; set; }=""; public bool AllowAdminLoginDuringMaintenance { get; set; }=true; }

public static class SystemSettingKeys { public const string General="system.general"; public const string Recruitment="system.recruitment"; public const string Uploads="system.uploads"; public const string Security="system.security"; public const string Notifications="system.notifications"; public const string Maintenance="system.maintenance"; public static readonly IReadOnlyDictionary<string,string> Sections=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){{"general",General},{"recruitment",Recruitment},{"uploads",Uploads},{"security",Security},{"notifications",Notifications},{"maintenance",Maintenance}}; }
public static class SystemSettingDefaults
{
 public static readonly GeneralSettingsDto General=new("JobOrbit","support@joborbit.test","Asia/Colombo","LKR","yyyy-MM-dd");
 public static readonly RecruitmentSettingsDto Recruitment=new(true,false,true,60,30,true,false);
 public static readonly UploadSettingsDto Uploads=new(5,[".pdf",".doc",".docx"],2,[".jpg",".jpeg",".png",".webp"]);
 public static readonly SecuritySettingsDto Security=new(8,true,true,true,true,5,15,60);
 public static readonly NotificationSettingsDto Notifications=new(true,true,true,true,true);
 public static readonly MaintenanceSettingsDto Maintenance=new(false,"JobOrbit is temporarily unavailable for maintenance.",true);
 public static SystemSettingsDto All=>new(General,Recruitment,Uploads,Security,Notifications,Maintenance);
}
