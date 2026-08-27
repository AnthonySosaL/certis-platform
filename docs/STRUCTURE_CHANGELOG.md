# Structure changelog

Every new top-level folder, new app, or significant architectural change
gets an entry here, newest first — this is the traceability log the notes
asked for, separate from git history so it reads as a narrative instead of
a diff.

## 2026-08-27 — Backend auth: register/login/JWT, first real domain model

`client-backend` now has actual auth, not just a health check:

- `AppDbContext` -> `IdentityDbContext<ApplicationUser, IdentityRole<Guid>,
  Guid>`. `ApplicationUser` lives in `Infrastructure/Identity/` (tightly
  coupled to the Identity framework, not a pure Domain concept).
- `AddIdentityCore<ApplicationUser>` wired up in
  `Infrastructure/DependencyInjection.cs` (8-char minimum password,
  unique email required, `RequireConfirmedEmail = false` since there's no
  email sender yet). `AddDefaultTokenProviders()` deliberately not called
  yet — only needed for password-reset/email-confirmation tokens.
- JWT bearer auth: `JwtOptions` (Issuer/Audience/ExpirationMinutes in
  `appsettings.json`, `SigningKey` in `dotnet user-secrets` only — a
  freshly generated random secret, not tied to any account) and
  `JwtTokenService` in Infrastructure; `AddAuthentication().AddJwtBearer()`
  wired in `Program.cs`. JWT over cookies because Angular calls the API
  cross-origin as a separate SPA.
- `AuthController` (`Api/Auth/`): `POST /api/auth/register`,
  `POST /api/auth/login`, `GET /api/auth/me` (`[Authorize]`). Swagger UI
  now has a Bearer auth scheme configured so these are testable directly
  from `/swagger`.
- Migration `AddIdentity` created and applied against the real MonsterASP
  database (all `AspNetUser*`/`AspNetRole*` tables). Verified end-to-end
  with curl: register -> login -> `/me` without a token (401) -> `/me`
  with a valid token (200, correct email back).
- CORS origin fixed from the stale `localhost:5173` (old React/Vite port)
  to `localhost:4200` (Angular) — leftover from the frontend switch that
  hadn't been caught since nothing had exercised CORS yet.

