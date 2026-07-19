# JobOrbit

JobOrbit is the initial technical foundation for an AI-powered recruitment platform. This repository currently contains infrastructure and presentation scaffolding only; business features have intentionally not been implemented.

## Structure

```text
src/
  JobOrbit.API             ASP.NET Core controller API and composition root
  JobOrbit.Application     Application-layer contracts and use cases
  JobOrbit.Domain          Core domain model
  JobOrbit.Infrastructure  EF Core, SQL Server, and external infrastructure
tests/
  JobOrbit.Tests           xUnit backend tests
frontend/                  React and Vite client
```

Dependencies point inward: Domain has no project dependencies, Application references Domain, Infrastructure references Application and Domain, and API composes Application and Infrastructure.

## Run locally

Trust the ASP.NET Core development certificate once if needed:

```powershell
dotnet dev-certs https --trust
```

Start the API:

```powershell
dotnet run --project src/JobOrbit.API --launch-profile https
```

Swagger is available at `https://localhost:7075/swagger`, and the health endpoint is `GET https://localhost:7075/api/health`.

Start the web client in a second terminal:

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

The client runs at `http://localhost:5173`. Set `VITE_API_URL` in `frontend/.env.local` if the API uses a different address.

## Configuration

- `ConnectionStrings:DefaultConnection` configures SQL Server.
- `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpiryMinutes` configure issued tokens.
- `Jwt:Key` is development-only in `appsettings.Development.json`. Supply `Jwt__Key` from a secret store or environment variable outside development.
- CORS currently permits `http://localhost:5173`.

## Authentication API

- `POST /api/auth/register` creates Candidate accounts only.
- `POST /api/auth/login` validates credentials and returns a bearer token.
- `GET /api/auth/me` requires a bearer token and returns the current user.

Public requests cannot select a role. Recruiter, Hiring Manager, and Administrator accounts must be provisioned through a future restricted workflow.

The initial database migration is stored under `src/JobOrbit.Infrastructure/Persistence/Migrations`. Restore the repository-local EF tool and apply it with:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/JobOrbit.Infrastructure --startup-project src/JobOrbit.API
```

## Verify

```powershell
dotnet test JobOrbit.sln
cd frontend
npm.cmd run lint
npm.cmd run build
```
