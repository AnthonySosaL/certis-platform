# client-backend

ASP.NET Core 8 Web API for the NutriBoost storefront. Layered
(Domain → Application → Infrastructure → Api) — see
[../docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for why.

## Run locally

```bash
# 1. Postgres must be up first (docker compose up -d from the repo root,
#    or just use the "NutriBoost - Start" Desktop shortcut).

# 2. First time only — create the database schema:
cd src/NutriBoost.Client.Api
dotnet ef database update

# 3. Run the API:
dotnet watch run
```

Swagger UI: `http://localhost:5223/swagger` (port from
`src/NutriBoost.Client.Api/Properties/launchSettings.json`).
Health check: `GET /health`. Sample endpoint: `GET /api/products`.

## Adding a migration after changing an entity

```bash
cd src/NutriBoost.Client.Api
dotnet ef migrations add <DescriptiveName>
dotnet ef database update
```

## Project layout

```
src/
├── NutriBoost.Client.Domain/          entities, no dependencies
├── NutriBoost.Client.Application/     use cases, interfaces (depends on Domain)
├── NutriBoost.Client.Infrastructure/  EF Core, repositories (implements Application's interfaces)
└── NutriBoost.Client.Api/             controllers, DI wiring, the only HTTP-aware project
tests/
└── NutriBoost.Client.Domain.Tests/    xUnit — Domain logic is the easiest layer to unit test
```
