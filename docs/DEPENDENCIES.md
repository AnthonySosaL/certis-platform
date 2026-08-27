# Dependencies & tooling log

Every tool, package, or skill added to this project gets an entry here —
what it is, why it was chosen, and when. Removing something? Move its row
to the "Removed" section at the bottom instead of deleting it, so the
history stays visible (per the notes' traceability requirement).

## System tooling

| Tool | Version | Why | Added |
|---|---|---|---|
| .NET SDK | 8.0.424 (LTS) | Backend runtime + CLI (`dotnet new`, EF Core migrations). Only the runtime was preinstalled. First tried `winget install Microsoft.DotNet.SDK.8`, but that install requires admin elevation and hung indefinitely waiting on a UAC prompt with no interactive desktop to show it to — killed it and installed to the user profile instead (`dotnet-install.ps1 -InstallDir %USERPROFILE%\.dotnet`, no admin needed). `PATH` and `DOTNET_ROOT` (User scope) point there now. If `dotnet --list-sdks` ever stops showing 8.0.424, check those two env vars first. | 2026-08-26 |
| Node.js | v22.22.3 (via `nvm4w`, already installed) | Frontend tooling. Updated from v22.22.0 — the latest Angular CLI requires >=22.22.3. `nvm4w` was already on this machine (`nvm install 22.22.3` + `nvm use 22.22.3`, no elevation needed). | pre-existing, updated 2026-08-26 |
| Docker Desktop | 29.4.3 (already installed) | Was local Postgres via `docker-compose.yml`. No longer used day to day — the app connects to the real MonsterASP.NET database now (see `docs/HOSTING.md`). Kept only as an offline fallback, not auto-started. | pre-existing, wired 2026-08-26, demoted to fallback same day once MonsterASP DB was live |
| git | 2.53.0 (already installed) | Version control. | pre-existing |

**Known PATH gotcha**: because the SDK is user-scoped, not machine-wide, a
process tree that started before the PATH change won't see it (Explorer
doesn't re-read env vars for already-open sessions until logoff/reboot).
This bit `scripts/start-dev.ps1` — the backend window it spawned couldn't
find `dotnet` at all. Fixed by having the script prepend
`%USERPROFILE%\.dotnet` and `...\.dotnet\tools` to its own `$env:Path`
before spawning anything, so it's self-contained regardless of ambient
PATH staleness. If `dotnet` is ever "not found" in a fresh terminal, log
off/on (or reboot) once to pick up the registry PATH everywhere.

## `client-frontend` (Angular 22 + Material)

Switched from React on 2026-08-26 — see "Removed" below for what it
replaced and why.

| Package | Why |
|---|---|
| `@angular/core`, `@angular/router`, `@angular/cli` | Framework + CLI, `ng new` default. Standalone components, signals. |
| `@angular/material`, `@angular/cdk` | Component library — added via `ng add @angular/material` (theme: initially azure-blue, retargeted to a green Material 3 palette in `src/styles.scss` to match the brand accent the React version used). |
| TypeScript, Vite (bundler, used internally by the Angular 22 CLI) | Scaffolded by `ng new`. |

No icon package — a handful of inline SVGs in `navbar.html` (menu,
sun/moon) instead. `lucide-angular`'s peer dependency range doesn't cover
Angular 22 yet (too new, `npm install` failed with an ERESOLVE conflict);
inline SVG sidesteps that without waiting on an upstream update, and stays
true to the "SVG, not an icon font" preference either way.

**Known gotcha**: `ng serve` can hang indefinitely after several
interrupted runs (stuck rebuilding, port never binds) — clearing the
`.angular/` cache directory in `client-frontend` and restarting fixes it.
Not a real bug, just what happened during this session's repeated manual
testing; unlikely to matter for normal day-to-day use.

## `client-backend` (ASP.NET Core, layered: Domain/Application/Infrastructure/Api)

| Package | Project | Why |
|---|---|---|
| `Microsoft.EntityFrameworkCore` 8.0.11 | Infrastructure | ORM — pinned to the 8.x line to match the net8.0 target (`dotnet add` defaults to the newest major, which was 10.x and incompatible). |
| `Microsoft.EntityFrameworkCore.SqlServer` 8.0.11 | Infrastructure | SQL Server provider for EF Core — swapped in for Npgsql on 2026-08-26 once the real database (MonsterASP.NET, SQL Server 2025 free tier) was created. See `docs/HOSTING.md`. |
| `Microsoft.EntityFrameworkCore.Design` 8.0.11 | Api | Enables `dotnet ef migrations` from the Api project. |
| `Swashbuckle.AspNetCore` | Api | Swagger/OpenAPI UI — scaffolded by default with `dotnet new webapi`, kept for local API exploration. |

The `AppDb` connection string lives only in `dotnet user-secrets` (set via
`dotnet user-secrets set "ConnectionStrings:AppDb" "..."` from
`src/EnglishC1.Client.Api`) — never in a committed `appsettings*.json`.
It points at MonsterASP's Remote Access (SSMS) endpoint, not Local Access
(which only works from apps hosted on MonsterASP itself).

`AppDbContext` has no entities yet — the domain model waits on the feature
scope decision in [PENDING_IDEAS.md](PENDING_IDEAS.md). Worth keeping for
whenever it does: if any entity needs optimistic concurrency (e.g. two
people submitting the same exercise attempt at once), SQL Server's idiom
is a `byte[]` property mapped with `.IsRowVersion()` (a `rowversion`/
`timestamp` column) — straightforward with `Microsoft.EntityFrameworkCore.
SqlServer`, unlike the Npgsql/Postgres setup this project briefly used,
which needed the `xmin` shadow-property workaround instead.

## Claude Code skills relied on for this project

Not installed via a package manager — these are skills already available to
Claude Code sessions in this environment. Listed here so it's clear which
ones this project leans on and why, per the notes' request to track "skills
used" like any other tool:

| Skill | Why |
|---|---|
| `anthropic-skills:dev-engineering-rules` | Protect-existing-logic / one-layer-per-change / reuse-before-creating / 200-line-file-cap rules — applied to every coding session on this repo. |
| `engineering:code-review` | Use before merging any feature branch to `main`. |
| `engineering:architecture` | Use when a new architecture decision record is needed (e.g. finalizing the domain model once feature scope is set). |
| `engineering:testing-strategy` | Use once the backend has enough surface area to need a real test plan. |
| `engineering:documentation` | Use for README/runbook writing beyond what's already in `docs/`. |

## Removed

| What | Why removed | Date |
|---|---|---|
| `Product` domain entity, `ProductsController`, `IProductRepository`/`ProductRepository`, `InitialCreate` migration | Wrong-project scope (e-commerce domain) — see [errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md). Migration rolled back before deletion. | 2026-08-26 |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Switched to `Microsoft.EntityFrameworkCore.SqlServer` once the real database was created on MonsterASP.NET (SQL Server, not Postgres). | 2026-08-26 |
| Entire React `client-frontend` (`react`, `react-router-dom`, `shadcn`/Radix components, `motion`, `i18next`, `@fontsource-variable/geist`, `tailwindcss`) | Deliberate framework switch to Angular, not a mistake — the user has many existing React projects and wants portfolio breadth. Fully recoverable from git history if ever needed (nothing lost, just not the active choice). Blazor was also briefly considered/started as a "C# frontend" option before the user chose Angular by name. | 2026-08-26 |
