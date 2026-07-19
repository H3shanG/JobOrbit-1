JobOrbit🌐

JobOrbit is a full-stack recruitment management platform developed as a university software engineering project. It connects candidates, recruiters, hiring managers, and system administrators through a centralized role-based system.

The platform supports the complete recruitment process, from job posting and application submission to interview scheduling, candidate evaluation, hiring decisions, and administrative monitoring.

Project Overview

JobOrbit was designed to solve common recruitment challenges such as:

Manual job application handling
Poor communication between recruitment stakeholders
Difficulty tracking candidate progress
Disconnected interview and evaluation processes
Limited visibility for system administrators
Inefficient candidate-to-job matching

The system provides separate dashboards and features for each user role while maintaining secure access through JWT authentication and role-based authorization.

Main User Roles
Candidate

Candidates can:

Register and log in
Manage their profile
Upload and manage a resume
Browse available job postings
View job details
Apply for jobs
Track submitted applications
View application status updates
View scheduled interview information
View final hiring results
Update account settings
Recruiter

Recruiters can:

Log in through the shared authentication page
View recruitment dashboard statistics
Create and publish job postings
Edit and manage owned jobs
View applicants for their jobs
Search and filter applications
Change application statuses
Shortlist or reject candidates
Schedule, reschedule, cancel, and complete interviews
View ranked applicants and match information
View recruitment analytics
Manage profile and notification preferences
Hiring Manager

Hiring managers can:

View candidates requiring review
Access candidate and application details
View scheduled interviews
Review candidate profiles and resumes
Submit candidate evaluations
Score technical skill, communication, experience, and culture fit
Provide hiring recommendations
Make final hiring decisions
Place candidates on hold
View recruitment reports
Manage profile and security settings

Evaluation and hiring-decision actions are accessed contextually through the candidate review workflow.

Administrator

Administrators can:

View system-wide dashboard statistics
Manage user accounts
Create Candidate, Recruiter, Hiring Manager, and Admin users
Activate or deactivate users
Reset user passwords
Manage roles and permissions
Manage organizations
Manage departments
Supervise job postings
Review applications across the platform
Perform controlled application-status corrections
View immutable audit logs
Manage system settings
Monitor platform health and activity
Key Features
JWT authentication
Role-based authorization
Protected frontend routes
Candidate profile management
Resume upload and download
Job posting management
Job application workflow
Interview scheduling
Candidate evaluation and scoring
Hiring decisions
Organization and department management
User and permission administration
Audit logging
System settings
Candidate-to-job matching
Recruiter applicant ranking
Responsive dashboard interfaces
Swagger API documentation
SQL Server database integration
Candidate Matching

JobOrbit includes an explainable candidate-matching feature.

The matching engine compares candidate and job information using factors such as:

Skills
Experience
Education
Professional-title relevance
Location compatibility
Workplace preference
Employment-type compatibility

The engine generates:

Match score
Confidence score
Matched skills
Missing skills
Strengths
Gaps
Score breakdown

The score is used only as decision support. It does not automatically shortlist, reject, evaluate, or hire a candidate.

Protected personal attributes such as age, gender, religion, ethnicity, and marital status are not used in the matching process.

Technology Stack
Backend
C#
ASP.NET Core Web API
Entity Framework Core
SQL Server
JWT Bearer Authentication
ASP.NET Core Authorization
Swagger / OpenAPI
xUnit or the existing project testing framework
Frontend
React
JavaScript or TypeScript
React Router
Axios
Vite
Responsive CSS
Shared dashboard components
Existing chart library
Database
Microsoft SQL Server
Entity Framework Core migrations
Project Architecture

The backend follows a layered architecture.

JobOrbit.Domain
JobOrbit.Application
JobOrbit.Infrastructure
JobOrbit.API
JobOrbit.Tests
Domain Layer

Contains:

Entities
Enums
Core business models
Domain rules
Application Layer

Contains:

DTOs
Service interfaces
Business services
Validators
Application logic
Infrastructure Layer

Contains:

Entity Framework Core
Database context
Repositories
Entity configurations
Data persistence
Development seeding
API Layer

Contains:

Controllers
Authentication configuration
Authorization policies
Dependency injection
Swagger configuration
Middleware
Frontend

Contains:

Pages
Role-based layouts
Shared components
API services
Authentication context
Protected routes
Forms
Tables
Charts
Responsive styling
Core Recruitment Workflow
Candidate registers
        ↓
Candidate completes profile
        ↓
Candidate uploads resume
        ↓
Candidate browses jobs
        ↓
Candidate submits application
        ↓
Recruiter reviews application
        ↓
Recruiter shortlists candidate
        ↓
Recruiter schedules interview
        ↓
Hiring Manager reviews candidate
        ↓
Hiring Manager submits evaluation
        ↓
Hiring Manager makes hiring decision
        ↓
Candidate views final status
Authentication and Authorization

JobOrbit uses JWT authentication.

After login, the API returns a JWT containing user and role information. The frontend uses this token when making protected API requests.

The system supports these roles:

Candidate
Recruiter
HiringManager
Admin

The backend remains the source of truth for permissions and ownership checks.

Users cannot access another role’s protected resources.

Examples:

A Candidate cannot access Recruiter endpoints
A Recruiter cannot manage another Recruiter’s job
A Hiring Manager cannot review out-of-scope applications
A non-Admin user cannot access Admin endpoints
Development Accounts

Development seed accounts may include:

