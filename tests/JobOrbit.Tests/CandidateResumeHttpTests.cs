using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using JobOrbit.Application.Interfaces;
using JobOrbit.Domain.Entities;
using JobOrbit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobOrbit.Tests;

public sealed class CandidateResumeHttpTests
{
    [Fact]
    public async Task Admin_system_settings_persist_reset_authorize_and_audit()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();
        var admin=factory.Client(8,"Administrator");
        Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync("/api/admin/system-settings")).StatusCode);
        var update=await admin.PutAsJsonAsync("/api/admin/system-settings/general",new{platformName="JobOrbit Test",supportEmail="support@test.local",defaultTimeZone="Asia/Colombo",defaultCurrency="LKR",dateFormat="yyyy-MM-dd"});
        Assert.Equal(HttpStatusCode.OK,update.StatusCode);
        var persisted=await admin.GetFromJsonAsync<JsonElement>("/api/admin/system-settings");
        Assert.Equal("JobOrbit Test",persisted.GetProperty("general").GetProperty("platformName").GetString());
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/recruitment",new{allowCandidateSelfRegistration=true,allowMultipleApplicationsPerJob=false,requireProfileCompletionBeforeApply=true,minimumProfileCompletionPercent=50,defaultJobClosingDays=45,requireEvaluationBeforeHiringDecision=true,requireInterviewBeforeHiringDecision=false})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PutAsJsonAsync("/api/admin/system-settings/recruitment",new{allowCandidateSelfRegistration=true,allowMultipleApplicationsPerJob=true,requireProfileCompletionBeforeApply=false,minimumProfileCompletionPercent=0,defaultJobClosingDays=30,requireEvaluationBeforeHiringDecision=false,requireInterviewBeforeHiringDecision=false})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/uploads",new{maximumResumeSizeMb=6,allowedResumeExtensions=new[]{"PDF",".docx"},maximumProfileImageSizeMb=3,allowedProfileImageExtensions=new[]{"jpg",".png"}})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/security",new{minimumPasswordLength=10,requireUppercase=true,requireLowercase=true,requireNumber=true,requireSpecialCharacter=true,maximumFailedLoginAttempts=6,accountLockoutMinutes=20,sessionTimeoutMinutes=90})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/notifications",new{enableNotifications=false,notifyCandidateOnStatusChange=true,notifyRecruiterOnNewApplication=false,notifyManagerOnEvaluationRequired=true,notifyParticipantsOnInterviewChange=false})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/maintenance",new{maintenanceModeEnabled=true,maintenanceMessage="Planned maintenance",allowAdminLoginDuringMaintenance=true})).StatusCode);
        var publicSettings=await factory.CreateClient().GetFromJsonAsync<JsonElement>("/api/platform-settings/public");Assert.True(publicSettings.GetProperty("maintenanceModeEnabled").GetBoolean());Assert.Equal("Planned maintenance",publicSettings.GetProperty("maintenanceMessage").GetString());
        Assert.Equal(HttpStatusCode.ServiceUnavailable,(await factory.Client(1,"Candidate").GetAsync("/api/candidates/me/resumes")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync("/api/admin/system-settings")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/system-settings/maintenance",new{maintenanceModeEnabled=false,maintenanceMessage="JobOrbit is temporarily unavailable for maintenance.",allowAdminLoginDuringMaintenance=true})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PostAsync("/api/admin/system-settings/reset/general",null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PostAsync("/api/admin/system-settings/reset/unknown",null)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").GetAsync("/api/admin/system-settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/system-settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/system-settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/system-settings")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        Assert.True(await db.AuditLogs.AnyAsync(x=>x.Action=="GeneralSettingsUpdated"));
        Assert.True(await db.AuditLogs.AnyAsync(x=>x.Action=="SystemSettingsSectionReset"));
        Assert.True(await db.AuditLogs.AnyAsync(x=>x.Action=="MaintenanceSettingsUpdated"));
    }
    [Fact]
    public async Task Swagger_keeps_recruiter_routes_and_excludes_removed_admin_reports()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();
        var provider=factory.Services.GetRequiredService<Swashbuckle.AspNetCore.Swagger.ISwaggerProvider>();
        var document=provider.GetSwagger("v1");
        Assert.True(document.Paths.ContainsKey("/api/recruiter/interviews"));
        Assert.True(document.Paths.ContainsKey("/api/recruiter/interviews/{interviewId}"));
        Assert.DoesNotContain(document.Paths.Keys, path => path.StartsWith("/api/admin/reports", StringComparison.OrdinalIgnoreCase));
        await using var scope=factory.Services.CreateAsyncScope();
        var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        Assert.False(await db.Permissions.AnyAsync(x=>x.Code=="admin.reports.view"));
    }
    [Fact]
    public async Task Explainable_matching_is_scoped_bounded_and_does_not_change_status()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();int applicationId,jobId;
        await using(var scope=factory.Services.CreateAsyncScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var candidate=await db.CandidateProfiles.SingleAsync(x=>x.UserId==1);var app=await db.JobApplications.Include(x=>x.JobPosting).SingleAsync(x=>x.CandidateProfileId==candidate.Id&&x.JobPosting.RecruiterProfile.UserId==3);applicationId=app.Id;jobId=app.JobPostingId;var react=new Skill{Name="React"};var dotnet=new Skill{Name="ASP.NET Core"};db.Skills.AddRange(react,dotnet);await db.SaveChangesAsync();db.CandidateSkills.Add(new CandidateSkill{CandidateProfileId=candidate.Id,SkillId=react.Id,ProficiencyLevel=4,YearsOfExperience=2});db.JobSkills.AddRange(new JobSkill{JobPostingId=jobId,SkillId=react.Id,IsRequired=true,MinimumYearsOfExperience=1},new JobSkill{JobPostingId=jobId,SkillId=dotnet.Id,IsRequired=true,MinimumYearsOfExperience=1});candidate.Headline="Frontend React Developer";candidate.Location="Colombo";app.JobPosting.Location="Colombo";app.JobPosting.WorkplaceType="Hybrid";await db.SaveChangesAsync();
        }
        var candidateClient=factory.Client(1,"Candidate");var detail=await candidateClient.GetAsync($"/api/candidate/jobs/{jobId}/match");Assert.Equal(HttpStatusCode.OK,detail.StatusCode);var match=JsonDocument.Parse(await detail.Content.ReadAsStringAsync()).RootElement;var score=match.GetProperty("matchScore").GetInt32();var confidence=match.GetProperty("confidenceScore").GetInt32();Assert.InRange(score,0,100);Assert.InRange(confidence,0,100);var breakdown=match.GetProperty("scoreBreakdown");Assert.Equal(score,breakdown.EnumerateObject().Sum(x=>x.Value.GetInt32()));Assert.Contains(match.GetProperty("matchedSkills").EnumerateArray(),x=>x.GetString()=="React");Assert.Equal("1.0",match.GetProperty("algorithmVersion").GetString());
        Assert.Equal(HttpStatusCode.OK,(await factory.Client(3,"Recruiter").GetAsync($"/api/recruiter/jobs/{jobId}/ranked-applicants")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await factory.Client(3,"Recruiter").GetAsync($"/api/recruiter/applications/{applicationId}/match")).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await factory.Client(4,"Recruiter").GetAsync($"/api/recruiter/jobs/{jobId}/ranked-applicants")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await candidateClient.GetAsync($"/api/recruiter/jobs/{jobId}/ranked-applicants")).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/candidate/jobs/recommended")).StatusCode);
        await factory.SeedHiringManagerEvaluationScopeAsync();Assert.Equal(HttpStatusCode.OK,(await factory.Client(6,"HiringManager").GetAsync($"/api/manager/applications/{applicationId}/match")).StatusCode);
        await using var verify=factory.Services.CreateAsyncScope();var db2=verify.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.Equal(JobOrbit.Domain.Enums.ApplicationStatus.Shortlisted,(await db2.JobApplications.FindAsync(applicationId))!.Status);
    }
    [Theory]
    [InlineData("resume.pdf", "application/pdf", "pdf")]
    [InlineData("resume.doc", "application/msword", "doc")]
    [InlineData("resume.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "docx")]
    public async Task Valid_resume_upload_returns_201(string name, string mime, string format)
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var response = await Upload(factory.Client(1), name, mime, ValidFile(format), " Resume ");
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Contains("\"displayName\":\"Resume\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Multipart_upload_is_listed_and_json_upload_is_rejected()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var client = factory.Client(1);
        Assert.Equal(HttpStatusCode.Created, (await Upload(client, "listed.pdf", "application/pdf", ValidFile("pdf"), "Listed resume")).StatusCode);
        var list = JsonDocument.Parse(await (await client.GetAsync("/api/candidates/me/resumes")).Content.ReadAsStringAsync());
        Assert.Contains(list.RootElement.EnumerateArray(), item => item.GetProperty("displayName").GetString() == "Listed resume");
        var json = await client.PostAsJsonAsync("/api/candidates/me/resumes", new { file="not multipart", displayName="JSON" });
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, json.StatusCode);
    }

    [Fact]
    public async Task Oversized_file_is_rejected()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var bytes = new byte[5 * 1024 * 1024 + 1]; "%PDF-"u8.CopyTo(bytes);
        Assert.Equal(HttpStatusCode.BadRequest, (await Upload(factory.Client(1), "resume.pdf", "application/pdf", bytes)).StatusCode);
    }

    [Theory]
    [InlineData("resume.pdf", "application/pdf")]
    [InlineData("resume.pdf", "application/msword")]
    public async Task Invalid_signature_or_mime_mismatch_is_rejected(string name, string mime)
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await Upload(factory.Client(1), name, mime, "not-a-resume"u8.ToArray())).StatusCode);
    }

    [Fact]
    public async Task Display_name_too_long_is_rejected()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        Assert.Equal(HttpStatusCode.BadRequest, (await Upload(factory.Client(1), "resume.pdf", "application/pdf", ValidFile("pdf"), new string('x', 201))).StatusCode);
    }

    [Fact]
    public async Task Owner_can_download_with_safe_headers_but_another_candidate_cannot()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var id = await UploadedId(factory.Client(1), "mine.pdf", "application/pdf", ValidFile("pdf"));
        var own = await factory.Client(1).GetAsync($"/api/candidates/me/resumes/{id}");
        Assert.Equal(HttpStatusCode.OK, own.StatusCode);
        Assert.Equal("application/pdf", own.Content.Headers.ContentType?.MediaType);
        Assert.Contains("mine.pdf", own.Content.Headers.ContentDisposition?.ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await factory.Client(2).GetAsync($"/api/candidates/me/resumes/{id}")).StatusCode);
    }

    [Fact]
    public async Task Set_default_keeps_only_one_default()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var client = factory.Client(1);
        await UploadedId(client, "first.pdf", "application/pdf", ValidFile("pdf"));
        var second = await UploadedId(client, "second.pdf", "application/pdf", ValidFile("pdf"));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PatchAsync($"/api/candidates/me/resumes/{second}/default", null)).StatusCode);
        var json = JsonDocument.Parse(await (await client.GetAsync("/api/candidates/me/resumes")).Content.ReadAsStringAsync());
        Assert.Single(json.RootElement.EnumerateArray(), x => x.GetProperty("isDefault").GetBoolean());
    }

    [Fact]
    public async Task Deleting_default_reassigns_newest_remaining_resume()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var client = factory.Client(1);
        var first = await UploadedId(client, "first.pdf", "application/pdf", ValidFile("pdf"));
        var second = await UploadedId(client, "second.pdf", "application/pdf", ValidFile("pdf"));
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/api/candidates/me/resumes/{first}")).StatusCode);
        var json = JsonDocument.Parse(await (await client.GetAsync("/api/candidates/me/resumes")).Content.ReadAsStringAsync());
        var only = Assert.Single(json.RootElement.EnumerateArray());
        Assert.Equal(second, only.GetProperty("resumeId").GetInt32()); Assert.True(only.GetProperty("isDefault").GetBoolean());
    }

    [Fact]
    public async Task Authentication_is_required_and_non_candidate_is_forbidden()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/candidates/me/resumes")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await factory.Client(99, "Recruiter").GetAsync("/api/candidates/me/resumes")).StatusCode);
    }

    [Fact]
    public async Task Deleting_resume_referenced_by_application_returns_409()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var client = factory.Client(1);
        var resumeId = await UploadedId(client, "submitted.pdf", "application/pdf", ValidFile("pdf"));
        var jobId = await factory.SeedJobAsync();
        await factory.AddApplicationAsync(1, jobId, resumeId);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/api/candidates/me/resumes/{resumeId}")).StatusCode);
    }

    [Fact]
    public async Task Applying_with_another_candidates_resume_is_rejected()
    {
        await using var factory = await ResumeApiFactory.CreateAsync();
        var resumeId = await UploadedId(factory.Client(1), "private.pdf", "application/pdf", ValidFile("pdf"));
        var jobId = await factory.SeedJobAsync();
        var response = await factory.Client(2).PostAsJsonAsync($"/api/jobs/{jobId}/applications", new { coverLetter="Qualified candidate", resumeId });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Candidate_settings_persist_and_password_change_replaces_credentials()
    {
        await using var factory = await ResumeApiFactory.CreateAsync(); var client=factory.Client(1);
        Assert.Equal(HttpStatusCode.OK,(await client.GetAsync("/api/candidates/me/settings")).StatusCode);
        var updated=await client.PutAsJsonAsync("/api/candidates/me/settings",new { emailNotifications=false,applicationStatusNotifications=true,interviewReminders=false,jobRecommendationNotifications=true });
        Assert.Equal(HttpStatusCode.OK,updated.StatusCode);
        var saved=JsonDocument.Parse(await (await client.GetAsync("/api/candidates/me/settings")).Content.ReadAsStringAsync()).RootElement;
        Assert.False(saved.GetProperty("emailNotifications").GetBoolean()); Assert.False(saved.GetProperty("interviewReminders").GetBoolean());
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.PutAsJsonAsync("/api/candidates/me/password",new { currentPassword="WrongPassword1",newPassword="NewPassword123",confirmNewPassword="NewPassword123" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PutAsJsonAsync("/api/candidates/me/password",new { currentPassword="CurrentPassword123",newPassword="weak",confirmNewPassword="different" })).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await client.PutAsJsonAsync("/api/candidates/me/password",new { currentPassword="CurrentPassword123",newPassword="NewPassword123",confirmNewPassword="NewPassword123" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new { email="one@test.local",password="CurrentPassword123" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new { email="one@test.local",password="NewPassword123" })).StatusCode);
    }

    [Fact]
    public async Task Candidate_settings_require_candidate_authorization()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/candidates/me/settings")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(99,"Recruiter").GetAsync("/api/candidates/me/settings")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_dashboard_stats_are_owned_and_authorized()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync();
        var response=await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/recruiter/stats");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var stats=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(1,stats.GetProperty("totalJobs").GetInt32()); Assert.Equal(1,stats.GetProperty("totalApplications").GetInt32()); Assert.Equal(1,stats.GetProperty("totalCandidates").GetInt32()); Assert.Equal(3,stats.GetProperty("interviewsThisMonth").GetInt32());
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/recruiter/stats")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").GetAsync("/api/dashboard/recruiter/stats")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_dashboard_empty_profile_returns_zero_values()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync();
        var json=JsonDocument.Parse(await (await factory.Client(5,"Recruiter").GetAsync("/api/dashboard/recruiter/stats")).Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0,json.GetProperty("totalJobs").GetInt32()); Assert.Equal(0,json.GetProperty("totalApplications").GetInt32());
    }

    [Fact]
    public async Task Recruiter_recent_applicants_are_owned_and_authorized()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync();
        var response=await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/recruiter/recent-applicants");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode); var rows=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var row=Assert.Single(rows.EnumerateArray()); Assert.Equal("One Candidate",row.GetProperty("candidateName").GetString()); Assert.Equal("Owned Job",row.GetProperty("jobTitle").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/recruiter/recent-applicants")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").GetAsync("/api/dashboard/recruiter/recent-applicants")).StatusCode);
        var empty=JsonDocument.Parse(await (await factory.Client(5,"Recruiter").GetAsync("/api/dashboard/recruiter/recent-applicants")).Content.ReadAsStringAsync()).RootElement;
        Assert.Empty(empty.EnumerateArray());
    }

    [Fact]
    public async Task Recruiter_upcoming_interviews_are_future_owned_and_authorized()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync();
        var response=await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/recruiter/upcoming-interviews");
        Assert.Equal(HttpStatusCode.OK,response.StatusCode); var rows=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        var row=Assert.Single(rows.EnumerateArray()); Assert.Equal("One Candidate",row.GetProperty("candidateName").GetString()); Assert.Equal("Scheduled",row.GetProperty("status").GetString());
        Assert.True(row.GetProperty("scheduledAt").GetDateTime()>DateTime.UtcNow);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/recruiter/upcoming-interviews")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").GetAsync("/api/dashboard/recruiter/upcoming-interviews")).StatusCode);
        var empty=JsonDocument.Parse(await (await factory.Client(5,"Recruiter").GetAsync("/api/dashboard/recruiter/upcoming-interviews")).Content.ReadAsStringAsync()).RootElement; Assert.Empty(empty.EnumerateArray());
    }

    [Fact]
    public async Task Recruiter_applications_overview_has_six_owned_chronological_months()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync();
        var response=await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/recruiter/applications-overview"); Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var months=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("months").EnumerateArray().ToList(); Assert.Equal(6,months.Count);
        Assert.Equal(1,months[^1].GetProperty("totalApplications").GetInt32()); Assert.Equal(5,months.Take(5).Count(x=>x.GetProperty("totalApplications").GetInt32()==0));
        Assert.True(months.Zip(months.Skip(1),(a,b)=>string.CompareOrdinal(a.GetProperty("month").GetString(),b.GetProperty("month").GetString())<0).All(x=>x));
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/recruiter/applications-overview")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").GetAsync("/api/dashboard/recruiter/applications-overview")).StatusCode);
        var empty=JsonDocument.Parse(await (await factory.Client(5,"Recruiter").GetAsync("/api/dashboard/recruiter/applications-overview")).Content.ReadAsStringAsync()).RootElement.GetProperty("months").EnumerateArray(); Assert.All(empty,x=>Assert.Equal(0,x.GetProperty("totalApplications").GetInt32()));
    }

    [Fact]
    public async Task Recruiter_can_create_draft_and_published_jobs_with_owned_references()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync(); var recruiter=factory.Client(3,"Recruiter");
        var departments=JsonDocument.Parse(await (await recruiter.GetAsync("/api/recruiter/departments")).Content.ReadAsStringAsync()).RootElement; var departmentId=Assert.Single(departments.EnumerateArray()).GetProperty("id").GetInt32();
        Assert.Equal(HttpStatusCode.OK,(await recruiter.GetAsync("/api/recruiter/skills")).StatusCode);
        object Request(string title,bool publish)=>new{title,departmentId,location="Colombo",employmentType="Full-time",description="Build reliable software",responsibilities="Develop features",requirements="C#",minimumSalary=100m,maximumSalary=200m,closingDate=DateTime.UtcNow.AddDays(10),skillIds=Array.Empty<int>(),publishNow=publish};
        var draft=await recruiter.PostAsJsonAsync("/api/recruiter/jobs",Request("Draft Test Job",false)); Assert.Equal(HttpStatusCode.Created,draft.StatusCode);
        var published=await recruiter.PostAsJsonAsync("/api/recruiter/jobs",Request("Published Test Job",true)); Assert.Equal(HttpStatusCode.Created,published.StatusCode);
        var candidateJobs=JsonDocument.Parse(await (await factory.Client(1).GetAsync("/api/jobs?pageSize=50")).Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray().Select(x=>x.GetProperty("title").GetString()).ToList();
        Assert.DoesNotContain("Draft Test Job",candidateJobs); Assert.Contains("Published Test Job",candidateJobs);
        Assert.Equal(HttpStatusCode.BadRequest,(await recruiter.PostAsJsonAsync("/api/recruiter/jobs",new{title="Past",departmentId,location="Colombo",employmentType="Full-time",description="x",closingDate=DateTime.UtcNow.AddDays(-1),skillIds=Array.Empty<int>()})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await recruiter.PostAsJsonAsync("/api/recruiter/jobs",new{title="Salary",departmentId,location="Colombo",employmentType="Full-time",description="x",minimumSalary=300,maximumSalary=100,closingDate=DateTime.UtcNow.AddDays(1),skillIds=Array.Empty<int>()})).StatusCode);
        var foreignDepartment=await factory.AddForeignDepartmentAsync(); Assert.Equal(HttpStatusCode.NotFound,(await recruiter.PostAsJsonAsync("/api/recruiter/jobs",new{title="Foreign",departmentId=foreignDepartment,location="Colombo",employmentType="Full-time",description="x",closingDate=DateTime.UtcNow.AddDays(1),skillIds=Array.Empty<int>()})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/recruiter/jobs",Request("No auth",false))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1,"Candidate").PostAsJsonAsync("/api/recruiter/jobs",Request("Candidate",false))).StatusCode);
    }

    [Fact]
    public async Task Recruiter_manage_jobs_enforces_ownership_lifecycle_and_delete_rules()
    {
        await using var factory=await ResumeApiFactory.CreateAsync(); await factory.SeedRecruiterDashboardAsync(); var recruiter=factory.Client(3,"Recruiter");
        var departmentId=JsonDocument.Parse(await (await recruiter.GetAsync("/api/recruiter/departments")).Content.ReadAsStringAsync()).RootElement.EnumerateArray().Single().GetProperty("id").GetInt32();
        var create=await recruiter.PostAsJsonAsync("/api/recruiter/jobs",new{title="Manage Me",departmentId,location="Colombo",employmentType="Full-time",description="Initial",closingDate=DateTime.UtcNow.AddDays(14),skillIds=Array.Empty<int>(),publishNow=false});
        Assert.Equal(HttpStatusCode.Created,create.StatusCode);var jobId=JsonDocument.Parse(await create.Content.ReadAsStringAsync()).RootElement.GetProperty("jobId").GetInt32();
        var list=JsonDocument.Parse(await (await recruiter.GetAsync("/api/recruiter/jobs?search=Manage&status=Draft&page=1&pageSize=5&sort=newest")).Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(1,list.GetProperty("totalItems").GetInt32());Assert.Equal(jobId,list.GetProperty("items")[0].GetProperty("jobId").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound,(await factory.Client(4,"Recruiter").GetAsync($"/api/recruiter/jobs/{jobId}")).StatusCode);
        var update=new{title="Managed Job",departmentId,location="Kandy",employmentType="Contract",description="Updated",responsibilities="Ship",requirements="C#",minimumSalary=100m,maximumSalary=200m,closingDate=DateTime.UtcNow.AddDays(20),skillIds=Array.Empty<int>()};
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PutAsJsonAsync($"/api/recruiter/jobs/{jobId}",update)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsync($"/api/recruiter/jobs/{jobId}/publish",null)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await recruiter.PatchAsync($"/api/recruiter/jobs/{jobId}/publish",null)).StatusCode);
        var candidateTitles=JsonDocument.Parse(await (await factory.Client(1).GetAsync("/api/jobs?pageSize=50")).Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray().Select(x=>x.GetProperty("title").GetString()).ToList();Assert.Contains("Managed Job",candidateTitles);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsync($"/api/recruiter/jobs/{jobId}/close",null)).StatusCode);
        candidateTitles=JsonDocument.Parse(await (await factory.Client(1).GetAsync("/api/jobs?pageSize=50")).Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray().Select(x=>x.GetProperty("title").GetString()).ToList();Assert.DoesNotContain("Managed Job",candidateTitles);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.DeleteAsync($"/api/recruiter/jobs/{jobId}")).StatusCode);
        var ownedList=JsonDocument.Parse(await (await recruiter.GetAsync("/api/recruiter/jobs?search=Owned Job")).Content.ReadAsStringAsync()).RootElement;var applicationJobId=ownedList.GetProperty("items")[0].GetProperty("jobId").GetInt32();
        Assert.Equal(HttpStatusCode.Conflict,(await recruiter.DeleteAsync($"/api/recruiter/jobs/{applicationJobId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/recruiter/jobs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/recruiter/jobs")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_applicants_are_owned_filterable_and_follow_status_workflow()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var candidate=factory.Client(1);var recruiter=factory.Client(3,"Recruiter");
        var resumeId=await UploadedId(candidate,"candidate.pdf","application/pdf",ValidFile("pdf"));var ids=await factory.AttachResumeToOwnedApplicationAsync(resumeId);
        var response=await recruiter.GetAsync($"/api/recruiter/applications?jobId={ids.JobId}&status=Submitted&search=One&page=1&pageSize=5&sort=newest");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var result=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;var row=Assert.Single(result.GetProperty("items").EnumerateArray());Assert.Equal(ids.ApplicationId,row.GetProperty("applicationId").GetInt32());Assert.Equal("One Candidate",row.GetProperty("candidateName").GetString());
        var details=await recruiter.GetAsync($"/api/recruiter/applications/{ids.ApplicationId}");Assert.Equal(HttpStatusCode.OK,details.StatusCode);var detail=JsonDocument.Parse(await details.Content.ReadAsStringAsync()).RootElement;Assert.Equal("Owned Job",detail.GetProperty("job").GetProperty("title").GetString());Assert.Equal(resumeId,detail.GetProperty("resume").GetProperty("resumeId").GetInt32());
        var download=await recruiter.GetAsync($"/api/recruiter/applications/{ids.ApplicationId}/resume");Assert.Equal(HttpStatusCode.OK,download.StatusCode);Assert.Equal("application/pdf",download.Content.Headers.ContentType?.MediaType);Assert.Contains("candidate.pdf",download.Content.Headers.ContentDisposition?.FileName??download.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal(HttpStatusCode.NotFound,(await factory.Client(4,"Recruiter").GetAsync($"/api/recruiter/applications/{ids.ApplicationId}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsJsonAsync($"/api/recruiter/applications/{ids.ApplicationId}/status",new{status="UnderReview"})).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsJsonAsync($"/api/recruiter/applications/{ids.ApplicationId}/status",new{status="Shortlisted"})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await recruiter.PatchAsJsonAsync($"/api/recruiter/applications/{ids.ApplicationId}/status",new{status="UnderReview"})).StatusCode);
        var candidateApplications=JsonDocument.Parse(await (await candidate.GetAsync("/api/candidates/me/applications")).Content.ReadAsStringAsync()).RootElement;Assert.Equal("Shortlisted",candidateApplications.GetProperty("items")[0].GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/recruiter/applications")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await candidate.GetAsync("/api/recruiter/applications")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_interview_lifecycle_is_owned_and_updates_candidate_application()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var applicationId=await factory.PrepareShortlistedApplicationAsync();var recruiter=factory.Client(3,"Recruiter");
        object Request(DateTime when)=>new{applicationId,scheduledAt=when,durationMinutes=60,location="Online",meetingLink="https://meet.example.com/interview",notes="Technical interview"};
        var create=await recruiter.PostAsJsonAsync("/api/recruiter/interviews",Request(DateTime.UtcNow.AddDays(2)));Assert.Equal(HttpStatusCode.Created,create.StatusCode);var interviewId=JsonDocument.Parse(await create.Content.ReadAsStringAsync()).RootElement.GetProperty("interviewId").GetInt32();
        Assert.Equal(HttpStatusCode.Conflict,(await recruiter.PostAsJsonAsync("/api/recruiter/interviews",Request(DateTime.UtcNow.AddDays(3)))).StatusCode);
        var list=await recruiter.GetAsync("/api/recruiter/interviews?status=Scheduled&search=One&page=1&pageSize=5&sort=soonest");Assert.Equal(HttpStatusCode.OK,list.StatusCode);Assert.Single(JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray());
        Assert.Equal(HttpStatusCode.OK,(await recruiter.GetAsync($"/api/recruiter/interviews/{interviewId}")).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await factory.Client(4,"Recruiter").GetAsync($"/api/recruiter/interviews/{interviewId}")).StatusCode);
        var candidateDetail=JsonDocument.Parse(await (await factory.Client(1).GetAsync($"/api/candidates/me/applications/{applicationId}")).Content.ReadAsStringAsync()).RootElement;Assert.Equal("Interviewing",candidateDetail.GetProperty("status").GetString());Assert.Equal(interviewId,candidateDetail.GetProperty("interview").GetProperty("interviewId").GetInt32());
        var update=new{scheduledAt=DateTime.UtcNow.AddDays(4),durationMinutes=45,location="Office",meetingLink=(string?)null,notes="Updated"};Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PutAsJsonAsync($"/api/recruiter/interviews/{interviewId}",update)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await recruiter.PatchAsync($"/api/recruiter/interviews/{interviewId}/complete",null)).StatusCode);Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsync($"/api/recruiter/interviews/{interviewId}/cancel",null)).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await recruiter.PatchAsync($"/api/recruiter/interviews/{interviewId}/cancel",null)).StatusCode);
        create=await recruiter.PostAsJsonAsync("/api/recruiter/interviews",Request(DateTime.UtcNow.AddHours(2)));interviewId=JsonDocument.Parse(await create.Content.ReadAsStringAsync()).RootElement.GetProperty("interviewId").GetInt32();await factory.MakeInterviewDueAsync(interviewId);Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PatchAsync($"/api/recruiter/interviews/{interviewId}/complete",null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/recruiter/interviews")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/recruiter/interviews")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_analytics_are_owned_zero_filled_and_authorized()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var recruiter=factory.Client(3,"Recruiter");
        var from=DateTime.UtcNow.AddMonths(-1).ToString("O");var to=DateTime.UtcNow.AddMonths(1).ToString("O");
        var response=await recruiter.GetAsync($"/api/recruiter/analytics?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var analytics=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;var summary=analytics.GetProperty("summary");
        Assert.Equal(1,summary.GetProperty("totalJobs").GetInt32());Assert.Equal(1,summary.GetProperty("totalApplications").GetInt32());Assert.Equal(2,summary.GetProperty("interviewsScheduled").GetInt32());
        var top=Assert.Single(analytics.GetProperty("topJobs").EnumerateArray());Assert.Equal("Owned Job",top.GetProperty("jobTitle").GetString());Assert.Equal(1,top.GetProperty("applicationCount").GetInt32());
        Assert.Equal(3,analytics.GetProperty("applicationsTrend").GetArrayLength());Assert.Equal(8,analytics.GetProperty("applicationsByStatus").GetArrayLength());
        var empty=JsonDocument.Parse(await (await factory.Client(5,"Recruiter").GetAsync($"/api/recruiter/analytics?from={Uri.EscapeDataString(from)}&to={Uri.EscapeDataString(to)}")).Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(0,empty.GetProperty("summary").GetProperty("totalApplications").GetInt32());Assert.All(empty.GetProperty("applicationsTrend").EnumerateArray(),x=>Assert.Equal(0,x.GetProperty("applications").GetInt32()));
        Assert.Equal(HttpStatusCode.BadRequest,(await recruiter.GetAsync("/api/recruiter/analytics?from=2026-12-01&to=2026-01-01")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/recruiter/analytics")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/recruiter/analytics")).StatusCode);
    }

    [Fact]
    public async Task Recruiter_settings_persist_refresh_identity_and_change_password()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var recruiter=factory.Client(3,"Recruiter");
        var get=await recruiter.GetAsync("/api/recruiters/me/settings");Assert.Equal(HttpStatusCode.OK,get.StatusCode);var initial=JsonDocument.Parse(await get.Content.ReadAsStringAsync()).RootElement;Assert.True(initial.GetProperty("jobApplicationNotifications").GetBoolean());
        var update=new{firstName=" Sarah ",lastName=" Fernando ",phone=" 0771234567 ",jobApplicationNotifications=false,interviewNotifications=true,candidateStatusNotifications=false,emailNotifications=true};
        var saved=await recruiter.PutAsJsonAsync("/api/recruiters/me/settings",update);Assert.Equal(HttpStatusCode.OK,saved.StatusCode);
        var refreshed=JsonDocument.Parse(await (await recruiter.GetAsync("/api/recruiters/me/settings")).Content.ReadAsStringAsync()).RootElement;Assert.Equal("Sarah",refreshed.GetProperty("firstName").GetString());Assert.Equal("0771234567",refreshed.GetProperty("phone").GetString());Assert.False(refreshed.GetProperty("jobApplicationNotifications").GetBoolean());
        var me=JsonDocument.Parse(await (await recruiter.GetAsync("/api/auth/me")).Content.ReadAsStringAsync()).RootElement;Assert.Equal("Sarah Fernando",me.GetProperty("fullName").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized,(await recruiter.PutAsJsonAsync("/api/recruiters/me/password",new{currentPassword="WrongPassword",newPassword="NewRecruiter123",confirmNewPassword="NewRecruiter123"})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await recruiter.PutAsJsonAsync("/api/recruiters/me/password",new{currentPassword="CurrentRecruiter123",newPassword="weak",confirmNewPassword="weak"})).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await recruiter.PutAsJsonAsync("/api/recruiters/me/password",new{currentPassword="CurrentRecruiter123",newPassword="NewRecruiter123",confirmNewPassword="NewRecruiter123"})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="stats-one@test.local",password="CurrentRecruiter123"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="stats-one@test.local",password="NewRecruiter123"})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/recruiters/me/settings")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/recruiters/me/settings")).StatusCode);
    }

    [Fact]
    public async Task Hiring_manager_dashboard_returns_safe_empty_scope_and_is_role_protected()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();
        var response=await factory.Client(6,"HiringManager").GetAsync("/api/dashboard/hiring-manager/stats");Assert.Equal(HttpStatusCode.OK,response.StatusCode);
        var stats=JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;Assert.Equal(0,stats.GetProperty("pendingReviews").GetInt32());Assert.Equal(0,stats.GetProperty("todaysInterviews").GetInt32());Assert.Equal(0,stats.GetProperty("teamFeedback").GetInt32());Assert.Equal(0,stats.GetProperty("hiredThisMonth").GetInt32());
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/hiring-manager/stats")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/dashboard/hiring-manager/stats")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/hiring-manager/stats")).StatusCode);
    }

    [Fact]
    public async Task Hiring_manager_candidates_use_secure_empty_scope_and_role_protection()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var manager=factory.Client(6,"HiringManager");
        var list=await manager.GetAsync("/api/manager/candidates?search=One&status=Shortlisted&jobId=1&page=0&pageSize=100&sort=oldest");Assert.Equal(HttpStatusCode.OK,list.StatusCode);
        var page=JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement;Assert.Equal(0,page.GetProperty("items").GetArrayLength());Assert.Equal(1,page.GetProperty("page").GetInt32());Assert.Equal(50,page.GetProperty("pageSize").GetInt32());Assert.Equal(0,page.GetProperty("totalItems").GetInt32());
        var latest=await manager.GetAsync("/api/dashboard/hiring-manager/candidates-to-review");Assert.Equal(HttpStatusCode.OK,latest.StatusCode);Assert.Equal(0,JsonDocument.Parse(await latest.Content.ReadAsStringAsync()).RootElement.GetArrayLength());
        Assert.Equal(HttpStatusCode.NotFound,(await manager.GetAsync("/api/manager/candidates/1")).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await manager.GetAsync("/api/manager/candidates/1/resume")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await manager.GetAsync("/api/manager/candidates?status=NotAStatus")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/manager/candidates")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/manager/candidates")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/manager/candidates")).StatusCode);
    }

    [Fact]
    public async Task Hiring_manager_can_create_read_and_update_own_evaluation()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var applicationId=await factory.SeedHiringManagerEvaluationScopeAsync();var manager=factory.Client(6,"HiringManager");
        var request=new{technicalScore=8,communicationScore=7,experienceScore=8,cultureFitScore=9,overallComments="Strong candidate",recommendation="Proceed"};
        var created=await manager.PostAsJsonAsync($"/api/manager/applications/{applicationId}/evaluations",request);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var dto=JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement;Assert.Equal(8m,dto.GetProperty("overallScore").GetDecimal());var evaluationId=dto.GetProperty("evaluationId").GetInt32();
        Assert.Equal(HttpStatusCode.Conflict,(await manager.PostAsJsonAsync($"/api/manager/applications/{applicationId}/evaluations",request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await manager.PostAsJsonAsync($"/api/manager/applications/{applicationId}/evaluations",new{technicalScore=11,communicationScore=7,experienceScore=8,cultureFitScore=9,overallComments="Invalid",recommendation="Proceed"})).StatusCode);
        var list=await manager.GetAsync($"/api/manager/applications/{applicationId}/evaluations");Assert.Equal(HttpStatusCode.OK,list.StatusCode);Assert.Single(JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.EnumerateArray());
        var updated=await manager.PutAsJsonAsync($"/api/manager/evaluations/{evaluationId}",new{technicalScore=10,communicationScore=10,experienceScore=9,cultureFitScore=9,overallComments="Updated",recommendation="Hold"});Assert.Equal(HttpStatusCode.OK,updated.StatusCode);Assert.Equal(9.5m,JsonDocument.Parse(await updated.Content.ReadAsStringAsync()).RootElement.GetProperty("overallScore").GetDecimal());
        Assert.Equal(HttpStatusCode.NotFound,(await factory.Client(7,"HiringManager").PutAsJsonAsync($"/api/manager/evaluations/{evaluationId}",request)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync($"/api/manager/applications/{applicationId}/evaluations")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync($"/api/manager/applications/{applicationId}/evaluations")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync($"/api/manager/applications/{applicationId}/evaluations")).StatusCode);
        var summary=await manager.GetAsync("/api/dashboard/hiring-manager/evaluation-summary");Assert.Equal(HttpStatusCode.OK,summary.StatusCode);Assert.Equal(1,JsonDocument.Parse(await summary.Content.ReadAsStringAsync()).RootElement.GetProperty("completedEvaluations").GetInt32());
    }

    [Fact]
    public async Task Hiring_manager_decisions_enforce_evaluation_and_final_transitions()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var first=await factory.SeedHiringManagerEvaluationScopeAsync();var second=await factory.AddSecondManagerReviewApplicationAsync();var manager=factory.Client(6,"HiringManager");var evaluation=new{technicalScore=8,communicationScore=8,experienceScore=8,cultureFitScore=8,overallComments="Ready",recommendation="Proceed"};
        Assert.Equal(HttpStatusCode.Conflict,(await manager.PostAsJsonAsync($"/api/manager/applications/{second}/hiring-decision",new{decision="Hire",notes="No evaluation"})).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await manager.PostAsJsonAsync($"/api/manager/applications/{first}/evaluations",evaluation)).StatusCode);
        var hold=await manager.PostAsJsonAsync($"/api/manager/applications/{first}/hiring-decision",new{decision="Hold",notes="Discuss"});Assert.Equal(HttpStatusCode.Created,hold.StatusCode);
        var hire=await manager.PutAsJsonAsync($"/api/manager/applications/{first}/hiring-decision",new{decision="Hire",notes="Approved"});Assert.Equal(HttpStatusCode.OK,hire.StatusCode);Assert.Equal("Hired",JsonDocument.Parse(await hire.Content.ReadAsStringAsync()).RootElement.GetProperty("applicationStatus").GetString());
        Assert.Equal(HttpStatusCode.Conflict,(await manager.PutAsJsonAsync($"/api/manager/applications/{first}/hiring-decision",new{decision="Reject",notes="Reverse"})).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await manager.PostAsJsonAsync($"/api/manager/applications/{first}/hiring-decision",new{decision="Hire",notes="Duplicate"})).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await manager.PostAsJsonAsync($"/api/manager/applications/{second}/evaluations",evaluation)).StatusCode);Assert.Equal(HttpStatusCode.Created,(await manager.PostAsJsonAsync($"/api/manager/applications/{second}/hiring-decision",new{decision="Reject",notes="Not selected"})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await manager.GetAsync("/api/manager/hiring-decisions")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await manager.GetAsync("/api/dashboard/hiring-manager/hiring-funnel")).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/manager/hiring-decisions")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/manager/hiring-decisions")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/manager/hiring-decisions")).StatusCode);
    }

    [Fact]
    public async Task Hiring_manager_reports_are_scoped_filtered_and_role_protected()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var app=await factory.SeedHiringManagerEvaluationScopeAsync();var manager=factory.Client(6,"HiringManager");await manager.PostAsJsonAsync($"/api/manager/applications/{app}/evaluations",new{technicalScore=8,communicationScore=7,experienceScore=8,cultureFitScore=9,overallComments="Report",recommendation="Proceed"});await manager.PostAsJsonAsync($"/api/manager/applications/{app}/hiring-decision",new{decision="Hire",notes="Report hire"});
        foreach(var path in new[]{"summary","application-trends","hiring-funnel","job-performance","decision-outcomes"})Assert.Equal(HttpStatusCode.OK,(await manager.GetAsync($"/api/manager/reports/{path}")).StatusCode);
        var trends=JsonDocument.Parse(await(await manager.GetAsync("/api/manager/reports/application-trends?from=2026-01-01&to=2026-07-31")).Content.ReadAsStringAsync()).RootElement.EnumerateArray().Select(x=>x.GetProperty("period").GetString()).ToList();Assert.Equal(trends.OrderBy(x=>x).ToList(),trends);
        Assert.Equal(HttpStatusCode.BadRequest,(await manager.GetAsync("/api/manager/reports/summary?from=2026-07-31&to=2026-01-01")).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await manager.GetAsync("/api/manager/reports/summary?jobId=999999")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/manager/reports/summary")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/manager/reports/summary")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/manager/reports/summary")).StatusCode);
    }

    [Fact]
    public async Task Hiring_manager_settings_profile_preferences_and_password_persist()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();await factory.SeedHiringManagerEvaluationScopeAsync();var manager=factory.Client(6,"HiringManager");var get=await manager.GetAsync("/api/managers/me/settings");Assert.Equal(HttpStatusCode.OK,get.StatusCode);var initial=JsonDocument.Parse(await get.Content.ReadAsStringAsync()).RootElement;Assert.True(initial.GetProperty("candidateReviewNotifications").GetBoolean());Assert.Equal("Engineering",initial.GetProperty("departmentName").GetString());
        var update=new{firstName=" Dr. Test ",lastName=" Manager ",phone=" 0771234567 ",candidateReviewNotifications=false,interviewNotifications=true,evaluationNotifications=false,decisionNotifications=true,emailNotifications=false};var saved=await manager.PutAsJsonAsync("/api/managers/me/settings",update);Assert.Equal(HttpStatusCode.OK,saved.StatusCode);var refreshed=JsonDocument.Parse(await(await manager.GetAsync("/api/managers/me/settings")).Content.ReadAsStringAsync()).RootElement;Assert.Equal("Dr. Test",refreshed.GetProperty("firstName").GetString());Assert.False(refreshed.GetProperty("candidateReviewNotifications").GetBoolean());
        Assert.Equal(HttpStatusCode.Unauthorized,(await manager.PutAsJsonAsync("/api/managers/me/password",new{currentPassword="Wrong",newPassword="NewManager123",confirmNewPassword="NewManager123"})).StatusCode);Assert.Equal(HttpStatusCode.BadRequest,(await manager.PutAsJsonAsync("/api/managers/me/password",new{currentPassword="CurrentManager123",newPassword="weak",confirmNewPassword="weak"})).StatusCode);Assert.Equal(HttpStatusCode.NoContent,(await manager.PutAsJsonAsync("/api/managers/me/password",new{currentPassword="CurrentManager123",newPassword="NewManager123",confirmNewPassword="NewManager123"})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="manager@test.local",password="CurrentManager123"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="manager@test.local",password="NewManager123"})).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/managers/me/settings")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/managers/me/settings")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/managers/me/settings")).StatusCode);
    }

    [Fact]
    public async Task Admin_dashboard_endpoints_return_safe_data_and_are_role_protected()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");
        foreach(var path in new[]{"stats","user-growth","application-overview","recent-activity","system-health"})Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/dashboard/admin/{path}")).StatusCode);
        var growth=JsonDocument.Parse(await(await admin.GetAsync("/api/dashboard/admin/user-growth?from=2026-01-01&to=2026-07-31")).Content.ReadAsStringAsync()).RootElement.EnumerateArray().Select(x=>x.GetProperty("period").GetString()).ToList();Assert.Equal(growth.OrderBy(x=>x).ToList(),growth);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.GetAsync("/api/dashboard/admin/user-growth?from=2026-07-31&to=2026-01-01")).StatusCode);var health=await(await admin.GetAsync("/api/dashboard/admin/system-health")).Content.ReadAsStringAsync();Assert.DoesNotContain("connectionString",health,StringComparison.OrdinalIgnoreCase);Assert.DoesNotContain("password",health,StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/dashboard/admin/stats")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/dashboard/admin/stats")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/dashboard/admin/stats")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/dashboard/admin/stats")).StatusCode);
    }

    [Fact]
    public async Task Admin_user_management_creates_filters_deactivates_and_resets_passwords()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");var (idsOrg,idsDept)=await factory.GetOrganizationDepartmentIdsAsync();
        var candidate=new{firstName="New",lastName="Candidate",email="admin-created@test.local",phone="0771234567",role="Candidate",organizationId=(int?)null,departmentId=(int?)null,temporaryPassword="Temporary123",isActive=true};var created=await admin.PostAsJsonAsync("/api/admin/users",candidate);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var candidateId=JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement.GetProperty("userId").GetInt32();Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync("/api/admin/users",candidate)).StatusCode);
        Assert.Equal(HttpStatusCode.Created,(await admin.PostAsJsonAsync("/api/admin/users",new{firstName="New",lastName="Recruiter",email="admin-recruiter@test.local",phone="",role="Recruiter",organizationId=idsOrg,departmentId=(int?)null,temporaryPassword="Temporary123",isActive=true})).StatusCode);Assert.Equal(HttpStatusCode.Created,(await admin.PostAsJsonAsync("/api/admin/users",new{firstName="New",lastName="Manager",email="admin-manager@test.local",phone="",role="HiringManager",organizationId=idsOrg,departmentId=idsDept,temporaryPassword="Temporary123",isActive=true})).StatusCode);
        var list=await admin.GetAsync("/api/admin/users?search=admin-created&role=Candidate&status=Active&page=1&pageSize=1");Assert.Equal(HttpStatusCode.OK,list.StatusCode);Assert.Equal(1,JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.GetProperty("items").GetArrayLength());Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/users/{candidateId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/users/{candidateId}/status",new{isActive=false,reason="Test suspension"})).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="admin-created@test.local",password="Temporary123"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/users/{candidateId}/status",new{isActive=true,reason=""})).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent,(await admin.PostAsJsonAsync($"/api/admin/users/{candidateId}/reset-password",new{temporaryPassword="NewTemporary123",confirmTemporaryPassword="NewTemporary123",requirePasswordChange=true})).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="admin-created@test.local",password="Temporary123"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await factory.CreateClient().PostAsJsonAsync("/api/auth/login",new{email="admin-created@test.local",password="NewTemporary123"})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PatchAsJsonAsync("/api/admin/users/8/status",new{isActive=false,reason="self"})).StatusCode);Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/users")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/users")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/users")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/users")).StatusCode);
    }

    [Fact]
    public async Task Admin_role_permissions_are_validated_protected_reset_and_audited()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");
        var roles=await admin.GetAsync("/api/admin/roles");Assert.Equal(HttpStatusCode.OK,roles.StatusCode);Assert.Equal(4,JsonDocument.Parse(await roles.Content.ReadAsStringAsync()).RootElement.GetArrayLength());
        var details=await admin.GetAsync("/api/admin/roles/Recruiter");Assert.Equal(HttpStatusCode.OK,details.StatusCode);
        var catalog=JsonDocument.Parse(await(await admin.GetAsync("/api/admin/permissions")).Content.ReadAsStringAsync()).RootElement.EnumerateArray().Select(x=>x.GetProperty("code").GetString()).ToList();Assert.Equal(catalog.Count,catalog.Distinct().Count());
        Assert.Equal(HttpStatusCode.NotFound,(await admin.GetAsync("/api/admin/roles/Unknown")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PutAsJsonAsync("/api/admin/roles/Recruiter/permissions",new{permissionCodes=new[]{"unknown.permission"}})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PutAsJsonAsync("/api/admin/roles/Recruiter/permissions",new{permissionCodes=new[]{"admin.users.view"}})).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PutAsJsonAsync("/api/admin/roles/Admin/permissions",new{permissionCodes=new[]{"admin.dashboard.view"}})).StatusCode);
        var defaults=JobOrbit.Application.Authorization.PermissionConstants.Defaults[JobOrbit.Domain.Enums.UserRole.Recruiter].ToArray();Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync("/api/admin/roles/Recruiter/permissions",new{permissionCodes=defaults})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PostAsync("/api/admin/roles/Recruiter/permissions/reset",null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/roles")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/roles")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/roles")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/roles")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.True(await db.AuditLogs.CountAsync(x=>x.Action=="AdminUpdateRolePermissions"||x.Action=="AdminResetRolePermissions")>=2);
    }

    [Fact]
    public async Task Admin_organization_management_supports_crud_filters_status_security_and_audit()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");
        var request=new{name=" Acme Technologies ",code=" acme ",description="Technology services",email="contact@acme.test",phone="0119876543",websiteUrl="https://acme.example.com",addressLine1="25 Galle Road",addressLine2=(string?)null,city="Colombo",stateOrProvince="Western Province",postalCode="00300",country="Sri Lanka",isActive=true};
        var created=await admin.PostAsJsonAsync("/api/admin/organizations",request);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var body=JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement;var id=body.GetProperty("organizationId").GetInt32();Assert.Equal("ACME",body.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync("/api/admin/organizations",new{request.name,request.code,request.description,request.email,request.phone,request.websiteUrl,request.addressLine1,request.addressLine2,request.city,request.stateOrProvince,request.postalCode,request.country,request.isActive})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PostAsJsonAsync("/api/admin/organizations",new{name="Bad Email",code="BADMAIL",email="not-an-email",websiteUrl="https://valid.test"})).StatusCode);Assert.Equal(HttpStatusCode.BadRequest,(await admin.PostAsJsonAsync("/api/admin/organizations",new{name="Bad URL",code="BADURL",email="valid@test.local",websiteUrl="invalid"})).StatusCode);
        var list=await admin.GetAsync("/api/admin/organizations?search=ACME&status=Active&page=1&pageSize=1&sort=code");Assert.Equal(HttpStatusCode.OK,list.StatusCode);Assert.Equal(1,JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.GetProperty("items").GetArrayLength());Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/organizations/{id}")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync("/api/admin/organizations/lookup")).StatusCode);
        var updated=await admin.PutAsJsonAsync($"/api/admin/organizations/{id}",new{name="Acme Digital",code="ACME",description="Updated",email="info@acme.test",phone="0119876543",websiteUrl="https://acme.example.com",addressLine1="25 Galle Road",addressLine2=(string?)null,city="Kandy",stateOrProvince="Central",postalCode="20000",country="Sri Lanka",isActive=true});Assert.Equal(HttpStatusCode.OK,updated.StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/organizations/{id}/status",new{isActive=false,reason="Suspended for test"})).StatusCode);var inactive=await admin.GetAsync("/api/admin/organizations?status=Inactive&search=ACME");Assert.Contains(JsonDocument.Parse(await inactive.Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray(),x=>x.GetProperty("organizationId").GetInt32()==id);Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/organizations/{id}/status",new{isActive=true,reason=""})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/organizations")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/organizations")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/organizations")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/organizations")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.True(await db.AuditLogs.CountAsync(x=>x.EntityName==nameof(Organization)&&x.EntityId==id)>=4);
    }

    [Fact]
    public async Task Admin_department_management_enforces_scope_status_move_security_and_audit()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");var org=(await factory.GetOrganizationDepartmentIdsAsync()).OrganizationId;
        var request=new{organizationId=org,name=" Product Engineering ",code=" prod ",description="Product team",email="product@test.local",phone="0112345678",isActive=true};var created=await admin.PostAsJsonAsync("/api/admin/departments",request);Assert.Equal(HttpStatusCode.Created,created.StatusCode);var body=JsonDocument.Parse(await created.Content.ReadAsStringAsync()).RootElement;var id=body.GetProperty("departmentId").GetInt32();Assert.Equal("PROD",body.GetProperty("code").GetString());
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync("/api/admin/departments",request)).StatusCode);var list=await admin.GetAsync($"/api/admin/departments?search=PROD&organizationId={org}&status=Active&page=1&pageSize=1");Assert.Equal(HttpStatusCode.OK,list.StatusCode);Assert.Single(JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.GetProperty("items").EnumerateArray());Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/departments/{id}")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/departments/lookup?organizationId={org}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,(await admin.PutAsJsonAsync($"/api/admin/departments/{id}",new{organizationId=org+999,name="Moved",code="MOVE",description="",email="",phone="",isActive=true})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync($"/api/admin/departments/{id}",new{organizationId=org,name="Product and Platform",code="PROD",description="Updated",email="product@test.local",phone="",isActive=true})).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/departments/{id}/status",new{isActive=false,reason="Pause"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/departments/{id}/status",new{isActive=true,reason=""})).StatusCode);
        var inactiveOrg=await admin.PostAsJsonAsync("/api/admin/organizations",new{name="Inactive Department Org",code="INACTIVE-DEPT",isActive=false});var inactiveOrgId=JsonDocument.Parse(await inactiveOrg.Content.ReadAsStringAsync()).RootElement.GetProperty("organizationId").GetInt32();Assert.Equal(HttpStatusCode.Conflict,(await admin.PostAsJsonAsync("/api/admin/departments",new{organizationId=inactiveOrgId,name="Nope",code="NOPE",isActive=true})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/departments")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/departments")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/departments")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/departments")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.True(await db.AuditLogs.CountAsync(x=>x.EntityName==nameof(Department)&&x.EntityId==id)>=4);
    }

    [Fact]
    public async Task Admin_jobs_management_preserves_ownership_validates_transitions_feature_and_audits()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var admin=factory.Client(8,"Administrator");var list=await admin.GetAsync("/api/admin/jobs?search=Owned&status=Published&page=1&pageSize=1");Assert.Equal(HttpStatusCode.OK,list.StatusCode);var row=JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement.GetProperty("items")[0];var id=row.GetProperty("jobId").GetInt32();var recruiterId=row.GetProperty("recruiterId").GetInt32();var applications=row.GetProperty("applicationCount").GetInt32();
        var details=await admin.GetAsync($"/api/admin/jobs/{id}");Assert.Equal(HttpStatusCode.OK,details.StatusCode);var update=new{title=" Moderated Owned Job ",description="Updated safely",requirements="C#",responsibilities="Build",skills=new[]{"C#","SQL"},employmentType="Full-time",workplaceType="Hybrid",location="Colombo",salaryMin=100000m,salaryMax=200000m,currency="LKR",experienceLevel="Mid",vacancyCount=2,closingDate=DateTime.UtcNow.AddDays(30)};Assert.Equal(HttpStatusCode.OK,(await admin.PutAsJsonAsync($"/api/admin/jobs/{id}",update)).StatusCode);var after=JsonDocument.Parse(await(await admin.GetAsync($"/api/admin/jobs/{id}")).Content.ReadAsStringAsync()).RootElement;Assert.Equal(recruiterId,after.GetProperty("recruiter").GetProperty("recruiterId").GetInt32());Assert.Equal(applications,after.GetProperty("applicationCount").GetInt32());
        Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/jobs/{id}/feature",new{isFeatured=true})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/jobs/{id}/status",new{status="Closed",reason="Policy review"})).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await admin.PatchAsJsonAsync($"/api/admin/jobs/{id}/feature",new{isFeatured=true})).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await admin.PatchAsJsonAsync($"/api/admin/jobs/{id}/status",new{status="Published",reason="Invalid reopen"})).StatusCode);after=JsonDocument.Parse(await(await admin.GetAsync($"/api/admin/jobs/{id}")).Content.ReadAsStringAsync()).RootElement;Assert.Equal(applications,after.GetProperty("applicationCount").GetInt32());Assert.False(after.GetProperty("isFeatured").GetBoolean());
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/jobs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/jobs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/jobs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/jobs")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.True(await db.AuditLogs.CountAsync(x=>x.EntityName==nameof(JobPosting)&&x.EntityId==id&&x.Action.StartsWith("Admin"))>=3);
    }

    [Fact]
    public async Task Admin_applications_support_filters_details_resume_controlled_override_history_and_security()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();var candidate=factory.Client(1);var resumeId=await UploadedId(candidate,"admin-review.pdf","application/pdf",ValidFile("pdf"));var ids=await factory.AttachResumeToOwnedApplicationAsync(resumeId);var admin=factory.Client(8,"Administrator");
        var list=await admin.GetAsync($"/api/admin/applications?search=One&status=Submitted&jobId={ids.JobId}&candidateId=1&page=1&pageSize=1&sort=newest");Assert.Equal(HttpStatusCode.OK,list.StatusCode);var root=JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement;Assert.Single(root.GetProperty("items").EnumerateArray());Assert.Equal(ids.ApplicationId,root.GetProperty("items")[0].GetProperty("applicationId").GetInt32());
        Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/applications/{ids.ApplicationId}")).StatusCode);var download=await admin.GetAsync($"/api/admin/applications/{ids.ApplicationId}/resume");Assert.Equal(HttpStatusCode.OK,download.StatusCode);Assert.Equal("application/pdf",download.Content.Headers.ContentType?.MediaType);Assert.Contains("admin-review.pdf",download.Content.Headers.ContentDisposition?.FileNameStar??download.Content.Headers.ContentDisposition?.FileName);
        Assert.Equal(HttpStatusCode.BadRequest,(await admin.PatchAsJsonAsync($"/api/admin/applications/{ids.ApplicationId}/status",new{status="UnderReview",reason=""})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.PatchAsJsonAsync($"/api/admin/applications/{ids.ApplicationId}/status",new{status="UnderReview",reason="Correct initial review state"})).StatusCode);Assert.Equal(HttpStatusCode.Conflict,(await admin.PatchAsJsonAsync($"/api/admin/applications/{ids.ApplicationId}/status",new{status="Hired",reason="Unsafe direct hire"})).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync($"/api/admin/applications/{ids.ApplicationId}/history")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/applications")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/applications")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/applications")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/applications")).StatusCode);
        await using var scope=factory.Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();Assert.True(await db.AuditLogs.AnyAsync(x=>x.EntityName==nameof(JobApplication)&&x.EntityId==ids.ApplicationId&&x.Action=="AdminOverrideApplicationStatus"));var manageIds=await db.Permissions.Where(x=>x.Code==JobOrbit.Application.Authorization.PermissionConstants.AdminApplicationsManage).Select(x=>x.Id).ToListAsync();db.RolePermissions.RemoveRange(await db.RolePermissions.Where(x=>x.Role==JobOrbit.Domain.Enums.UserRole.Administrator&&manageIds.Contains(x.PermissionId)).ToListAsync());await db.SaveChangesAsync();Assert.Equal(HttpStatusCode.Forbidden,(await admin.PatchAsJsonAsync($"/api/admin/applications/{ids.ApplicationId}/status",new{status="Shortlisted",reason="Permission must be enforced"})).StatusCode);
    }

    [Fact]
    public async Task Admin_audit_logs_are_filterable_sanitized_immutable_and_permission_protected()
    {
        await using var factory=await ResumeApiFactory.CreateAsync();await factory.SeedRecruiterDashboardAsync();int auditId;
        await using(var scope=factory.Services.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var log=new AuditLog{UserId=8,Action="AdminCreateUser",EntityName=nameof(User),EntityId=1,EntityDisplayName="One Candidate",Description="Created user safely",Severity=JobOrbit.Domain.Enums.AuditSeverity.Warning,IsSuccess=true,OldValues="{\"password\":\"secret\"}",NewValues="{\"email\":\"one@test.local\",\"accessToken\":\"secret\"}",CorrelationId="audit-test"};db.AuditLogs.Add(log);await db.SaveChangesAsync();auditId=log.Id;Assert.DoesNotContain("secret",log.OldValues!);Assert.DoesNotContain("secret",log.NewValues!);}
        var admin=factory.Client(8,"Administrator");var list=await admin.GetAsync("/api/admin/audit-logs?search=One&action=AdminCreateUser&entityType=User&actorRole=Admin&severity=Warning&isSuccess=true&page=1&pageSize=1&sort=newest");Assert.Equal(HttpStatusCode.OK,list.StatusCode);var root=JsonDocument.Parse(await list.Content.ReadAsStringAsync()).RootElement;Assert.Single(root.GetProperty("items").EnumerateArray());Assert.Equal(auditId,root.GetProperty("items")[0].GetProperty("auditLogId").GetInt32());
        var details=await admin.GetAsync($"/api/admin/audit-logs/{auditId}");Assert.Equal(HttpStatusCode.OK,details.StatusCode);var text=await details.Content.ReadAsStringAsync();Assert.Contains("[REDACTED]",text);Assert.DoesNotContain("secret",text,StringComparison.OrdinalIgnoreCase);Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync("/api/admin/audit-logs/actions")).StatusCode);Assert.Equal(HttpStatusCode.OK,(await admin.GetAsync("/api/admin/audit-logs/entity-types")).StatusCode);Assert.Equal(HttpStatusCode.NotFound,(await admin.GetAsync("/api/admin/audit-logs/999999")).StatusCode);Assert.Equal(HttpStatusCode.MethodNotAllowed,(await admin.DeleteAsync($"/api/admin/audit-logs/{auditId}")).StatusCode);Assert.Equal(HttpStatusCode.MethodNotAllowed,(await admin.PatchAsJsonAsync($"/api/admin/audit-logs/{auditId}",new{})).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,(await factory.CreateClient().GetAsync("/api/admin/audit-logs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(1).GetAsync("/api/admin/audit-logs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(3,"Recruiter").GetAsync("/api/admin/audit-logs")).StatusCode);Assert.Equal(HttpStatusCode.Forbidden,(await factory.Client(6,"HiringManager").GetAsync("/api/admin/audit-logs")).StatusCode);
        await using(var scope=factory.Services.CreateAsyncScope()){var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var ids=await db.Permissions.Where(x=>x.Code==JobOrbit.Application.Authorization.PermissionConstants.AdminAuditLogsView).Select(x=>x.Id).ToListAsync();db.RolePermissions.RemoveRange(await db.RolePermissions.Where(x=>x.Role==JobOrbit.Domain.Enums.UserRole.Administrator&&ids.Contains(x.PermissionId)).ToListAsync());await db.SaveChangesAsync();}Assert.Equal(HttpStatusCode.Forbidden,(await admin.GetAsync("/api/admin/audit-logs")).StatusCode);
    }

    private static async Task<HttpResponseMessage> Upload(HttpClient client, string name, string mime, byte[] bytes, string display = "Resume")
    {
        using var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(bytes); file.Headers.ContentType = MediaTypeHeaderValue.Parse(mime);
        form.Add(file, "file", name); form.Add(new StringContent(display), "displayName");
        return await client.PostAsync("/api/candidates/me/resumes", form);
    }
    private static async Task<int> UploadedId(HttpClient client, string name, string mime, byte[] bytes)
    {
        var response = await Upload(client, name, mime, bytes); response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.GetProperty("resumeId").GetInt32();
    }
    private static byte[] ValidFile(string format)
    {
        if (format == "pdf") return "%PDF-1.7\nresume"u8.ToArray();
        if (format == "doc") return [0xD0,0xCF,0x11,0xE0,0xA1,0xB1,0x1A,0xE1,0,0];
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, true)) { zip.CreateEntry("[Content_Types].xml"); zip.CreateEntry("word/document.xml"); }
        return stream.ToArray();
    }
}

