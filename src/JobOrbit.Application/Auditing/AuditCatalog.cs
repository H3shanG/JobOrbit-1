using JobOrbit.Domain.Enums;
namespace JobOrbit.Application.Auditing;
public static class AuditCatalog
{
 public static readonly IReadOnlyDictionary<string,(string Label,string Category)> Actions=new Dictionary<string,(string,string)>(StringComparer.OrdinalIgnoreCase)
 {
  ["UserLoginSucceeded"]=("User Login Succeeded","Authentication"),["UserLoginFailed"]=("User Login Failed","Authentication"),["PasswordChanged"]=("Password Changed","Authentication"),["AdminResetPassword"]=("Password Reset by Admin","User Management"),
  ["AdminCreateUser"]=("User Created","User Management"),["AdminUpdateUser"]=("User Updated","User Management"),["AdminActivateUser"]=("User Activated","User Management"),["AdminDeactivateUser"]=("User Deactivated","User Management"),
  ["AdminUpdateRolePermissions"]=("Role Permissions Updated","Permissions"),["AdminResetRolePermissions"]=("Role Permissions Reset","Permissions"),
  ["AdminCreateOrganization"]=("Organization Created","Organizations"),["AdminUpdateOrganization"]=("Organization Updated","Organizations"),["AdminActivateOrganization"]=("Organization Activated","Organizations"),["AdminDeactivateOrganization"]=("Organization Deactivated","Organizations"),
  ["AdminCreateDepartment"]=("Department Created","Departments"),["AdminUpdateDepartment"]=("Department Updated","Departments"),["AdminActivateDepartment"]=("Department Activated","Departments"),["AdminDeactivateDepartment"]=("Department Deactivated","Departments"),
  ["AdminUpdateJob"]=("Job Updated","Jobs"),["AdminChangeJobStatus"]=("Job Status Changed","Jobs"),["AdminFeatureJob"]=("Job Featured","Jobs"),["AdminUnfeatureJob"]=("Job Unfeatured","Jobs"),["Create"]=("Job Created","Jobs"),["Update"]=("Record Updated","General"),["Publish"]=("Job Published","Jobs"),["Close"]=("Job Closed","Jobs"),["Delete"]=("Record Deleted","General"),
  ["StatusUpdate"]=("Application Status Changed","Applications"),["AdminOverrideApplicationStatus"]=("Application Status Overridden","Applications"),
  ["ScheduleInterview"]=("Interview Scheduled","Interviews"),["UpdateInterview"]=("Interview Updated","Interviews"),["CancelInterview"]=("Interview Cancelled","Interviews"),["CompleteInterview"]=("Interview Completed","Interviews"),
  ["CreateEvaluation"]=("Evaluation Created","Evaluations"),["UpdateEvaluation"]=("Evaluation Updated","Evaluations"),["CreateHiringDecision"]=("Hiring Decision Created","Decisions"),["UpdateHiringDecision"]=("Hiring Decision Updated","Decisions"),["ResumeAccessed"]=("Resume Accessed","Security")
 };
 public static readonly IReadOnlyDictionary<string,string> EntityTypes=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){{"User","User"},{"RolePermission","Role Permission"},{"Organization","Organization"},{"Department","Department"},{"JobPosting","Job"},{"JobApplication","Application"},{"Interview","Interview"},{"CandidateEvaluation","Candidate Evaluation"},{"ApplicationHiringDecision","Hiring Decision"},{"Resume","Resume"}};
 public static AuditSeverity InferSeverity(string action)=>action.Contains("Deactivate",StringComparison.OrdinalIgnoreCase)||action.Contains("Override",StringComparison.OrdinalIgnoreCase)||action.Contains("Cancel",StringComparison.OrdinalIgnoreCase)||action.Contains("Delete",StringComparison.OrdinalIgnoreCase)?AuditSeverity.Warning:action.Contains("Failed",StringComparison.OrdinalIgnoreCase)||action.Contains("Unauthorized",StringComparison.OrdinalIgnoreCase)?AuditSeverity.Critical:AuditSeverity.Information;
}
public sealed record AuditEvent(int? ActorUserId,string Action,string EntityType,int? EntityId=null,string? EntityDisplayName=null,string? Description=null,AuditSeverity Severity=AuditSeverity.Information,bool IsSuccess=true,object? OldValues=null,object? NewValues=null,object? Metadata=null,string? CorrelationId=null);
