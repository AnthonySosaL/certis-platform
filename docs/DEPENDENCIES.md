# Dependencies & tooling log

Every tool, package, or skill added to this project gets an entry here —
what it is, why it was chosen, and when. Removing something? Move its row
to the "Removed" section at the bottom instead of deleting it, so the
history stays visible (per the notes' traceability requirement).

## System tooling

| Tool | Version | Why | Added |
|---|---|---|---|
| .NET SDK | 8.0.424 (LTS) | Backend runtime + CLI (`dotnet new`, EF Core migrations). Only the runtime was preinstalled. First tried `winget install Microsoft.DotNet.SDK.8`, but that install requires admin elevation and hung indefinitely waiting on a UAC prompt with no interactive desktop to show it to — killed it and installed to the user profile instead (`dotnet-install.ps1 -InstallDir %USERPROFILE%\.dotnet`, no admin needed). `PATH` and `DOTNET_ROOT` (User scope) point there now. If `dotnet --list-sdks` ever stops showing 8.0.424, check those two env vars first. | 2026-08-26 |
| Node.js | v22.22.0 (already installed) | Frontend tooling (npm, Vite). | pre-existing |
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

## `client-frontend` (React + Vite)

| Package | Why |
|---|---|
| `react`, `react-dom` | UI framework — scaffolded by `create-vite`. |
| `typescript` | Type safety. |
| `vite`, `@vitejs/plugin-react` | Dev server / bundler. |
| `tailwindcss` v4, `@tailwindcss/vite` | Utility CSS, required by shadcn/ui. |
| `shadcn` CLI (`radix-ui` base, "Nova" preset) | Component library, chosen for reusable/consistent components from the start. Generates owned source in `src/components/ui`, not an opaque dependency. |
| `lucide-react` | Icon set — SVG only, no emoji/text-icons. Pulled in automatically by the shadcn "Nova" preset. |
| `@fontsource-variable/geist` | Self-hosted variable font, no external font CDN request. |
| `react-router-dom` | Client-side routing. |
| `motion` | Animation (the renamed/current Framer Motion package). |
| `i18next`, `react-i18next`, `i18next-browser-languagedetector` | i18n scaffold — English active (the platform's primary language), Spanish resource file present for the app's own UI chrome if ever needed. |
| `@types/node` (dev) | Needed for `import.meta.dirname` path aliasing in `vite.config.ts`. |

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
