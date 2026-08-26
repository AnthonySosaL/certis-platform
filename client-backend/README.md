# client-backend

ASP.NET Core 8 Web API. Layered (Domain → Application → Infrastructure →
Api) — see [../docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for why.

Project/namespace names are still `NutriBoost.Client.*`, a holdover from
an early scope mistake (see
[../docs/errors/2026-08-26-scope-mixup.md](../docs/errors/2026-08-26-scope-mixup.md)).
Not renamed yet — tracked in `../docs/PENDING_IDEAS.md`.

## Run locally

```bash
# 1. Postgres must be up first (docker compose up -d from the repo root,
#    or just use the "Project - Start" Desktop shortcut).

# 2. Run the API — no migrations yet, there's no domain model
#    (AppDbContext has zero entities) until feature scope is decided:
cd src/NutriBoost.Client.Api
dotnet watch run
```

Swagger UI: `http://localhost:5223/swagger` (port from
`src/NutriBoost.Client.Api/Properties/launchSettings.json`).
Health check: `GET /health`.

## Adding a migration once there's a real entity

```bash
cd src/NutriBoost.Client.Api
dotnet ef migrations add <DescriptiveName> \
  --project ../NutriBoost.Client.Infrastructure \
  --startup-project .
dotnet ef database update \
  --project ../NutriBoost.Client.Infrastructure \
  --startup-project .
```

## Project layout

```
src/
├── NutriBoost.Client.Domain/          entities, no dependencies (currently empty)
├── NutriBoost.Client.Application/     use cases, interfaces (depends on Domain)
├── NutriBoost.Client.Infrastructure/  EF Core (AppDbContext, no DbSets yet)
└── NutriBoost.Client.Api/             controllers, DI wiring, the only HTTP-aware project
tests/
└── NutriBoost.Client.Domain.Tests/    xUnit — currently empty, no Domain logic to test yet
```