internal sealed class ResumeApiFactory : WebApplicationFactory<Program>
{
    private readonly string dbName = Guid.NewGuid().ToString();
    public static async Task<ResumeApiFactory> CreateAsync()
    {
        var factory = new ResumeApiFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        var one=new User { Id=1, Email="one@test.local", FirstName="One", LastName="Candidate", Role=JobOrbit.Domain.Enums.UserRole.Candidate, CandidateProfile=new CandidateProfile { Id=1 } };
        var two=new User { Id=2, Email="two@test.local", FirstName="Two", LastName="Candidate", Role=JobOrbit.Domain.Enums.UserRole.Candidate, CandidateProfile=new CandidateProfile { Id=2 } };
        var hasher=new PasswordHasher<User>(); one.PasswordHash=hasher.HashPassword(one,"CurrentPassword123"); two.PasswordHash=hasher.HashPassword(two,"CurrentPassword123");
        db.Users.AddRange(one,two);
        await db.SaveChangesAsync(); return factory;
    }
    public HttpClient Client(int id, string role = "Candidate")
    {
        var client = CreateClient(); client.DefaultRequestHeaders.Add("X-Test-User", id.ToString()); client.DefaultRequestHeaders.Add("X-Test-Role", role); return client;
    }
    public async Task<int> SeedJobAsync()
    {
        await using var scope=Services.CreateAsyncScope(); var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        var recruiter=new User { Email="recruiter"+Guid.NewGuid()+"@test.local", FirstName="R", LastName="User", PasswordHash="x", Role=JobOrbit.Domain.Enums.UserRole.Recruiter };
        var organization=new Organization { Name="Test Organization "+Guid.NewGuid(), Location="Colombo" };
        var department=new Department { Name="Engineering", Organization=organization };
        var profile=new RecruiterProfile { User=recruiter, Organization=organization, JobTitle="Recruiter" };
        var job=new JobPosting { Title="Active Job", Description="Description", Location="Colombo", EmploymentType="Full-time", Status=JobOrbit.Domain.Enums.JobStatus.Published, PublishedAt=DateTime.UtcNow, ClosingAt=DateTime.UtcNow.AddDays(10), Organization=organization, Department=department, RecruiterProfile=profile };
        db.JobPostings.Add(job); await db.SaveChangesAsync(); return job.Id;
    }
    public async Task AddApplicationAsync(int candidateUserId, int jobId, int resumeId)
    {
        await using var scope=Services.CreateAsyncScope(); var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        var candidateId=await db.CandidateProfiles.Where(x=>x.UserId==candidateUserId).Select(x=>x.Id).SingleAsync();
        db.JobApplications.Add(new JobApplication { CandidateProfileId=candidateId, JobPostingId=jobId, ResumeId=resumeId, AppliedAt=DateTime.UtcNow });
        await db.SaveChangesAsync();
    }
    public async Task<(int ApplicationId,int JobId)> AttachResumeToOwnedApplicationAsync(int resumeId)
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var application=await db.JobApplications.SingleAsync(x=>x.JobPosting.RecruiterProfile.UserId==3);application.ResumeId=resumeId;await db.SaveChangesAsync();return(application.Id,application.JobPostingId);
    }
    public async Task<int> PrepareShortlistedApplicationAsync()
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var application=await db.JobApplications.Include(x=>x.Interviews).SingleAsync(x=>x.JobPosting.RecruiterProfile.UserId==3);db.Interviews.RemoveRange(application.Interviews);application.Status=JobOrbit.Domain.Enums.ApplicationStatus.Shortlisted;await db.SaveChangesAsync();return application.Id;
    }
    public async Task MakeInterviewDueAsync(int interviewId)
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var interview=await db.Interviews.FindAsync(interviewId);interview!.ScheduledAt=DateTime.UtcNow.AddMinutes(-5);await db.SaveChangesAsync();
    }
    public async Task SeedRecruiterDashboardAsync()
    {
        await using var scope=Services.CreateAsyncScope(); var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();
        var org=new Organization { Name="Recruiter Stats Org" }; var department=new Department { Name="Engineering",Organization=org };
        var first=new User { Id=3,Email="stats-one@test.local",FirstName="Stats",LastName="One",Role=JobOrbit.Domain.Enums.UserRole.Recruiter };
        var second=new User { Id=4,Email="stats-two@test.local",FirstName="Stats",LastName="Two",PasswordHash="x",Role=JobOrbit.Domain.Enums.UserRole.Recruiter };
        var empty=new User { Id=5,Email="stats-empty@test.local",FirstName="Stats",LastName="Empty",PasswordHash="x",Role=JobOrbit.Domain.Enums.UserRole.Recruiter };
        var manager=new User { Id=6,Email="manager@test.local",FirstName="Test",LastName="Manager",PasswordHash="x",Role=JobOrbit.Domain.Enums.UserRole.HiringManager };var admin=new User{Id=8,Email="admin@test.local",FirstName="Test",LastName="Admin",Role=JobOrbit.Domain.Enums.UserRole.Administrator};admin.PasswordHash=new PasswordHasher<User>().HashPassword(admin,"CurrentAdmin123");
        first.PasswordHash=new PasswordHasher<User>().HashPassword(first,"CurrentRecruiter123");var firstProfile=new RecruiterProfile { User=first,Organization=org,JobTitle="Recruiter" }; var secondProfile=new RecruiterProfile { User=second,Organization=org,JobTitle="Recruiter" };
        var emptyProfile=new RecruiterProfile { User=empty,Organization=org,JobTitle="Recruiter" };
        var owned=new JobPosting { Title="Owned Job",Description="Owned",Location="Colombo",EmploymentType="Full-time",Organization=org,Department=department,RecruiterProfile=firstProfile,Status=JobOrbit.Domain.Enums.JobStatus.Published };
        var unrelated=new JobPosting { Title="Other Job",Description="Other",Location="Kandy",EmploymentType="Full-time",Organization=org,Department=department,RecruiterProfile=secondProfile,Status=JobOrbit.Domain.Enums.JobStatus.Published };
        var application=new JobApplication { CandidateProfileId=1,JobPosting=owned,AppliedAt=DateTime.UtcNow };
        application.Interviews.Add(new Interview { ScheduledAt=DateTime.UtcNow.AddHours(2),DurationMinutes=30,Location="Online" });
        application.Interviews.Add(new Interview { ScheduledAt=DateTime.UtcNow.AddHours(-2),DurationMinutes=30 });
        application.Interviews.Add(new Interview { ScheduledAt=DateTime.UtcNow.AddHours(3),DurationMinutes=30,Status=JobOrbit.Domain.Enums.InterviewStatus.Cancelled });
        db.AddRange(org,department,first,second,empty,manager,admin,firstProfile,secondProfile,emptyProfile,owned,unrelated,application); await db.SaveChangesAsync();
        foreach(var definition in JobOrbit.Application.Authorization.PermissionConstants.All)
        {
            var permission=new Permission{Code=definition.Code,DisplayName=definition.DisplayName,Description=definition.Description,Category=definition.Category};
            db.Permissions.Add(permission); await db.SaveChangesAsync();
            if(definition.Role==JobOrbit.Domain.Enums.UserRole.Administrator)
                db.RolePermissions.Add(new RolePermission{Role=definition.Role,PermissionId=permission.Id});
        }
        await db.SaveChangesAsync();
    }
    public async Task<(int OrganizationId,int DepartmentId)> GetOrganizationDepartmentIdsAsync(){await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var d=await db.Departments.FirstAsync();return(d.OrganizationId,d.Id);}
    public async Task<int> SeedHiringManagerEvaluationScopeAsync()
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var manager=await db.Users.SingleAsync(x=>x.Id==6);manager.PasswordHash=new PasswordHasher<User>().HashPassword(manager,"CurrentManager123");var application=await db.JobApplications.Include(x=>x.JobPosting).SingleAsync(x=>x.JobPosting.RecruiterProfile.UserId==3);application.Status=JobOrbit.Domain.Enums.ApplicationStatus.Shortlisted;db.HiringManagerProfiles.Add(new HiringManagerProfile{UserId=manager.Id,OrganizationId=application.JobPosting.OrganizationId,DepartmentId=application.JobPosting.DepartmentId});var other=new User{Id=7,Email="other-manager@test.local",FirstName="Other",LastName="Manager",PasswordHash="x",Role=JobOrbit.Domain.Enums.UserRole.HiringManager};db.Users.Add(other);await db.SaveChangesAsync();db.HiringManagerProfiles.Add(new HiringManagerProfile{UserId=other.Id,OrganizationId=application.JobPosting.OrganizationId,DepartmentId=application.JobPosting.DepartmentId});await db.SaveChangesAsync();return application.Id;
    }
    public async Task<int> AddSecondManagerReviewApplicationAsync()
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var job=await db.JobPostings.SingleAsync(x=>x.RecruiterProfile.UserId==3);var application=new JobApplication{CandidateProfileId=2,JobPostingId=job.Id,AppliedAt=DateTime.UtcNow.AddMinutes(1),Status=JobOrbit.Domain.Enums.ApplicationStatus.Shortlisted};db.JobApplications.Add(application);await db.SaveChangesAsync();return application.Id;
    }
    public async Task<int> AddForeignDepartmentAsync()
    {
        await using var scope=Services.CreateAsyncScope();var db=scope.ServiceProvider.GetRequiredService<JobOrbitDbContext>();var department=new Department{Name="Foreign Department",Organization=new Organization{Name="Foreign Organization"}};db.Departments.Add(department);await db.SaveChangesAsync();return department.Id;
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "test-only-signing-key-with-at-least-32-characters",
            ["Jwt:Issuer"] = "JobOrbit.Tests",
            ["Jwt:Audience"] = "JobOrbit.Tests",
            ["Jwt:ExpiryMinutes"] = "60",
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=JobOrbitTests;Trusted_Connection=True"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<JobOrbitDbContext>>(); services.RemoveAll<JobOrbitDbContext>();
            services.RemoveAll<IDbContextOptionsConfiguration<JobOrbitDbContext>>();
            services.AddDbContext<JobOrbitDbContext>(x => x.UseInMemoryDatabase(dbName));
            services.RemoveAll<IFileStorageService>(); services.AddSingleton<IFileStorageService, MemoryStorage>();
            services.AddAuthentication(x => { x.DefaultAuthenticateScheme="Test"; x.DefaultChallengeScheme="Test"; x.DefaultForbidScheme="Test"; }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", null);
        });
    }
}

internal sealed class MemoryStorage : IFileStorageService
{
    private readonly Dictionary<string, byte[]> files = [];
    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken token=default) { var name=Guid.NewGuid()+extension; using var ms=new MemoryStream(); await content.CopyToAsync(ms,token); files[name]=ms.ToArray(); return name; }
    public Task<Stream?> OpenReadAsync(string name, CancellationToken token=default) => Task.FromResult<Stream?>(files.TryGetValue(name,out var data) ? new MemoryStream(data) : null);
    public Task DeleteAsync(string name, CancellationToken token=default) { files.Remove(name); return Task.CompletedTask; }
}

internal sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Test-User", out var id)) return Task.FromResult(AuthenticateResult.NoResult());
        var role=Request.Headers["X-Test-Role"].FirstOrDefault() ?? "Candidate";
        var identity=new ClaimsIdentity([new Claim("UserId",id!),new Claim("Role",role)],Scheme.Name,"FullName","Role");
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity),Scheme.Name)));
    }
}
