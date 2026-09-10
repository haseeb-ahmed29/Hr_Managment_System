# Northstar HR & Lab Office Management System

Northstar is a production-oriented **Human Resources and Lab Office Management System** built with ASP.NET Core and Clean Architecture. It provides one workspace for employee records, departments, leave workflows, financial requests, resignations, employee data changes, recruitment, CV management, notifications, background jobs and operational reporting.

The project now includes a polished responsive management dashboard served by the API. It is designed for office administrators and HR teams who need a clear overview of people, requests, teams and hiring activity while retaining historical records across previous years.

> **Local-first notice:** GitHub stores this source code but does not run the application or database. The included Docker Compose setup provides a complete local environment. For background jobs to continue while a computer is switched off, the same container must later be deployed to an always-on server or hosting provider.

## Product Highlights

| Area | Included capability |
|---|---|
| Management dashboard | Overview cards, people distribution, attention queue, recent activity, responsive navigation and operational status panels |
| Employee records | Paginated employee directory, search, department filtering, role visibility and active/inactive status |
| Organization | Department management, managers, hierarchy support and tenant-safe relationships |
| Request workflows | Leave, financial, resignation and employee data-change requests with status transitions and history |
| Historical data | SQL Server persistence, date/year filters and request history endpoints designed to preserve prior periods |
| Recruitment | Job positions, applications, CV uploads and candidate match-score workflow |
| Security | JWT authentication, role-based authorization, tenant isolation, rate limiting and environment-based secrets |
| Background processing | Quartz leave accrual and optional Hangfire pending-request reminders |
| Operations | Serilog logging, health checks, OpenAPI development documentation and Docker support |

## Dashboard

The dashboard is available at the application root (`/`). It includes a Northstar management theme with a dark command sidebar, clear KPI cards, responsive layouts and office-oriented workflows.

The main sections are:

- **Overview:** total employees, open requests, departments, open positions, department distribution, priority queue and recent activity.
- **Employees:** employee search, department/year filters, role and status visibility, and export-ready table layout.
- **Departments:** team cards, employee counts, managers and organization visibility.
- **Requests:** approval queue with request search, status, type and year filters.
- **Recruitment:** open-position cards and application pipeline visibility.
- **Reports:** historical headcount, request approvals and recruitment funnel entry points.
- **Settings:** database persistence, background jobs and security status indicators.

The interface uses the existing API contracts and keeps the data layer separate from presentation. Login uses the existing `/api/auth/login` endpoint and stores only the JWT session token in the browser.

## Architecture

The solution follows a Clean Architecture / Onion Architecture structure:

```text
HrManagmentSystem_API              Presentation, controllers, middleware, dashboard
HrManagementSystem_Application     Services, use cases, DTO mapping, jobs and interfaces
HrManagementSystem_Domain           Entities, base classes and domain rules
HrManagementSystem_Infrastructure   EF Core DbContext, repositories, migrations and storage
HrManagementSystem_Dto              Request and response DTOs
HrManagementSystem_Shared           Enums and localization resources
Hr_ManagementSystem_Test            Unit tests for application services
```

The system supports multi-tenancy through tenant-aware entities, current-tenant resolution and EF Core query filters. This prevents records from one tenant being accidentally exposed to another tenant.

## Technology Stack

- .NET 9 and ASP.NET Core Web API
- Entity Framework Core 9
- SQL Server
- JWT Bearer authentication and role-based authorization
- AutoMapper
- Quartz.NET and optional Hangfire
- Serilog
- OpenAPI development documentation
- xUnit, Moq and coverlet-compatible test project
- Docker and Docker Compose
- Vanilla HTML, CSS and JavaScript dashboard served from `wwwroot`

## Roles

The application contains role-based access for the following operational roles:

- `SystemAdmin`
- `HrAdmin`
- `Manager`
- `Employee`
- `Recruiter`

Controllers enforce permissions for employee management, request approvals, recruitment operations, tenant administration and CV ranking.

## Run Locally with .NET and SQL Server

### Prerequisites

Install the .NET 9 SDK and SQL Server. Then configure the connection string in `HrManagmentSystem_API/appsettings.json` or through environment variables. Keep JWT, SMTP and OpenAI secrets outside source control.

### Restore and migrate

