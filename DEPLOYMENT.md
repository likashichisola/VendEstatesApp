# Deploying VendEstatesApp to Render (Docker)

This document explains exactly what to configure in Render to deploy VendEstatesApp
as a Docker-based Web Service backed by Render PostgreSQL, without hard-coding any
secrets in the repository.

## 1. Overview

- **Runtime**: Docker (multi-stage build defined in `VendEstatesApp/Dockerfile`)
- **.NET version**: .NET 10 (`mcr.microsoft.com/dotnet/sdk:10.0` / `aspnet:10.0`)
- **Database**: Render PostgreSQL (accessed via Npgsql/EF Core)
- **Region**: Oregon
- **Branch to deploy**: `master`

## 2. Create the Render Web Service

1. In the Render dashboard, click **New +** → **Web Service**.
2. Connect the GitHub repository `likashichisola/VendEstatesApp`.
3. Select branch **master**.
4. **Runtime**: choose **Docker**.
5. **Dockerfile path**: `VendEstatesApp/Dockerfile`
6. **Docker build context**: repository root (`.`), since the Dockerfile copies
   `VendEstatesApp/VendEstatesApp.csproj` relative to the repo root.
7. **Region**: Oregon.
8. **Instance type**: choose based on your needs (Free/Starter is fine to begin).
9. Render automatically builds the Docker image and runs the container; it also
   automatically injects a `PORT` environment variable that the container must
   listen on. The Dockerfile's `ENTRYPOINT` already honors this via
   `ASPNETCORE_URLS=http://+:${PORT:-8080}`, so **no extra configuration is
   needed for the port**.

## 3. Create the Render PostgreSQL database

1. In the Render dashboard, click **New +** → **PostgreSQL**.
2. Choose region **Oregon** (must match/co-locate with the web service for lowest
   latency).
3. Once created, open the database's **Info** page and copy the **Internal
   Connection String** (use the internal one if the web service is in the same
   Render region/private network; otherwise use the external one).

Render gives you the connection details as a standard Postgres URL
(`postgres://user:password@host:port/dbname`). You need to convert this into an
Npgsql-style connection string (see below).

## 4. Required environment variables on the Web Service

Go to the Web Service → **Environment** tab and add the following:

| Key | Value | Notes |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | Ensures HSTS/HTTPS redirection and production settings are used. |
| `ConnectionStrings__DefaultConnection` | `Host=<db-host>;Port=5432;Database=<db-name>;Username=<db-user>;Password=<db-password>;Ssl Mode=Require;Trust Server Certificate=true` | Build this from the values shown on the Render PostgreSQL **Info** page. **Never commit this value to source control.** |
| `Vapid__PublicKey` | *(your VAPID public key)* | Required only if web-push notifications are used. |
| `Vapid__PrivateKey` | *(your VAPID private key)* | Same as above — keep secret. |
| `Vapid__Subject` | `mailto:admin@vendestates.local` (or your real contact) | Optional override. |

Notes:
- The double underscore (`__`) is how ASP.NET Core configuration binds
  environment variables to nested JSON keys (`ConnectionStrings:DefaultConnection`).
- Do **not** set `PORT` yourself — Render sets it automatically for Docker
  services.
- Do **not** put any of these values into `appsettings.json`,
  `appsettings.Production.json`, the Dockerfile, or any committed file. They are
  intentionally left blank/placeholder in the repository.

## 5. Database schema / migrations

The application already applies EF Core migrations automatically on startup:

```csharp
// Data/DbInitializer.cs
await context.Database.MigrateAsync();
```

This is invoked from `Program.cs` during application startup, **before**
`app.Run()`. This means:

- No manual migration step is required for the first deployment — the schema
  (from `VendEstatesApp/Migrations/20260816183704_InitialCreate.cs`, which is
  already PostgreSQL-native) will be created automatically against the Render
  PostgreSQL database the first time the container starts.
- Subsequent deployments that include new migrations will also apply
  automatically on startup.
- If you ever need to run migrations manually (e.g., troubleshooting), you can
  use Render's **Shell** tab on the web service and run:
  ```bash
  dotnet ef database update --project VendEstatesApp
  ```
  (this requires the EF Core CLI tools to be available in that shell context;
  normally this is not necessary since migrations run automatically at startup).

## 6. Build & start behavior

- **Build**: Render builds the Docker image using the multi-stage
  `VendEstatesApp/Dockerfile`:
  1. Restores and builds with the .NET 10 SDK image.
  2. Publishes the app in `Release` configuration.
  3. Copies the published output into the smaller ASP.NET runtime image.
- **Start**: the container's `ENTRYPOINT` runs
  `dotnet VendEstatesApp.dll`, binding Kestrel to `http://+:$PORT` (Render
  injects `PORT`; defaults to `8080` if run locally without it).
- **Health check**: the app exposes an anonymous `GET /health` endpoint that
  returns `200 OK`. You can configure this as the **Health Check Path** in the
  Render Web Service settings (Settings → Health & Alerts → Health Check Path
  → `/health`).

## 7. HTTPS / reverse proxy behavior

Render terminates TLS at its edge proxy and forwards plain HTTP to your
container along with `X-Forwarded-For` / `X-Forwarded-Proto` headers. The app
now configures `ForwardedHeadersMiddleware` (`Program.cs`) so that
`UseHttpsRedirection()`, HSTS, and cookie "Secure" behavior correctly detect the
original HTTPS request. No additional Render configuration is required for this.

## 8. Local development (unaffected)

Local development continues to work as before via `appsettings.Development.json`
and/or user secrets. Since the plaintext database password was removed from
`appsettings.Development.json`, set your own local connection string using one
of:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true" --project VendEstatesApp
```

or an environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=...;Port=5432;Database=...;Username=...;Password=...;Ssl Mode=Require;Trust Server Certificate=true"
```

## 9. Deploying from `master`

Render is configured to auto-deploy from the `master` branch by default when
you connect the repo (this can be toggled under **Settings → Build & Deploy →
Auto-Deploy**). Every push to `master` triggers a new Docker build and
deployment.

## 10. Security note — rotate the previously committed database password

A real Render PostgreSQL password was previously committed in
`appsettings.Development.json` in this repository's git history. Even though it
has now been removed from the file, **it still exists in the git history and
on GitHub**. You should:

1. Rotate/reset the database password from the Render PostgreSQL dashboard.
2. Update the `ConnectionStrings__DefaultConnection` environment variable on
   the Web Service (and your local user-secrets) with the new password.
3. Optionally, scrub the old secret from git history (e.g., using
   `git filter-repo` or GitHub's secret scanning remediation guidance) if this
   is a public repository.
