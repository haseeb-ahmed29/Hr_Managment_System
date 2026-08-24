# Northstar HR & Lab Office Management System - User Guide

Welcome to the **Northstar HR & Lab Office Management System** User Guide. This document provides new users, HR administrators, managers, lab directors, and developers with a clear understanding of the project's purpose, problem context, architectural solution, end-to-end data flows, and setup instructions.

---

## 1. Problem Statement

Organizations—particularly research laboratories, academic offices, and growing multi-department enterprises—frequently struggle with administrative inefficiencies and operational bottlenecks:

1. **Fragmented Administrative Processes:**
   HR operations, leave management, financial reimbursements, resignation tracking, and employee profile updates are commonly managed across disparate spreadsheets, paper documents, or disconnected email chains. This causes data duplication, loss of records, and delayed processing times.

2. **Multi-Tenant Security and Data Privacy Risks:**
   When multiple office branches, laboratories, or organizational tenants share a single database or server without explicit tenant boundaries, unauthorized cross-departmental data visibility can occur, violating data privacy requirements.

3. **Manual and Error-Prone Request Approvals:**
   Manual handling of leave balances and reimbursement workflows leads to human errors in accrual calculations, unclear approval hierarchies, and an inability to audit past approval decisions.

4. **Inefficient Recruitment and CV Evaluation:**
   Hiring for specialized laboratory or technical positions requires screening numerous resumes. Manual resume parsing and candidate evaluation is slow, subjective, and difficult to track across hiring managers.

5. **Loss of Historical Auditing across Fiscal Periods:**
   When transitioning between fiscal years or updating employee records, organizations often overwrite historical data, losing key audit trails needed for operational reporting and compliance.

---

## 2. The Solution

Northstar solves these operational challenges by providing a unified, enterprise-ready **Human Resources and Lab Office Management System** built with **.NET 9**, **ASP.NET Core Web API**, and **Clean Architecture**.

Key aspects of the solution include:

* **Unified Workspace & Management Dashboard:**
  A polished, responsive web dashboard (`/`) that integrates employee management, department organization, request approval queues, recruitment tracking, and historical reporting into a single workspace.

* **Native Multi-Tenancy & Role-Based Authorization:**
  Entity Framework Core global query filters ensure strict tenant data isolation (`TenantId`), preventing cross-tenant data leaks. JWT authentication combined with Role-Based Access Control (RBAC) enforces granular permissions across roles (`SystemAdmin`, `HrAdmin`, `Manager`, `Employee`, `Recruiter`).

* **Automated Request Lifecycles & Audit History:**
  Standardized workflows for Leave, Financial, Resignation, and Employee Data Change requests with formal status transitions (`Pending`, `Approved`, `Rejected`) and immutable historical logs.

* **AI-Powered CV Processing & Candidate Matching:**
  Centralized job position management, CV document storage, and AI-assisted candidate analysis to evaluate and rank resumes against specific job position criteria automatically.

* **Automated Background Processing:**
  Quartz.NET handles scheduled leave accruals automatically, while optional Hangfire processing sends reminders for pending requests—eliminating manual spreadsheet tracking.

* **Long-Term Data Persistence & Historical Reporting:**
  SQL Server data architecture designed to preserve records across past years, supporting period filtering, headcount trend analysis, and comprehensive compliance reporting.

---

## 3. Data Flow & System Workflows

The system follows **Clean Architecture (Onion Architecture)** principles to separate concerns and guarantee data integrity across presentation, application, domain, and infrastructure layers.

### 3.1 Overall System Architecture Flow

```text
[ Client / Browser Dashboard / REST API Client ]
                      │
                      ▼
   [ Presentation Layer: HrManagmentSystem_API ]
   ├── Controllers (Employees, Requests, Recruitment, etc.)
   ├── JWT Authentication & Tenant Context Middleware
   └── Web Dashboard (wwwroot - HTML/CSS/JS)
                      │
                      ▼
 [ Application Layer: HrManagementSystem_Application ]
   ├── Application Services & Use Cases
   ├── DTO Mappers & Business Rules
   └── Quartz Background Jobs
                      │
                      ▼
    [ Domain Layer: HrManagementSystem_Domain ]
   ├── Domain Entities (Employee, Department, Request, etc.)
   ├── Base Multi-Tenant Interfaces
   └── Domain Rules & Enums
                      │
                      ▼
[ Infrastructure Layer: HrManagementSystem_Infrastructure ]
   ├── EF Core DbContext & Global Tenant Filters
   ├── Repository Implementations & Migrations
   └── File Storage & External Integrations
                      │
                      ▼
               [ SQL Server Database ]
```

