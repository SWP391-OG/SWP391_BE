# SWP391_BE (FPTechnical API)

Backend for a technical support / service ticketing system built with **.NET 8** and **C# 12**.  
Provides RESTful APIs for **authentication**, **user/role management**, **campus/location/department/category** management, and a **ticket workflow** (create → assign → resolve → feedback), using **JWT-based authorization** and **SQL Server** via **EF Core**.

---

## Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core** (SQL Server)
- **JWT Bearer Authentication** (+ Cookie scheme configured)
- **Swagger / OpenAPI**
- **AutoMapper**
- **Hangfire** (background job processing for overdue tickets)

---

## Solution Structure

- `SWP391.WebAPI`: API host, controllers, middleware, Swagger, authentication.
- `SWP391.Services`: Business logic (authentication, tickets, users, notifications, etc.).
- `SWP391.Repositories`: EF Core `DbContext`, repositories, Unit of Work, entities.
- `SWP391.Contracts`: DTOs, request/response models, shared API response types.

---

## Core Domain Model (Database)

EF Core `DbContext`: `FPTechnicalContext` (SQL Server)

Main tables/entities:
- `Users`, `Roles`
- `Tickets`
- `Departments`, `Categories`
- `Campuses`, `Locations`
- `Notifications`
- `VerificationCodes` (email verification + password reset)

---

## Authentication & Authorization

- Auth scheme: **JWT Bearer**
- Roles used throughout the API:
  - `Admin`
  - `Staff`
  - `Student`

JWT includes claims:
- `NameIdentifier` (user id)
- `Email`, `Name`
- `Role`
- `UserCode`

Swagger is configured with a **Bearer** security scheme (paste token only; no `Bearer ` prefix).

---

## Ticket Workflow (High-Level)

- **Student**
  - Create ticket
  - Update own **NEW** ticket
  - Cancel own **NEW** ticket (soft-cancel)
  - View own tickets (with pagination/filtering)
  - Provide feedback for **RESOLVED** ticket (closes ticket)

- **Admin**
  - View all tickets (with pagination/filtering)
  - Assign tickets:
    - Auto-assign (least workload)
    - Manual assign (specific staff)
  - Cancel any ticket (soft-cancel)

- **Staff**
  - View assigned tickets (with pagination/filtering)
  - Update status:
    - `ASSIGNED` → `IN_PROGRESS`
    - `IN_PROGRESS` → `RESOLVED` (resolution notes required)

- **Background Job (Hangfire)**
  - Automatically marks overdue tickets as `OVERDUE` every 2 hours

---

## API Endpoints

Base URL default (Development):  
- HTTPS: `https://localhost:7151`
- HTTP: `http://localhost:5056`

Swagger:
- `GET /swagger`

---

### Authentication (`api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/api/auth/register` | Register new user account | No |
| `POST` | `/api/auth/verify-email` | Verify email with code | No |
| `POST` | `/api/auth/resend-verification` | Resend verification code | No |
| `POST` | `/api/auth/forgot-password` | Request password reset | No |
| `POST` | `/api/auth/reset-password` | Reset password with code | No |
| `POST` | `/api/auth/login` | Login and get JWT token | No |

---

### Tickets (`api/Ticket`)

**All ticket endpoints require authentication.**

#### Query Endpoints (GET)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Ticket` | Admin | Get all tickets with pagination and filtering |
| `GET` | `/api/Ticket/{ticketCode}` | All | Get specific ticket by code |
| `GET` | `/api/Ticket/my-tickets` | Student | Get tickets created by current student |
| `GET` | `/api/Ticket/my-assigned-tickets` | Staff | Get tickets assigned to current staff |

#### Create Endpoints (POST)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `POST` | `/api/Ticket` | Student | Create new ticket |

#### Update Endpoints (PUT)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `PUT` | `/api/Ticket/{ticketCode}` | Student | Update own NEW ticket details |

#### Workflow Endpoints (PATCH)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `PATCH` | `/api/Ticket/{ticketCode}/status` | Staff | Update ticket status (ASSIGNED → IN_PROGRESS → RESOLVED) |
| `PATCH` | `/api/Ticket/{ticketCode}/assign` | Admin | Auto-assign ticket to staff with least workload |
| `PATCH` | `/api/Ticket/{ticketCode}/assign/manual` | Admin | Manually assign ticket to specific staff |
| `PATCH` | `/api/Ticket/{ticketCode}/feedback` | Student | Provide feedback for RESOLVED ticket |

#### Delete/Cancel Endpoints (DELETE)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `DELETE` | `/api/Ticket/{ticketCode}` | Student | Cancel own NEW ticket (soft delete) |
| `DELETE` | `/api/Ticket/{ticketCode}/cancel` | Admin | Cancel any ticket (soft delete) |

