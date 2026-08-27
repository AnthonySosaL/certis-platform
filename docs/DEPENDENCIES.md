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

**Known PATH gotcha**: there's a second, machine-wide `dotnet.exe` at
`C:\Program Files\dotnet` (runtime-only, no SDK — came with the machine).
Depending on the ambient environment, it can resolve *before* the
per-user SDK one in PATH, so a plain `dotnet` call silently runs the
wrong exe ("No .NET SDKs were found") — this bit `scripts/start-dev.ps1`
more than once, including a first attempted fix (prepending to
`$env:Path`) that didn't reliably win the ordering. The real fix: the
script now calls the SDK's `dotnet.exe` by its **full path**
(`%USERPROFILE%\.dotnet\dotnet.exe`) everywhere, sidestepping PATH
resolution entirely — see
[errors/2026-08-26-ng-serve-hangs.md](errors/2026-08-26-ng-serve-hangs.md#update-the-backend-had-a-third-cause-too)
for the debugging trail. If `dotnet` is ever "not found" or resolves to
the wrong one in a **fresh terminal you're typing into yourself** (not
the script), log off/on once, or just call the full path directly.

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

**Two real `ng serve` hangs found and fixed in `scripts/start-dev.ps1`**
(not upstream Angular bugs, just facts about running it from a spawned
Windows console — see errors/2026-08-26-ng-serve-hangs.md for the full
diagnosis):
1. `.angular/cache` gets corrupted whenever the process is killed abruptly
   instead of exiting cleanly (closing the window, `taskkill`, a crash) —
   the next run then hangs forever, port never binds. Fix: the script
   clears that cache before every start.
2. Separately, `ng serve`'s output going straight to a **detached,
   non-redirected** console window hangs it outright, even with a clean
   cache — confirmed by testing the same command with and without output
   redirection, repeatedly, with consistent results either way. Fix: the
   script redirects `ng serve`'s output to `%TEMP%\ng-serve.log` instead
   of letting it print directly to the spawned window. Tail that file if
   the frontend window looks stuck.

## `client-backend` (ASP.NET Core, layered: Domain/Application/Infrastructure/Api)

| Package | Project | Why |
|---|---|---|
| `Microsoft.EntityFrameworkCore` 8.0.11 | Infrastructure | ORM — pinned to the 8.x line to match the net8.0 target (`dotnet add` defaults to the newest major, which was 10.x and incompatible). |
| `Microsoft.EntityFrameworkCore.SqlServer` 8.0.11 | Infrastructure | SQL Server provider for EF Core — swapped in for Npgsql on 2026-08-26 once the real database (MonsterASP.NET, SQL Server 2025 free tier) was created. See `docs/HOSTING.md`. |
| `Microsoft.EntityFrameworkCore.Design` 8.0.11 | Api | Enables `dotnet ef migrations` from the Api project. |
| `Swashbuckle.AspNetCore` | Api | Swagger/OpenAPI UI — scaffolded by default with `dotnet new webapi`, kept for local API exploration. Configured with a Bearer auth scheme (2026-08-26) so `/api/auth/me` etc. can be tested straight from Swagger UI. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.11 | Infrastructure | ASP.NET Core Identity's EF Core store — `AppDbContext` is now an `IdentityDbContext`. |
| `Microsoft.Extensions.Identity.Core` 8.0.11 | Infrastructure | Needed explicitly (not pulled in transitively in a way the compiler could see) for `IdentityBuilder` extensions like `AddRoles<T>`. |
| `Microsoft.Extensions.Options.ConfigurationExtensions` 8.0.0 | Infrastructure | Needed explicitly for `services.Configure<JwtOptions>(IConfigurationSection)` — a class library project doesn't get this for free the way an ASP.NET Core Web SDK project does. |
| `Microsoft.AspNetCore.Authentication.JwtBearer` 8.0.11 | Api | JWT bearer authentication scheme for validating incoming tokens. |

The `AppDb` connection string and `Jwt:SigningKey` both live only in
`dotnet user-secrets` (`dotnet user-secrets set "ConnectionStrings:AppDb"
"..."` / `"Jwt:SigningKey" "..."` from `src/EnglishC1.Client.Api`) — never
in a committed `appsettings*.json`. The connection string points at
MonsterASP's Remote Access (SSMS) endpoint, not Local Access (which only
works from apps hosted on MonsterASP itself). The signing key was
generated once (`openssl rand -base64 48`) and is just a random secret —
nothing tied to an external account, so no confirmation needed to
generate it, unlike the database password. **Not yet added to the
production `web.config`** on MonsterASP (same gotcha as the connection
string — see `docs/PENDING_IDEAS.md` "Rough edges") — the deployed API
will 500 on any auth endpoint until that's done, same as it did for the
database before that was fixed.

`AppDbContext`'s domain model so far is Identity's own tables
(`AspNetUsers`, `AspNetRoles`, etc., via `IdentityDbContext`) — the actual
English-practice entities (exercises, attempts, progress...) still wait
on the feature scope decision in [PENDING_IDEAS.md](PENDING_IDEAS.md).
Worth keeping for whenever it does: if any entity needs optimistic
concurrency (e.g. two people submitting the same exercise attempt at
once), SQL Server's idiom is a `byte[]` property mapped with
`.IsRowVersion()` (a `rowversion`/
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
