# Vend Estates Management System

A full-stack ASP.NET Core MVC business management system covering Lodge, Car Rental, and Agro Field operations for Vend Estates, built on .NET 10 with Entity Framework Core and SQL Server.

## Modules

1. **Authentication** – Cookie-based auth with BCrypt-hashed passwords and role-based authorization (Director, Manager, Accountant).
2. **Employee Management** – Employee profiles, branches, and employment contracts.
3. **Lodge** – Room inventory, occupancy dashboard, and guest bookings (including long-term stay tracking).
4. **Car Rental** – Vehicle fleet management and rental bookings.
5. **Agro Field** – Inventory tracking with low-stock alerts and sales recording.
6. **Payroll** – Statutory payroll computation (PAYE, NAPSA, NHIMA) with approval workflow.
7. **Expenses & Payments** – Expense requests with approval workflow and multi-source payment recording.
8. **Leave Management** – Leave applications, approvals, and cancellations.
9. **Notifications** – In-app notification center with read/unread tracking.
10. **Dashboards & Reports** – Role-aware operational dashboard plus income/expense, branch expense, and payroll reports.

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (SQL Server)
- Bootstrap 5 + Bootstrap Icons
- BCrypt.Net-Next for password hashing
- Cookie Authentication

## Getting Started (Local Development)

1. Ensure SQL Server LocalDB is installed (included with Visual Studio).
2. Restore and build:
   ```powershell
   dotnet restore
   dotnet build
   ```
3. Apply migrations (the app also applies pending migrations automatically on startup):
   ```powershell
   dotnet ef database update --project VendEstatesApp
   ```
4. Run the application:
   ```powershell
   dotnet run --project VendEstatesApp
   ```
5. Log in with the seeded default account:
   - **Username:** `director`
   - **Password:** `Director@123`

   Change this password immediately in any non-local environment.

## Configuration

Connection strings and settings are configured via `appsettings.json` / `appsettings.Development.json`. For production, supply a `ConnectionStrings__DefaultConnection` value via environment variable or a machine-level `appsettings.Production.json` (not committed) rather than editing the checked-in template.

## Deployment

### Docker

A `Dockerfile` is provided under `VendEstatesApp/Dockerfile`, along with a `docker-compose.yml` at the repository root that runs the app alongside a SQL Server container.

```powershell
docker compose up --build
```

The app will be available at `http://localhost:8080`, backed by a SQL Server container. Update the SA password and connection string for any real deployment.

### IIS / App Service

Publish with:
```powershell
dotnet publish VendEstatesApp -c Release -o ./publish
```
Then deploy the contents of `./publish` to IIS or Azure App Service, configuring the `DefaultConnection` connection string and `ASPNETCORE_ENVIRONMENT=Production` via environment/app settings.

## Notes

- Database schema changes are managed with EF Core Migrations (`Data/Migrations`). The app calls `Database.MigrateAsync()` on startup to keep the schema current.
- Global authorization requires authentication for all controllers except `Account` (Login/AccessDenied) and `Home` (Error/NotFound).