---

### Users (`api/User`)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/User` | Admin | Get all users |
| `GET` | `/api/User/profile` | All | Get current user's profile |
| `PUT` | `/api/User/profile` | All | Update current user's profile |
| `POST` | `/api/User` | Admin | Create new user |
| `PUT` | `/api/User/{userId}` | Admin | Update user by ID |
| `PATCH` | `/api/User/status` | Admin | Update user status (ACTIVE/INACTIVE) |

---

### Notifications (`api/Notification`)

**All notification endpoints require authentication.**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Notification/my-notifications` | Get current user's notifications with pagination |
| `GET` | `/api/Notification/unread-count` | Get unread notification count |
| `PATCH` | `/api/Notification/{notificationId}/mark-read` | Mark specific notification as read |
| `PATCH` | `/api/Notification/mark-all-read` | Mark all notifications as read |

---

### Campuses (`api/Campus`)

**All campus endpoints require authentication.**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Campus` | Get all campuses |
| `GET` | `/api/Campus/{campusCode}` | Get campus by code |

---

### Locations (`api/Location`)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Locations` | All | Get all locations (admins see all, others see active only) |
| `GET` | `/api/Location/{locationCode}` | All | Get location by code |
| `GET` | `/api/Location/get-by/{campusCode}` | All | Get locations by campus code |
| `POST` | `/api/Location` | Admin | Create new location |
| `PUT` | `/api/Location/{locationId}` | Admin | Update location |
| `PATCH` | `/api/Location/status` | Admin | Update location status (ACTIVE/INACTIVE) |

---

### Departments (`api/Department`)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Departments` | All | Get all departments (admins see all, others see active only) |
| `GET` | `/api/Department/{departmentCode}` | All | Get department by code |
| `POST` | `/api/Department` | Admin | Create new department |
| `PUT` | `/api/Department/{departmentId}` | Admin | Update department |
| `PATCH` | `/api/Department/status` | Admin | Update department status (ACTIVE/INACTIVE) |

---

### Categories (`api/Category`)

| Method | Endpoint | Role | Description |
|--------|----------|------|-------------|
| `GET` | `/api/Category` | All | Get all categories (admins see all, others see active only) |
| `GET` | `/api/Category/{categoryCode}` | All | Get category by code |
| `POST` | `/api/Category` | Admin | Create new category |
| `PUT` | `/api/Category/{categoryId}` | Admin | Update category |
| `PATCH` | `/api/Category/status` | Admin | Update category status (ACTIVE/INACTIVE) |

---

### Roles (`api/Role`)

**All role endpoints require Admin authentication.**

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/Role` | Get all roles |
| `GET` | `/api/Role/{roleName}` | Get role by name |
| `POST` | `/api/Role` | Create new role |
| `PUT` | `/api/Role` | Update role |
| `DELETE` | `/api/Role/{roleId}` | Delete role |

---

## Configuration

### Database
Requires a SQL Server connection string:
- `ConnectionStrings:DefaultConnection`

### JWT
Required configuration:
- `Jwt:Key`
- `Jwt:Issuer` *(optional validation if empty)*
- `Jwt:Audience` *(optional validation if empty)*
- `Jwt:ExpiresMinutes`

### Email (SMTP)
Required for authentication emails:
- `Email:Smtp:Host`
- `Email:Smtp:Port`
- `Email:Smtp:Username`
- `Email:Smtp:Password`
- `Email:Smtp:From`

### Secrets (Development)
In Development environment, the API loads:
- `SWP391.WebAPI/Secrets/appsettings.Secret.json` *(optional)*

Recommended: store local secrets (connection string, JWT key, SMTP) there and keep it out of source control.

---

## Background Jobs (Hangfire)

The system uses **Hangfire** for background job processing:

- **Overdue Ticket Job**: Runs every 2 hours (cron: `0 */2 * * *`)
  - Automatically marks tickets as `OVERDUE` when they exceed their SLA deadline
  - Only affects tickets in `ASSIGNED` or `IN_PROGRESS` status
  - Adds system notes explaining the cancellation

**Hangfire Dashboard**: Available at `/hangfire` (requires authentication in production)

---

## Running Locally (Visual Studio)

1. Ensure **.NET 8 SDK** is installed.
2. Set startup project to `SWP391.WebAPI`.
3. Configure `appsettings.Secret.json` with:
   - SQL Server connection string
   - JWT settings
   - SMTP settings for email
4. Run the project (F5).
5. Open Swagger: `https://localhost:7151/swagger`
6. View Hangfire Dashboard: `https://localhost:7151/hangfire`

---

## License

GNU Affero General Public License v3.0  
See `LICENSE.txt`.

---

## Contributing

Contributions are welcome. Please follow the repository's coding standards and submit PRs with clear context and test steps.