---

### 3.2 Workflow 1: User Authentication & Tenant Resolution Flow

```text
User / Browser               API Controller           JWT / Middleware           EF Core DbContext           SQL Server
     │                             │                         │                           │                       │
     │ ── 1. POST /api/auth/login ─►                         │                           │                       │
     │    (Username, Password)     │                         │                           │                       │
     │                             │ ── 2. Validate Creds ───────────────────────────────►                       │
     │                             │ ── 3. Retrieve TenantId & Roles ────────────────────►                       │
     │ ◄─ 4. Return JWT Bearer ────│                         │                           │                       │
     │    (Claims: UserId, TenantId, Role)                   │                           │                       │
     │                             │                         │                           │                       │
     │ ── 5. Request + Bearer Token ────────────────────────►│                           │                       │
     │                             │                         │ ── 6. Extract Claims ───► │                       │
     │                             │                         │    Set TenantContext      │                       │
     │                             │                         │                           │ ── 7. Query DB ─────► │
     │                             │                         │                           │    (Filter TenantId)  │
     │ ◄─ 8. Tenant-Safe Data ───────────────────────────────┴───────────────────────────┴───────────────────────│
```

1. **Authentication:** The client sends credentials to `POST /api/auth/login`.
2. **Token Issuance:** Upon verification, the server generates a JWT containing the user's `UserId`, `TenantId`, and `Roles`.
3. **Request Inspection:** Middleware inspects incoming bearer tokens, validates claims, and populates the current execution context.
4. **Tenant Isolation:** Entity Framework Core automatically attaches `WHERE TenantId = @CurrentTenantId` to database operations, enforcing tenant isolation across all endpoints.

---

### 3.3 Workflow 2: Request Lifecycle (Leave, Financial, Resignation, Data Changes)

```text
Employee                    API / Controller              Application Service           EF Core / Database           Manager / HR Queue
   │                               │                               │                            │                         │
   │ ── 1. Submit Request ────────►│                               │                            │                         │
   │    (e.g., Leave / Financial)  │ ── 2. Pass DTO ──────────────►│                            │                         │
   │                               │                               │ ── 3. Validate Rules ────► │                         │
   │                               │                               │ ── 4. Save Status=Pending ─►                         │
   │                               │                               │ ── 5. Record Audit History►                         │
   │ ◄─ 6. Request Submitted ──────┴───────────────────────────────┴────────────────────────────┤                         │
   │                                                                                            │                         │
   │                                                                                            │ ── 7. Fetch Pending ───►│
   │                                                                                            │    Requests Queue       │
   │                                                                                            │                         │
   │                               ◄── 8. Manager Approves/Rejects (POST /api/requests/{id}/approve)                      │
   │                               │ ── 9. Update Status & Recalculate Balances ───────────────►                         │
   │ ◄─ 10. Request Completed Notification / Status Updated ────────────────────────────────────┘                         │
```

1. **Submission:** An employee submits a request (Leave, Financial, Resignation, or Data Change) through the dashboard or API.
2. **Validation & Creation:** The service layer validates rules (e.g., available leave balance), assigns `Status = Pending`, links the request to the employee's `TenantId`, and records an initial audit history entry.
3. **Approval Queue:** Department Managers or HR Administrators retrieve pending items via `GET /api/requests/for-approval`.
4. **Action & Audit:** Upon approval or rejection, the system updates request state, updates relevant records (e.g. adjusts leave balance), and records immutable history logs.

---

### 3.4 Workflow 3: Recruitment & AI Resume (CV) Ranking Pipeline

```text
Recruiter / HR              Job Position API            Candidate / Document API         AI CV Ranking Engine           SQL / File Storage
      │                            │                               │                               │                            │
      │ ── 1. Create Position ────►│                               │                               │                            │
      │    (POST /api/jobposition) │ ── 2. Save Position Spec ─────────────────────────────────────────────────────────────────►│
      │                            │                               │                               │                            │
      │                            │                               │ ── 3. Upload Application & CV►│                            │
      │                            │                               │    (POST /api/documentcv)     │ ── 4. Save CV File ───────►│
      │                            │                               │                               │    & DB Application Record │
      │                            │                               │                               │                            │
      │ ── 5. Trigger AI Ranking ─────────────────────────────────────────────────────────────────►│                            │
      │    (POST /api/aicvranking/rank/{applicationId})            │                               │ ── 6. Parse CV & Evaluate ─►
      │                                                                                            │    against Job Spec        │
      │ ◄─ 7. Match Score & Candidate Insights Dashboard ──────────────────────────────────────────│ ── 8. Store Rank Score ────►│
```

