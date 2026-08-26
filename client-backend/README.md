# client-backend

ASP.NET Core 8 Web API. Layered (Domain → Application → Infrastructure →
Api) — see [../docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for why.

Project/namespace names are still `NutriBoost.Client.*`, a holdover from
an early scope mistake (see
[../docs/errors/2026-08-26-scope-mixup.md](../docs/errors/2026-08-26-scope-mixup.md)).
Not renamed yet — tracked in `../docs/PENDING_IDEAS.md`.

## Database

Connects straight to the real MonsterASP.NET SQL Server database (see
[../docs/HOSTING.md](../docs/HOSTING.md)) — no local database needed for
normal work. The connection string lives only in `dotnet user-secrets`,
never in a committed file:

```bash
cd src/NutriBoost.Client.Api
dotnet user-secrets set "ConnectionStrings:AppDb" "Server=...;Database=...;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

## Run locally

```bash
cd src/NutriBoost.Client.Api
dotnet watch run
```

## Adding a migration once there's a real entity

```bash
cd client-backend
dotnet ef migrations add <DescriptiveName> \
  --project src/NutriBoost.Client.Infrastructure \
  --startup-project src/NutriBoost.Client.Api \
  --output-dir Persistence/Migrations
dotnet ef database update \
  --project src/NutriBoost.Client.Infrastructure \
  --startup-project src/NutriBoost.Client.Api
```

## Project layout

```
src/
├── NutriBoost.Client.Domain/          entities, no dependencies (currently empty)
├── NutriBoost.Client.Application/     use cases, interfaces (depends on Domain)
├── NutriBoost.Client.Infrastructure/  EF Core (AppDbContext, SQL Server, no DbSets yet)
└── NutriBoost.Client.Api/             controllers, DI wiring, the only HTTP-aware project
tests/
└── NutriBoost.Client.Domain.Tests/    xUnit — currently empty, no Domain logic to test yet
```
