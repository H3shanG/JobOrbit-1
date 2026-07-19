using System.Text;
using JobOrbit.API;
using JobOrbit.API.Authorization;
using JobOrbit.API.Services;
using JobOrbit.API.Middleware;
using JobOrbit.Application.Authorization;
using JobOrbit.Application.Configuration;
using JobOrbit.Application.Interfaces;
using JobOrbit.Application.Services;
using JobOrbit.Domain.Entities;
using JobOrbit.Infrastructure.Persistence;
using JobOrbit.Infrastructure.Persistence.Repositories;
using JobOrbit.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string frontendCorsPolicy = "JobOrbitFrontend";

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ----------------------------------------------------
// Configuration
// ----------------------------------------------------

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

var jwtSection = builder.Configuration.GetRequiredSection(JwtSettings.SectionName);
var jwtSettings = jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings were not found.");

// ----------------------------------------------------
// Controllers
// ----------------------------------------------------

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services
    .AddOptions<JwtSettings>()
    .Bind(jwtSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// ----------------------------------------------------
// Database
// ----------------------------------------------------

builder.Services.AddDbContext<JobOrbitDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddMemoryCache();

// ----------------------------------------------------
// Application services
// ----------------------------------------------------

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ICandidateDashboardService, CandidateDashboardService>();
builder.Services.AddScoped<ICandidateDashboardRepository, CandidateDashboardRepository>();
builder.Services.AddScoped<IRecruiterDashboardService, RecruiterDashboardService>();
builder.Services.AddScoped<IRecruiterDashboardRepository, RecruiterDashboardRepository>();
builder.Services.AddScoped<IRecruiterJobService, RecruiterJobService>();
builder.Services.AddScoped<IRecruiterJobRepository, RecruiterJobRepository>();
builder.Services.AddScoped<IRecruiterApplicationService, RecruiterApplicationService>();
builder.Services.AddScoped<IRecruiterApplicationRepository, RecruiterApplicationRepository>();
builder.Services.AddScoped<IRecruiterInterviewService, RecruiterInterviewService>();
builder.Services.AddScoped<IRecruiterInterviewRepository, RecruiterInterviewRepository>();
builder.Services.AddScoped<IRecruiterAnalyticsService, RecruiterAnalyticsService>();
builder.Services.AddScoped<IRecruiterAnalyticsRepository, RecruiterAnalyticsRepository>();
builder.Services.AddScoped<IRecruiterSettingsService, RecruiterSettingsService>();
builder.Services.AddScoped<IRecruiterSettingsRepository, RecruiterSettingsRepository>();
builder.Services.AddScoped<IHiringManagerDashboardService, HiringManagerDashboardService>();
builder.Services.AddScoped<IHiringManagerDashboardRepository, HiringManagerDashboardRepository>();
builder.Services.AddScoped<IHiringManagerCandidateService, HiringManagerCandidateService>();
builder.Services.AddScoped<IHiringManagerCandidateRepository, HiringManagerCandidateRepository>();
builder.Services.AddScoped<IHiringManagerEvaluationService, HiringManagerEvaluationService>();
builder.Services.AddScoped<IHiringManagerEvaluationRepository, HiringManagerEvaluationRepository>();
builder.Services.AddScoped<IHiringDecisionService, HiringDecisionService>();
builder.Services.AddScoped<IHiringDecisionRepository, HiringDecisionRepository>();
builder.Services.AddScoped<IHiringManagerReportService, HiringManagerReportService>();
builder.Services.AddScoped<IHiringManagerReportRepository, HiringManagerReportRepository>();
builder.Services.AddScoped<IHiringManagerSettingsService, HiringManagerSettingsService>();
builder.Services.AddScoped<IHiringManagerSettingsRepository, HiringManagerSettingsRepository>();
builder.Services.AddScoped<IHiringManagerInterviewService, HiringManagerInterviewService>();
builder.Services.AddScoped<IHiringManagerInterviewRepository, HiringManagerInterviewRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<IAdminRoleService, AdminRoleService>();
builder.Services.AddScoped<IAdminRoleRepository, AdminRoleRepository>();
builder.Services.AddScoped<IAdminOrganizationService, AdminOrganizationService>();
builder.Services.AddScoped<IAdminOrganizationRepository, AdminOrganizationRepository>();
builder.Services.AddScoped<IAdminDepartmentService, AdminDepartmentService>();
builder.Services.AddScoped<IAdminDepartmentRepository, AdminDepartmentRepository>();
builder.Services.AddScoped<IAdminJobService, AdminJobService>();
builder.Services.AddScoped<IAdminJobRepository, AdminJobRepository>();
builder.Services.AddScoped<IAdminApplicationService, AdminApplicationService>();
builder.Services.AddScoped<IAdminApplicationRepository, AdminApplicationRepository>();
builder.Services.AddScoped<IAdminAuditLogService, AdminAuditLogService>();
builder.Services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();
builder.Services.AddScoped<IAdminSystemSettingsService, AdminSystemSettingsService>();
builder.Services.AddScoped<ISystemSettingsProvider, SystemSettingsProvider>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserPermissionService, CurrentUserPermissionService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IJobMatchingService, JobMatchingService>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IJobApplicationService, JobApplicationService>();
builder.Services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
builder.Services.AddScoped<ICandidateApplicationService, CandidateApplicationService>();
builder.Services.AddScoped<ICandidateApplicationRepository, CandidateApplicationRepository>();
builder.Services.AddScoped<ICandidateProfileService, CandidateProfileService>();
builder.Services.AddScoped<ICandidateProfileRepository, CandidateProfileRepository>();
builder.Services.AddScoped<ICandidateResumeService, CandidateResumeService>();
builder.Services.AddScoped<ICandidateResumeRepository, CandidateResumeRepository>();
builder.Services.AddScoped<ICandidateSettingsService, CandidateSettingsService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddSingleton<IResumeFileValidator, ResumeFileValidator>();
builder.Services.AddScoped<DevelopmentDataSeeder>();

// ----------------------------------------------------
// JWT authentication
// ----------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Key)),

                NameClaimType = "FullName",
                RoleClaimType = "Role",

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in PermissionConstants.All)
        options.AddPolicy(permission.Code, policy => policy.Requirements.Add(new PermissionRequirement(permission.Code)));
});

