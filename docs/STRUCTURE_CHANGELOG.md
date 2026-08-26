# Structure changelog

Every new top-level folder, new app, or significant architectural change
gets an entry here, newest first — this is the traceability log the notes
asked for, separate from git history so it reads as a narrative instead of
a diff.

## 2026-08-26 — Initial scaffold

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