Not done yet: Angular-side auth (login/register pages, auth service,
interceptor, route guard), the JWT signing key isn't added to the
production `web.config` yet (same class of gotcha as the connection
string — deployed auth endpoints will 500 until that's done), Google
OAuth, and password reset. See
[PENDING_IDEAS.md](PENDING_IDEAS.md#rough-edges-worth-revisiting).

## 2026-08-26 — Fixed backend hang too: dotnet.exe now called by full path

Follow-up to the ng serve fixes below — once the frontend was reliable,
the **backend** started hanging the same way (window open, port 5223
never bound). Redirecting its output too (same trick as the frontend)
revealed the real cause: a plain `dotnet` call in the spawned window was
resolving to the wrong `dotnet.exe` (a machine-wide, SDK-less one at
`C:\Program Files\dotnet`) instead of the per-user SDK. An earlier fix
for this exact class of problem (prepending to `$env:Path`) didn't
actually work — its "is the path already present" check matched a
substring that existed *later* in PATH, so it skipped prepending and the
wrong exe kept winning. Fixed for real by calling the SDK's `dotnet.exe`
by its full path everywhere in the script, avoiding PATH resolution
entirely. Full trail in
[errors/2026-08-26-ng-serve-hangs.md](errors/2026-08-26-ng-serve-hangs.md#update-the-backend-had-a-third-cause-too).
Verified: two clean stop/start round-trips, frontend + backend both up
within seconds each time.

## 2026-08-26 — Fixed ng serve hanging on start

Found (the frontend silently failed to come up after `start.bat`, twice)
and fixed two separate causes — full diagnosis in
[errors/2026-08-26-ng-serve-hangs.md](errors/2026-08-26-ng-serve-hangs.md).
`scripts/start-dev.ps1` now clears `client-frontend/.angular/cache` before
every frontend start and redirects `ng serve`'s output to
`%TEMP%\ng-serve.log` instead of the spawned console window (the second
cause; that specific combination reliably prevented the hang across
repeated testing). Verified with two clean stop/start round-trips.

## 2026-08-26 — Frontend switched: React -> Angular 22 + Material

Deliberate change, not a correction — the user has many existing React
projects and wants portfolio breadth. Blazor (writing the frontend in C#
too, leaning fully into the .NET-skills goal) was floated first and
briefly started, but the user chose Angular by name after weighing it, so
that's what got built.

- Deleted `client-frontend` (React) and rebuilt it from scratch with
  `ng new client-frontend --routing --style=scss --ssr=false`, then
  `ng add @angular/material` (Material 3 theming via `mat.theme()`,
  palette retargeted from the default azure-blue to green to match the
  brand accent the React version used).
- Rebuilt the same shell the React version had: `Navbar` (responsive —
  horizontal nav >= 768px, `mat-menu` mobile dropdown below that),
  `Footer`, a placeholder `Home` page, and a signal-based `Theme` service
  (`src/app/core/theme.ts`) — light default, dark mode fully wired via an
  `html.dark` class.
- Icons: inline SVG (menu, sun/moon) instead of a package — `lucide-angular`
  doesn't support Angular 22 yet (peer dependency conflict), and inline
  SVG was simplest not to block on that.
- Had to update Node.js (v22.22.0 -> v22.22.3 via the already-installed
  `nvm4w`) — the latest Angular CLI refused to run on the older patch
  version. See `docs/DEPENDENCIES.md`.
- Updated `scripts/start-dev.ps1` / `stop-dev.ps1` (port 5173 -> 4200) and
  `.claude/launch.json` / `scripts/dev-frontend.cmd` for the preview
  tooling. Verified end-to-end: build clean, dev server serves correctly,
  dark mode toggle works, mobile breakpoint correctly collapses the nav.
- Old React code isn't lost — fully recoverable from git history — but is
  not part of the active codebase.

## 2026-08-26 — Backend deployed live: MonsterASP.NET

`client-backend` is reachable at `https://english-c1-api.runasp.net`
(FreeSite plan, EU datacenter, HTTPS via Let's Encrypt with HTTP->HTTPS
redirect on). `/health` verified 200 over both HTTP (redirects) and
HTTPS. Deployed via `dotnet publish` + `scp` (SFTP) to `wwwroot/` — chosen
over WebDeploy (would've needed installing Web Deploy/msdeploy locally)
and Git deploy (needs a GitHub repo, not set up yet). Full steps and the
production-connection-string handling (env var injected into
`web.config`, not committed anywhere) in
[HOSTING.md](HOSTING.md#backend-hosting-monsteraspnet-live-decided-2026-08-26).
Known fragility logged in [PENDING_IDEAS.md](PENDING_IDEAS.md#rough-edges-worth-revisiting):
`web.config` gets regenerated (and the env var lost) on every fresh
`dotnet publish`.

## 2026-08-26 — Backend renamed: NutriBoost.Client.* -> EnglishC1.Client.*

Closed out the last tracked holdover from the scope mix-up (see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md)):
every folder, `.sln`/`.csproj` file, and C# `namespace` declaration in
`client-backend` renamed `NutriBoost.Client.*` -> `EnglishC1.Client.*`
(Domain, Application, Infrastructure, Api, and the Domain.Tests project).
Rebuilt clean, tests still pass (0 tests — the project is still empty),
and re-verified the app starts and connects to the real MonsterASP
database under the new names. `EnglishC1` is a descriptive placeholder
tied to the platform's subject matter, not a proposed final brand — see
[NAMING.md](NAMING.md), still undecided. Updated every doc and script that
referenced the old path (`README.md`, `client-backend/README.md`,
`ARCHITECTURE.md`, `DEPENDENCIES.md`, `PENDING_IDEAS.md`, `NAMING.md`) —
`scripts/start-dev.ps1` needed no change, it finds the backend project by
glob (`*.Api.csproj`), not by name.

## 2026-08-26 — Real database: MonsterASP.NET SQL Server, dropped local Postgres

- Created the real dev database on MonsterASP.NET's free plan: SQL Server
  2025, EU/Germany datacenter, Remote Access (SSMS) enabled for
  connections from outside their network. See
  [HOSTING.md](HOSTING.md#database-monsterasp.net-sql-server-2025-decided-2026-08-26)
  for the full picture (why SQL Server over MySQL, free-tier limits).
- Swapped `client-backend`'s EF Core provider: `Npgsql.EntityFrameworkCore.
  PostgreSQL` -> `Microsoft.EntityFrameworkCore.SqlServer`
  (`DependencyInjection.cs`: `UseNpgsql` -> `UseSqlServer`).
- Removed the local Postgres connection string from
  `appsettings.Development.json` (it's gone now — the real connection
  string lives only in `dotnet user-secrets`, never in a committed file).
- Ran `dotnet ef migrations add InitialCreate` (empty — no entities yet)
  and `dotnet ef database update` against the real remote database as an
  end-to-end connectivity test: it connected, created
  `__EFMigrationsHistory`, and recorded the migration. Confirms the full
  local-machine -> internet -> MonsterASP path works before any real
  domain model gets built on top of it.
- `scripts/start-dev.ps1` no longer auto-starts Docker/local Postgres —
  the local container is now an explicit offline-only fallback (see
  HOSTING.md), not part of the normal day-to-day flow.

## 2026-08-26 — Launcher follow-up: widget URL bug + real window cleanup

Two more real bugs found by actually using the launcher repeatedly (the
way it'd get used day to day), both now fixed and round-trip tested
(start -> stop -> start, three times, verified clean each time):

- **Widget opened broken.** The repo path contains a space
  (`PROYECTOS PERSONALES`), which wasn't percent-encoded in the
  `file://` URL passed to Edge's `--app=` flag — Windows' argument
  splitting cut the URL off at the space, so Edge opened
  `file:///D:/PROYECTOS` (nonexistent) instead of the widget. Every
  re-run of Start created another broken Edge window. Fixed: the URL is
  now percent-encoded, and opening the widget is now idempotent too
  (skipped if a `widget/index.html` Edge process is already found).
- **Stop didn't actually close the visible windows.** `taskkill /F /T` on
  the port-owning process (node.exe/dotnet.exe) kills that process and
  its descendants, but not its *parent* - so the wrapper PowerShell
  window (title "Project - client-frontend"/"-backend") was left behind,
  now just an idle empty prompt, indistinguishable from a live one at a
  glance. Compounded by testing start.bat repeatedly without stopping
  cleanly in between: the user ended up with a cluttered taskbar (several
  duplicate "Project - client-*" windows, several broken widget Edge
  windows) and reasonably asked "what do I close and what not?" Fixed:
  `stop-dev.ps1` now walks up the parent chain from the port-owner (past
  the intermediate cmd.exe npm spawns), specifically looking for the
  "Project - ..." wrapper by its command line, and kills from there - the
  window itself closes now, not just the process inside it. Also closes
  the widget window on stop.
- Manually cleaned up the mess left behind by the testing above (5+ stray
  windows, several broken Edge tabs) - confirmed nothing was left running
  outside of a fresh `start.bat` afterward.

## 2026-08-26 — Reload-safe launcher + icon

- Added `assets/app-icon.ico` — a small generated icon (brand green
  rounded square, "C1"), no download involved, so the Desktop shortcuts
  have something better than the default batch-file icon while there's no
  real logo yet.
- Added `scripts/start.bat` / `scripts/stop.bat` (thin wrappers around the
  existing `.ps1` scripts) and repointed the Desktop shortcuts at them
  ("Start Project" / "Stop Project"), with the new icon.
- Made `start-dev.ps1` safe to run repeatedly: it checks each dev-server
  port first and skips relaunching anything already up, instead of
  spawning duplicate windows. Combined with Vite/`dotnet watch`'s own
  hot-reload, the intent is: run Start once per session, then just save
  files — no more stop/start per change.
- Rewrote `stop-dev.ps1` to kill by **port ownership**
  (`Get-NetTCPConnection` + `taskkill /F /T`) instead of matching spawned
  windows by title — title matching turned out unreliable (a spawned
  console window's title wasn't reliably readable from outside it, so the
  old version silently did nothing to the frontend/backend processes).
  Verified: start -> stop -> start again all work cleanly now.
- Fixed a real bug found while testing the above: `start-dev.ps1`'s
  spawned backend window couldn't find `dotnet` at all (user-scoped PATH
  change not yet visible to that process tree) — see
  `docs/DEPENDENCIES.md` "Known PATH gotcha". The script now sets its own
  `$env:Path` defensively before spawning anything.
- Also cleaned up Docker Compose's stale project registration left over
  from the folder rename (it still pointed at the old `NutriBoost\`
  path) — recreated the container fresh under the `english-c1-platform`
  project name and removed the two orphaned volumes from the old name
  (both were empty dev databases, nothing lost).

## 2026-08-26 — Scope correction

The "Initial scaffold" entry below built the wrong project — a fitness
e-commerce storefront ("NutriBoost") instead of this English-practice
platform. Full explanation: [errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md).
Fixes made:

- Root folder renamed `NutriBoost` → `english-c1-platform` (a plain
  placeholder, not a proposed name — see [NAMING.md](NAMING.md)). Updated
  to match: `.claude/launch.json`, `scripts/dev-frontend.cmd`, the window
  titles in `scripts/start-dev.ps1`/`stop-dev.ps1`, and the two Desktop
  shortcuts (now "Project - Start" / "Project - Stop").
- Removed the e-commerce domain from `client-backend`: `Product` entity,
  `ProductsController`, `IProductRepository`/`ProductRepository`, and the
  `InitialCreate` migration (rolled back against the local database first,
  then deleted). `NutriBoostDbContext` renamed to `AppDbContext`, now with
  no entities — see `docs/DEPENDENCIES.md` "Removed".
- `docker-compose.yml`: container/env names genericized
  (`nutriboost-postgres` → `app-postgres`, `nutriboost`/`nutriboost_client`
  → `app`/`app_dev`), container recreated. Still on host port 5433 (that
  part of the earlier fix was unrelated to the scope mistake and stays).
- Frontend: removed e-commerce-specific nav items (Shop, Cart) and page
  copy; `i18n` `brand.name` set to an explicit `[Project name]` placeholder
  instead of a made-up brand; `index.html` title updated; the
  `localStorage` key in `ThemeProvider` and in `widget/index.html`
  genericized.
- Docs rewritten to describe the actual project and stop asserting
  decisions (hosting, naming, pending features) that belonged to the other
  project: `ARCHITECTURE.md`, `HOSTING.md`, `NAMING.md`, `PENDING_IDEAS.md`,
  `COLOR_PALETTE.md`, `GIT_WORKFLOW.md`, `AI_WORKFLOW.md`, this file,
  `README.md`.
- **Not yet fixed** (tracked in `PENDING_IDEAS.md`): the backend's C#
  project/namespace names are still `NutriBoost.Client.*`. Left alone
  deliberately — a full rename is better done once alongside the real
  domain model than as two separate mechanical passes.

## 2026-08-26 — Initial scaffold (wrong project — see correction above)

- Created the monorepo root `NutriBoost/` at
  `D:\PROYECTOS PERSONALES\NutriBoost` (previous working directory,
  `D:\download`, was Downloads — not a real project location).
- Added `docs/` with `ARCHITECTURE.md`, `DEPENDENCIES.md`, `HOSTING.md`,
  `COLOR_PALETTE.md`, `NAMING.md`, `AI_WORKFLOW.md`, `GIT_WORKFLOW.md`,
  `PENDING_IDEAS.md`, `errors/`.
- Scaffolded `client-frontend/`: Vite + React + TypeScript, Tailwind CSS v4,
  shadcn/ui ("Nova" preset — Radix + Lucide + Geist), `react-router-dom`,
  `motion`, `i18next` + `react-i18next`.
  - Built `Navbar` (responsive: horizontal nav ≥ md, `Sheet` drawer below),
    `Footer`, a placeholder `Home` page, and a `ThemeProvider` (light
    default, dark-mode CSS variables already wired but not yet exposed to
    users).
  - Chose a green brand accent (`--primary`/`--accent`/`--ring`, hue 150)
    over shadcn's default neutral-only palette — see
    [COLOR_PALETTE.md](COLOR_PALETTE.md).
- Scaffolded `client-backend/`: ASP.NET Core 8, layered as
  `NutriBoost.Client.sln` → `src/NutriBoost.Client.{Domain,Application,
  Infrastructure,Api}`, referenced Domain → Application → Infrastructure →
  Api per [ARCHITECTURE.md](ARCHITECTURE.md#backend-layered--clean-architecture).
  First vertical slice end-to-end: `Product` entity (Domain) →
  `IProductRepository` + `GetProductsQueryHandler` (Application) →
  `NutriBoostDbContext` + `ProductRepository` (Infrastructure, EF Core +
  Npgsql, PostgreSQL `xmin` used as the optimistic-concurrency token) →
  `ProductsController` (`GET /api/products`) + CORS for the Vite dev
  origin + `GET /health` (Api). Builds clean, 0 warnings. `InitialCreate`
  migration created and applied against local Postgres; `GET /health` and
  `GET /api/products` verified end-to-end against a real database (empty
  result, as expected — no seed data yet).
- Added `tests/NutriBoost.Client.Domain.Tests` (xUnit) with 5 passing tests
  covering `Product`'s validation — establishes the testing pattern; the
  other layers don't have tests yet.
- `docker-compose.yml` Postgres moved from host port 5432 to **5433** —
  5432 was already taken by an unrelated local Postgres service on this
  machine. See [errors/2026-08-26-postgres-port-conflict.md](errors/2026-08-26-postgres-port-conflict.md).
- `admin-frontend/`, `admin-backend/` **not yet scaffolded** — see
  [PENDING_IDEAS.md](PENDING_IDEAS.md).
- Root `docker-compose.yml` added for local Postgres. Docker Desktop was
  installed but not running — started it this session to run the migration
  and end-to-end check above; it does not auto-start on login by default,
  so start it manually (or via the Desktop shortcut) each time.
- `scripts/start-dev.ps1` / `scripts/stop-dev.ps1` plus two Desktop
  shortcuts added, per the notes' request to not have to reopen everything
  by hand each time.
- `widget/` added: a minimal local HTML/JS pending-vs-done tracker,
  minimizable, opened alongside the dev environment by `start-dev.ps1` —
  see its own README for why it's a lightweight web widget rather than a
  native tray app (scope tradeoff, revisit if it's not enough later).
