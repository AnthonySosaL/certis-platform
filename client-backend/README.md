# client-backend

ASP.NET Core 8 Web API. Layered (Domain → Application → Infrastructure →
Api) — see [../docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for why.

## Database

Connects straight to the real MonsterASP.NET SQL Server database (see
[../docs/HOSTING.md](../docs/HOSTING.md)) — no local database needed for
normal work. The connection string lives only in `dotnet user-secrets`,
never in a committed file:

```bash
cd src/EnglishC1.Client.Api
dotnet user-secrets set "ConnectionStrings:AppDb" "Server=...;Database=...;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
```

## Run locally

```bash
cd src/EnglishC1.Client.Api
dotnet watch run
```

## Adding a migration once there's a real entity

```bash
cd client-backend
dotnet ef migrations add <DescriptiveName> \
  --project src/EnglishC1.Client.Infrastructure \
  --startup-project src/EnglishC1.Client.Api \
  --output-dir Persistence/Migrations
dotnet ef database update \
  --project src/EnglishC1.Client.Infrastructure \
  --startup-project src/EnglishC1.Client.Api
```

## Project layout

```
src/
├── EnglishC1.Client.Domain/          entities, no dependencies (currently empty)
├── EnglishC1.Client.Application/     use cases, interfaces (depends on Domain)
├── EnglishC1.Client.Infrastructure/  EF Core (AppDbContext, SQL Server, no DbSets yet)
└── EnglishC1.Client.Api/             controllers, DI wiring, the only HTTP-aware project
tests/
└── EnglishC1.Client.Domain.Tests/    xUnit — currently empty, no Domain logic to test yet
```
