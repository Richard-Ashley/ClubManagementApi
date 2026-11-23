# ClubManagementApi

A REST API for managing a sports/social club: members, venues, time slots and bookings. Built as a traditional layered ASP.NET Core 8 Web API (Controllers → Services → EF Core), with JWT authentication and role-based authorization.

A Flutter client, ClubManagerApp, consumes this API.

## Tech stack

- ASP.NET Core 8, C#
- Entity Framework Core 8 with SQLite
- JWT bearer authentication, BCrypt password hashing
- Swagger / OpenAPI (Swashbuckle), enabled in Development
- xUnit tests (17)

## Project structure

```
Controllers/     Auth, Members, Venues, Bookings
Services/        Interfaces/ and Implementations/ (business logic)
Models/          Entities/ and DTOs/
Data/            AppDbContext, seed data
Middleware/      ExceptionMiddleware (centralised error handling)
Migrations/      EF Core migrations
ClubManagementApi.Tests/   xUnit tests for AuthService and BookingService
```

## Getting started

Prerequisite: [.NET 8 SDK](https://dotnet.microsoft.com/download).

### 1. Set the JWT signing key

The key is not stored in the repo. `appsettings.json` contains only a placeholder, and the app refuses to start without a real key. Generate one (32+ characters) and store it in user secrets:

```powershell
# PowerShell: generate a random key
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }) -as [byte[]])
```

```bash
dotnet user-secrets set "Jwt:Key" "<your-generated-key>"
```

(In other environments, set the `Jwt__Key` environment variable instead.)

### 2. Run

```bash
dotnet run
```

In Development, the database (`clubmanagement.db`, SQLite) is created and migrations are applied automatically on startup, along with seed data.

- HTTP: http://localhost:5014
- HTTPS (https launch profile): https://localhost:7190
- Swagger UI: `/swagger` on either address

HTTPS redirection is disabled in Development so emulators and devices can call the plain HTTP endpoint.

### 3. Sign in

A seeded admin account is created for local development:

| Email / password | Role |
|---|---|
| `admin@clubapi.com` / `Admin@123` | Admin |

Call `POST /auth/login`, copy the token, and click **Authorize** in Swagger. This account is for local development only; change or remove it before any real deployment.

## API overview

| Resource | Endpoint | Access |
|---|---|---|
| Auth | `POST /auth/register`, `POST /auth/login` | Public |
| Members | `GET /members` | Admin |
| | `GET /members/{id}`, `PUT /members/{id}` | Authenticated |
| | `DELETE /members/{id}` | Admin |
| Venues | `GET /venues`, `GET /venues/{id}`, `GET /venues/{id}/slots` | Authenticated |
| | `POST /venues`, `PATCH /venues/{id}/toggle-active`, `POST /venues/{id}/slots` | Admin |
| Bookings | `GET /bookings/my`, `POST /bookings`, `DELETE /bookings/{id}` | Authenticated |
| | `GET /bookings` | Admin |

Double-booking is prevented both in the service layer (conflict validation) and by a unique database index on slot + date + status.

## Tests

```bash
dotnet test
```

17 xUnit tests cover `AuthService` and `BookingService`.

## Notes

- SQLite keeps setup to zero infrastructure; swap the provider in `Program.cs` for a server database in production.
- Production: set `Jwt__Key` via environment variable or a secret store, and run outside Development so HTTPS redirection is on. Migrations are only auto-applied in Development.

## Known limitations

- The seeded admin account uses a fixed, publicly documented password. It exists so the auth flow can be tried immediately; for a real deployment, gate the seed to Development or generate a one-time password.
