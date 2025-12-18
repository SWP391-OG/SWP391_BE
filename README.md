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
  - View own tickets
  - Provide feedback for **RESOLVED** ticket (closes ticket)

- **Admin**
  - View all tickets (with pagination/filtering)
  - Assign tickets:
    - Auto-assign (least workload)
    - Manual assign (specific staff)
  - Cancel any ticket (soft-cancel)
  - View overdue tickets, escalate ticket

- **Staff**
  - View assigned tickets
  - Update status:
    - `ASSIGNED` → `IN_PROGRESS`
    - `IN_PROGRESS` → `RESOLVED` (resolution notes required)
  - View own overdue tickets

---

## API Endpoints (Key Routes)

Base URL default (Development):  
- HTTPS: `https://localhost:7151`
- HTTP: `http://localhost:5056`

Swagger:
- `GET /swagger`

Auth (`api/auth`):
- `POST /api/auth/register`
- `POST /api/auth/verify-email`
- `POST /api/auth/resend-verification`
- `POST /api/auth/forgot-password`
- `POST /api/auth/reset-password`
- `POST /api/auth/login`

Tickets (`api/Ticket`) *(requires auth)*:
- `GET /api/Ticket` *(Admin)*
- `GET /api/Ticket/{ticketCode}`
- `GET /api/Ticket/my-tickets` *(Student)*
- `GET /api/Ticket/my-assigned-tickets` *(Staff)*
- `POST /api/Ticket` *(Student)*
- `PUT /api/Ticket/{ticketCode}` *(Student)*
- `PATCH /api/Ticket/{ticketCode}/status` *(Staff)*
- `PATCH /api/Ticket/{ticketCode}/assign` *(Admin)*
- `PATCH /api/Ticket/{ticketCode}/assign/manual` *(Admin)*
- `PATCH /api/Ticket/{ticketCode}/feedback` *(Student)*
- `DELETE /api/Ticket/{ticketCode}` *(Student)*
- `DELETE /api/Ticket/{ticketCode}/cancel` *(Admin)*
- `GET /api/Ticket/overdue` *(Admin)*
- `GET /api/Ticket/my-overdue-tickets` *(Staff)*
- `PATCH /api/Ticket/{ticketCode}/escalate` *(Admin)*

Master data:
- Locations (`api/Location`)
  - `GET /api/Locations` *(authenticated)*
  - `GET /api/Location/{locationCode}` *(authenticated)*
  - `GET /api/Location/get-by/{campusCode}` *(authenticated)*
  - `POST /api/Location` *(Admin)*
  - `PUT /api/Location/{locationId}` *(Admin)*
  - `PATCH /api/Location/status` *(Admin)*
  - `DELETE /api/Location/{locationId}` *(Admin)*

- Departments (`api/Department`)
  - `GET /api/Departments` *(authenticated)*
  - `GET /api/Department/{departmentCode}` *(authenticated)*
  - `POST /api/Department` *(Admin)*
  - `PUT /api/Department/{departmentId}` *(Admin)*
  - `PATCH /api/Department/status` *(Admin)*
  - `DELETE /api/Department/{departmentId}` *(Admin)*

- Categories (`api/Category`)
  - `GET /api/Category` *(authenticated)*
  - `GET /api/Category/{categoryCode}` *(authenticated)*
  - `POST /api/Category` *(Admin)*
  - `PUT /api/Category/{categoryId}` *(Admin)*
  - `PATCH /api/Category/status` *(Admin)*
  - `DELETE /api/Category/{categoryId}` *(Admin)*

- Roles (`api/Role`) *(Admin-only)*

> Note: additional controllers exist (e.g., users/notifications/campus) per the solution structure.

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

### Secrets (Development)
In Development environment, the API loads:
- `SWP391.WebAPI/Secrets/appsettings.Secret.json` *(optional)*

Recommended: store local secrets (connection string, JWT key, SMTP) there and keep it out of source control.

---

## Running Locally (Visual Studio)

1. Ensure **.NET 8 SDK** is installed.
2. Set startup project to `SWP391.WebAPI`.
3. Configure SQL Server + `DefaultConnection`.
4. Run the project.
5. Open Swagger:
   - `https://localhost:7151/swagger`

---

## License

GNU Affero General Public License v3.0  
See `LICENSE.txt`.

---

## Contributing

Contributions are welcome. Please follow the repository’s coding standards and submit PRs with clear context and test steps.