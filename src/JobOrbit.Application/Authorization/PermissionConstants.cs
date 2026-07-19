using JobOrbit.Domain.Enums;

namespace JobOrbit.Application.Authorization;

public static class PermissionConstants
{
    public const string CandidateProfileView = "candidate.profile.view";
    public const string CandidateProfileUpdate = "candidate.profile.update";
    public const string CandidateJobsView = "candidate.jobs.view";
    public const string CandidateApplicationsCreate = "candidate.applications.create";
    public const string CandidateApplicationsView = "candidate.applications.view";
    public const string CandidateResumeManage = "candidate.resume.manage";
    public const string RecruiterDashboardView = "recruiter.dashboard.view";
    public const string RecruiterJobsCreate = "recruiter.jobs.create";
    public const string RecruiterJobsView = "recruiter.jobs.view";
    public const string RecruiterJobsUpdate = "recruiter.jobs.update";
    public const string RecruiterJobsClose = "recruiter.jobs.close";
    public const string RecruiterApplicationsView = "recruiter.applications.view";
    public const string RecruiterApplicationsUpdate = "recruiter.applications.update";
    public const string RecruiterInterviewsManage = "recruiter.interviews.manage";
    public const string RecruiterAnalyticsView = "recruiter.analytics.view";
    public const string RecruiterSettingsManage = "recruiter.settings.manage";
    public const string ManagerDashboardView = "manager.dashboard.view";
    public const string ManagerCandidatesView = "manager.candidates.view";
    public const string ManagerEvaluationsCreate = "manager.evaluations.create";
    public const string ManagerEvaluationsUpdate = "manager.evaluations.update";
    public const string ManagerDecisionsCreate = "manager.decisions.create";
    public const string ManagerReportsView = "manager.reports.view";
    public const string ManagerSettingsManage = "manager.settings.manage";
    public const string AdminDashboardView = "admin.dashboard.view";
    public const string AdminUsersView = "admin.users.view";
    public const string AdminUsersCreate = "admin.users.create";
    public const string AdminUsersUpdate = "admin.users.update";
    public const string AdminUsersChangeStatus = "admin.users.change_status";
    public const string AdminUsersResetPassword = "admin.users.reset_password";
    public const string AdminRolesView = "admin.roles.view";
    public const string AdminRolesManage = "admin.roles.manage";
    public const string AdminOrganizationsManage = "admin.organizations.manage";
    public const string AdminDepartmentsManage = "admin.departments.manage";
    public const string AdminJobsManage = "admin.jobs.manage";
    public const string AdminApplicationsView = "admin.applications.view";
    public const string AdminApplicationsManage = "admin.applications.manage";
    public const string AdminAuditLogsView = "admin.audit_logs.view";
    public const string AdminSettingsManage = "admin.settings.manage";

    public static readonly IReadOnlySet<string> MandatoryAdmin = new HashSet<string>
    { AdminDashboardView, AdminUsersView, AdminRolesView, AdminRolesManage };

    public static IReadOnlyDictionary<UserRole, IReadOnlySet<string>> Defaults =>
        new Dictionary<UserRole, IReadOnlySet<string>>
        {
            [UserRole.Candidate] = Set(All.Where(x => x.Role == UserRole.Candidate)),
            [UserRole.Recruiter] = Set(All.Where(x => x.Role == UserRole.Recruiter)),
            [UserRole.HiringManager] = Set(All.Where(x => x.Role == UserRole.HiringManager)),
            [UserRole.Administrator] = Set(All.Where(x => x.Role == UserRole.Administrator))
        };

