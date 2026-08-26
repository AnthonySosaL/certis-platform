# Structure changelog

Every new top-level folder, new app, or significant architectural change
gets an entry here, newest first — this is the traceability log the notes
asked for, separate from git history so it reads as a narrative instead of
a diff.

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