// ----------------------------------------------------
// CORS
// ----------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ----------------------------------------------------
// Swagger
// ----------------------------------------------------

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JobOrbit API",
        Version = "v1",
        Description =
            "API foundation for the JobOrbit recruitment platform."
    });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description =
                "Paste only the JWT token. Do not type the word Bearer."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document)
            ] = []
        });
});

var app = builder.Build();

await using (var permissionScope = app.Services.CreateAsyncScope())
{
    await permissionScope.ServiceProvider.GetRequiredService<DevelopmentDataSeeder>().SeedPermissionsAsync();
}

if (app.Environment.IsDevelopment())
{
    await using var seedScope = app.Services.CreateAsyncScope();
    await seedScope.ServiceProvider
        .GetRequiredService<DevelopmentDataSeeder>()
        .SeedAsync();
}

await using (var settingsScope = app.Services.CreateAsyncScope())
{
    await settingsScope.ServiceProvider.GetRequiredService<ISystemSettingsProvider>().SeedDefaultsAsync();
}

app.UseExceptionHandler();

// ----------------------------------------------------
// HTTP pipeline
// ----------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "JobOrbit API v1");

        options.DocumentTitle = "JobOrbit API";
    });
}

app.UseHttpsRedirection();

app.UseCors(frontendCorsPolicy);

// Authentication must appear before authorization.
app.UseAuthentication();
app.UseMiddleware<MaintenanceModeMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program;