```bash
dotnet restore Hr_Management_System.sln
dotnet ef database update \
  --project HrManagementSystem_Infrastructure \
  --startup-project HrManagmentSystem_API
```

### Start the API

```bash
dotnet run --project HrManagmentSystem_API
```

Open the URL printed by ASP.NET Core. The management dashboard is available at `/`. OpenAPI documentation is enabled in Development mode.

## Run Everything with Docker Compose

Docker Compose starts SQL Server and the API together, persists SQL Server data in a named volume, and mounts runtime uploads and logs back into the repository.

```bash
docker compose up --build
```

Then open:

```text
http://localhost:8080
```

The local SQL Server credentials are intentionally development-only values in `docker-compose.yml`. Change them before using this configuration outside a private development machine. The API container receives its database connection string and JWT key through environment variables.

To stop the environment while retaining database data:

```bash
docker compose down
```

To remove the local database volume as well:

```bash
docker compose down -v
```

## Database and Historical Records

SQL Server is the source of truth for office records. Existing EF Core migrations are stored under `HrManagementSystem_Infrastructure/Migrations`. Historical records are not removed when a new year is selected; year selectors in the dashboard are intended for period filtering and reporting views.

For production, use scheduled SQL Server backups and store connection strings through a deployment secret manager. Candidate CV files and logs are intentionally excluded from Git tracking through `.gitignore`.

## Background Jobs

Quartz is configured for scheduled leave accrual. Hangfire is available for pending-request reminder processing and its dashboard. Hangfire is disabled by default in the plain local appsettings file so the dashboard shell can start without a running SQL Server; Docker Compose explicitly enables it because the SQL Server service is available there.

The jobs run while the API process is running. Closing the browser does not stop them. Switching off the computer does stop them unless the application is deployed to an always-on server or managed hosting provider.

## Configuration and Secrets

Use environment variables or .NET User Secrets for sensitive values. Examples include:

```bash
dotnet user-secrets init --project HrManagmentSystem_API
dotnet user-secrets set "JwtSettings:Key" "replace-with-a-long-random-key" --project HrManagmentSystem_API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "replace-with-private-connection-string" --project HrManagmentSystem_API
dotnet user-secrets set "Smtp:Password" "replace-with-smtp-password" --project HrManagmentSystem_API
```

Never commit real JWT keys, SMTP passwords, OpenAI keys, production connection strings, candidate CVs or application logs.

## API Modules

The API contains controllers for authentication, employees, departments, tenants, leave requests, financial requests, resignation requests, employee data changes, generic request history, job positions, applications, CV documents and AI CV ranking.

Useful development routes include:

```text
GET  /health
POST /api/auth/login
GET  /api/employees
GET  /api/departments
GET  /api/requests/for-approval
GET  /api/requests/{requestId}/history
GET  /api/jobposition
```

Protected endpoints require a valid bearer token and the appropriate role.

## Validation Performed

The current project has been checked with:

```bash
dotnet build Hr_Management_System.sln --configuration Release
dotnet test Hr_Management_System.sln --configuration Release --no-build
node --check HrManagmentSystem_API/wwwroot/js/dashboard.js
```

The solution builds successfully, all existing unit tests pass, the dashboard JavaScript parses successfully, and the root dashboard smoke test returns the expected HTML. A health status of `503` without SQL Server is expected because the database health check correctly reports that the local database is unavailable.

## Repository Structure

```text
.
├── Dockerfile
├── docker-compose.yml
├── Hr_Management_System.sln
├── HrManagmentSystem_API
│   ├── Controllers
│   ├── Extension Method
│   ├── Middleware
│   ├── wwwroot
│   │   ├── css/dashboard.css
│   │   ├── js/dashboard.js
│   │   └── index.html
│   ├── appsettings.json
│   └── Program.cs
├── HrManagementSystem_Application
├── HrManagementSystem_Domain
├── HrManagementSystem_Dto
├── HrManagementSystem_Infrastructure
├── HrManagementSystem_Shared
└── Hr_ManagementSystem_Test
```

## License and Ownership

This repository contains the Northstar HR and Lab Office Management System source code. Add your preferred license and organization policy before distributing it outside your team.

<!-- github-daily-pipeline -->
## Daily maintenance

README verified by the daily repository maintenance pipeline on 2026-09-10.