    public static readonly IReadOnlyList<PermissionDefinition> All =
    [
        D(CandidateProfileView,"View profile","View the candidate profile.","Candidate Profile",UserRole.Candidate,true), D(CandidateProfileUpdate,"Update profile","Update the candidate profile.","Candidate Profile",UserRole.Candidate), D(CandidateJobsView,"View jobs","Browse available jobs.","Candidate Jobs",UserRole.Candidate,true), D(CandidateApplicationsCreate,"Apply for jobs","Submit job applications.","Candidate Applications",UserRole.Candidate), D(CandidateApplicationsView,"View applications","View submitted applications.","Candidate Applications",UserRole.Candidate,true), D(CandidateResumeManage,"Manage resumes","Upload and manage resumes.","Candidate Resume",UserRole.Candidate),
        D(RecruiterDashboardView,"View dashboard","View recruiter dashboard.","Recruiter Dashboard",UserRole.Recruiter,true), D(RecruiterJobsCreate,"Create jobs","Create job postings.","Recruiter Jobs",UserRole.Recruiter), D(RecruiterJobsView,"View jobs","View owned job postings.","Recruiter Jobs",UserRole.Recruiter,true), D(RecruiterJobsUpdate,"Update jobs","Update owned job postings.","Recruiter Jobs",UserRole.Recruiter), D(RecruiterJobsClose,"Close jobs","Close owned job postings.","Recruiter Jobs",UserRole.Recruiter), D(RecruiterApplicationsView,"View applications","View applications to owned jobs.","Recruiter Applications",UserRole.Recruiter), D(RecruiterApplicationsUpdate,"Update applications","Update applicant statuses.","Recruiter Applications",UserRole.Recruiter), D(RecruiterInterviewsManage,"Manage interviews","Schedule and manage interviews.","Recruiter Interviews",UserRole.Recruiter), D(RecruiterAnalyticsView,"View analytics","View recruitment analytics.","Recruiter Analytics",UserRole.Recruiter), D(RecruiterSettingsManage,"Manage settings","Manage recruiter settings.","Recruiter Settings",UserRole.Recruiter),
        D(ManagerDashboardView,"View dashboard","View hiring manager dashboard.","Manager Dashboard",UserRole.HiringManager,true), D(ManagerCandidatesView,"View candidates","Review assigned candidates.","Manager Candidates",UserRole.HiringManager,true), D(ManagerEvaluationsCreate,"Create evaluations","Create candidate evaluations.","Manager Evaluations",UserRole.HiringManager), D(ManagerEvaluationsUpdate,"Update evaluations","Update candidate evaluations.","Manager Evaluations",UserRole.HiringManager), D(ManagerDecisionsCreate,"Make decisions","Record hiring decisions.","Manager Decisions",UserRole.HiringManager), D(ManagerReportsView,"View reports","View hiring reports.","Manager Reports",UserRole.HiringManager), D(ManagerSettingsManage,"Manage settings","Manage hiring manager settings.","Manager Settings",UserRole.HiringManager),
        D(AdminDashboardView,"View dashboard","View the admin dashboard.","Admin Dashboard",UserRole.Administrator,true), D(AdminUsersView,"View users","View user accounts and profile summaries.","User Management",UserRole.Administrator,true), D(AdminUsersCreate,"Create users","Create managed user accounts.","User Management",UserRole.Administrator), D(AdminUsersUpdate,"Update users","Update managed user accounts.","User Management",UserRole.Administrator), D(AdminUsersChangeStatus,"Change user status","Activate or deactivate user accounts.","User Management",UserRole.Administrator), D(AdminUsersResetPassword,"Reset passwords","Reset managed user passwords.","User Management",UserRole.Administrator), D(AdminRolesView,"View roles","View roles and permission assignments.","Roles and Permissions",UserRole.Administrator,true), D(AdminRolesManage,"Manage roles","Change and reset role permissions.","Roles and Permissions",UserRole.Administrator,true), D(AdminOrganizationsManage,"Manage organizations","Manage organizations.","Administration",UserRole.Administrator), D(AdminDepartmentsManage,"Manage departments","Manage departments.","Administration",UserRole.Administrator), D(AdminJobsManage,"Manage jobs","Administer job postings.","Administration",UserRole.Administrator), D(AdminApplicationsView,"View applications","View platform applications.","Administration",UserRole.Administrator), D(AdminApplicationsManage,"Manage applications","Perform controlled administrative application status corrections.","Administration",UserRole.Administrator), D(AdminAuditLogsView,"View audit logs","View platform audit records.","Administration",UserRole.Administrator), D(AdminSettingsManage,"Manage settings","Manage platform settings.","Administration",UserRole.Administrator)
    ];

    public static bool TryRole(string value, out UserRole role)
    {
        if (value.Equals("Admin", StringComparison.OrdinalIgnoreCase)) { role = UserRole.Administrator; return true; }
        return Enum.TryParse(value, true, out role) && Enum.IsDefined(role);
    }
    public static string PublicRole(UserRole role) => role == UserRole.Administrator ? "Admin" : role.ToString();
    private static HashSet<string> Set(IEnumerable<PermissionDefinition> values) => values.Select(x => x.Code).ToHashSet(StringComparer.Ordinal);
    private static PermissionDefinition D(string code,string name,string description,string category,UserRole role,bool required=false) => new(code,name,description,category,role,required);
}

public sealed record PermissionDefinition(string Code,string DisplayName,string Description,string Category,UserRole Role,bool IsRequired);