Recruiter
Email: recruiter@joborbit.test
Password: Recruiter@123
Hiring Manager
Email: manager@joborbit.test
Password: Manager@123
Administrator
Email: admin@joborbit.test
Password: Admin@123

Candidate credentials depend on accounts created through registration or development seeding.

These credentials are intended only for local development and demonstrations. They should be removed or replaced before production deployment.

Prerequisites

Install the following:

.NET SDK
Node.js and npm
SQL Server or SQL Server LocalDB
SQL Server Management Studio
Git
Visual Studio, Visual Studio Code, or another compatible IDE
Running the Backend

From the project root:

dotnet restore
dotnet build
dotnet run --project src/JobOrbit.API

The API normally runs at:

http://localhost:5181

Swagger is available at:

http://localhost:5181/swagger

The exact port may depend on the launch settings.

Running the Frontend

Open another terminal:

cd frontend
npm install
npm run dev

Vite normally starts at:

http://localhost:5173

It may use another port such as 5174 when the default port is already occupied. Open the exact URL printed in the terminal.

Database Setup

Confirm that the SQL Server connection string is correctly configured in:

appsettings.json

or:

appsettings.Development.json

Apply migrations:

dotnet ef database update --project src/JobOrbit.Infrastructure --startup-project src/JobOrbit.API

The exact project paths may differ slightly depending on the final solution structure.

Do not store production database passwords directly in source-controlled configuration files.

Build and Test

Backend:

dotnet clean
dotnet build
dotnet test

Frontend:

cd frontend
npm run build

The frontend production build may show a Vite bundle-size advisory. This is an optimization notice unless the build fails.

API Documentation

Swagger provides interactive API documentation.

Open:

http://localhost:5181/swagger

To test protected endpoints:

Log in using the authentication endpoint.
Copy the returned JWT.
Click Authorize in Swagger.
Enter:
Bearer YOUR_TOKEN
Execute the required endpoint.
Important API Areas

The project includes API groups similar to:

/api/auth
/api/candidate
/api/recruiter
/api/manager
/api/admin
/api/dashboard
/api/notifications

Exact endpoint names should be confirmed through Swagger.

Security Features

JobOrbit includes:

Password hashing
JWT authentication
Role-based access
Permission checks
Resource ownership validation
Protected resume access
Input validation
Controlled status transitions
Controlled permission values
Audit logging
Account activation and deactivation
Prevention of self-deactivation for administrators
Protection against deactivating the last active Admin
Safe handling of application and hiring workflow conflicts

The system does not return:

Password hashes
Access tokens in profile responses
Database credentials
Physical resume paths
Connection strings
Internal security secrets
Audit Logs

Audit logs record important actions such as:

Login activity
User creation and updates
Account activation and deactivation
Password resets
Permission changes
Organization changes
Department changes
Job status changes
Application corrections
Interview activity
Candidate evaluations
Hiring decisions
System-settings changes

Audit logs are read-only and cannot be edited or deleted through the normal Admin interface.

System Settings

Administrators can manage controlled platform settings such as:

Platform name
Support email
Default timezone
Default currency
Candidate registration
Duplicate-application rules
Profile-completion requirements
Default job-closing duration
Evaluation requirements
Interview requirements
Resume upload restrictions
Password policy
Notification controls
Maintenance mode

Sensitive values such as JWT keys and database credentials are not exposed through System Settings.

User Interface

The interface uses a modern charcoal, lime, soft-gray, and white color palette.

Main colors include:

Charcoal: #1F2937
Dark Charcoal: #111827
Lime: #A3E635
Lime Hover: #84CC16
Soft Gray: #F3F4F6
Border Gray: #E5E7EB
White: #FFFFFF

Lime is used mainly as an accent for:

Active navigation
Focus indicators
Selected controls
Chart highlights
Important non-destructive actions

Semantic colors remain separate:

Green for success and hired
Red for errors, rejected, and destructive actions
Amber for pending and hold
Blue only where intentionally retained for informational or interview states
Current Limitations
No external email or SMS provider
No real-time SignalR notification delivery unless added later
Candidate matching is deterministic and explainable, not a trained machine-learning model
No external AI provider
No payment system
No advanced production infrastructure monitoring
CSV and PDF reporting may not be implemented
Development credentials must not be used in production
Production deployment configuration requires separate setup
Future Improvements

Possible future improvements include:

Real-time notifications with SignalR
Email notifications
Calendar integration
Video-interview integration
Advanced resume parsing
Machine-learning ranking models
Multi-factor authentication
Refresh-token rotation
Cloud file storage
Advanced reporting and exports
Docker support
CI/CD pipelines
Production monitoring
Localization
Accessibility audits
Automated end-to-end testing
Suggested Demonstration Flow

A complete demonstration can follow this sequence:

1. Candidate logs in
2. Candidate updates profile and uploads resume
3. Candidate applies for a job
4. Recruiter logs in
5. Recruiter reviews and shortlists the candidate
6. Recruiter schedules an interview
7. Hiring Manager logs in
8. Hiring Manager reviews and evaluates the candidate
9. Hiring Manager makes a hiring decision
10. Candidate views the updated application status
11. Admin views users, applications, and audit logs
Project Status

The core JobOrbit recruitment platform is functionally complete.

The current focus should be on:

Final bug fixes
Complete role-based workflow testing
Responsive layout verification
Security validation
Code cleanup
Deployment preparation
License

This project was created for educational and university coursework purposes.


Software Engineering Undergraduate
C# Full-Stack Development Project