1. **Job Posting:** HR creates a job position with criteria and required qualifications.
2. **CV Submission:** Candidates or recruiters upload CV documents (`POST /api/documentcv`). The file is stored securely in the system's storage path, and metadata is indexed in SQL Server.
3. **AI Ranking:** When ranking is requested (`POST /api/aicvranking/rank/{id}`), the AI CV Ranking engine extracts text from the CV, compares candidate skills against job requirements, and calculates a match score.
4. **Dashboard View:** Recruiters view ranked candidates sorted by match scores on the Recruitment Dashboard to streamline shortlisting decisions.

---

### 3.5 Workflow 4: Automated Background Operations (Quartz Leave Accruals)

```text
[ Quartz.NET Scheduler ] ─── (Triggers on Cron Schedule) ───► [ Leave Accrual Job ]
                                                                      │
                                                                      ▼
                                                       [ Load Active Employees Across Tenants ]
                                                                      │
                                                                      ▼
                                                       [ Calculate Monthly Leave Accruals ]
                                                                      │
                                                                      ▼
                                                       [ Update Balances & Write Audit Logs ]
```

1. **Automated Trigger:** Quartz.NET runs in the background according to configured schedules.
2. **Accrual Execution:** The job fetches active employees across tenants and calculates accrued leave based on company policies.
3. **Persistence:** Leave balances and system logs are automatically updated without requiring manual administrative intervention.

---

## 4. How to Set Up and Run the Application

### 4.1 Prerequisites
* **Option A (Docker):** Docker Desktop / Docker Engine and Docker Compose.
* **Option B (.NET SDK):** .NET 9.0 SDK and access to SQL Server.

---

### 4.2 Running with Docker Compose (Recommended)

Docker Compose provisions both SQL Server and the ASP.NET Core API server together in isolated containers:

1. **Build and start containers:**
   ```bash
   docker compose up --build
   ```

2. **Access the Application:**
   * **Management Dashboard:** Open `http://localhost:8080` in your web browser.
   * **API Documentation:** Accessible in Development mode at `http://localhost:8080/swagger`.

3. **Stop the Environment:**
   ```bash
   # Stop containers while preserving SQL Server data volume
   docker compose down

   # Stop containers and reset database volume
   docker compose down -v
   ```

---

### 4.3 Running Locally with .NET CLI and SQL Server

1. **Restore dependencies:**
   ```bash
   dotnet restore Hr_Management_System.sln
   ```

2. **Configure Connection String:**
   Ensure `HrManagmentSystem_API/appsettings.json` or .NET User Secrets has a valid `DefaultConnection` pointing to your local SQL Server instance.

3. **Apply Database Migrations:**
   ```bash
   dotnet ef database update --project HrManagementSystem_Infrastructure --startup-project HrManagmentSystem_API
   ```

4. **Start the API & Dashboard:**
   ```bash
   dotnet run --project HrManagmentSystem_API
   ```

5. Open the application URL shown in the terminal output (e.g., `http://localhost:5000`).

---

## 5. Dashboard Navigation Guide

Once logged into the management dashboard at `/`, users can access the following primary sections:

* **Overview:** High-level executive dashboard showing total employee count, pending request action items, department distribution charts, open recruitment positions, and recent system activities.
* **Employees:** Filterable directory of all employee profiles, roles, assigned departments, active/inactive status, and contact information.
* **Departments:** Organizational view displaying team structures, department heads/managers, and headcount statistics.
* **Requests:** Action queue for approving or rejecting leave, financial reimbursement, resignation, and employee data change requests with past period filter options.
* **Recruitment:** Pipeline for viewing open job vacancies, candidate applications, candidate CVs, and AI match scores.
* **Reports:** Analytical views for historical headcount, approval statistics, and hiring pipelines across past fiscal years.
* **Settings:** System status indicators for database connection state, background job execution status, and security configurations.

---

## 6. Summary

The Northstar HR & Lab Office Management System addresses the core challenges of administrative fragmentation, data security, manual request processing, and recruitment overhead. Through Clean Architecture, tenant isolation, automated request workflows, AI candidate ranking, and Quartz background scheduling, Northstar provides a reliable, automated platform for modern office and laboratory administration.
