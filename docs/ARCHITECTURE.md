# Architecture

Working name: TBD — see [NAMING.md](NAMING.md). A platform to practice and
evaluate English (targeting Cambridge C1) for you and a friend, built with
C# .NET partly as a learning goal in itself. Possibly expanded/offered to
institutions later — not the near-term scope. This doc is the source of
truth for *why* things are built the way they are; update it whenever a
decision changes, and log the change in
[STRUCTURE_CHANGELOG.md](STRUCTURE_CHANGELOG.md).

**What's built so far is infrastructure, not features.** The actual
English-practice domain (what a "practice session" or "evaluation" even
consists of) isn't decided yet — see
[PENDING_IDEAS.md](PENDING_IDEAS.md#feature-scope-not-yet-decided). Don't
read the folder names below as a finished design; they're a scaffold
waiting for that decision.

## Stack decision

| Layer | Choice | Why |
|---|---|---|
| Backend | **ASP.NET Core 8 (C#) Web API** | Explicitly requested — partly to learn C#/.NET, which is widely asked for in job postings. LTS version. |
| Frontend | **React 18 + TypeScript + Vite** | Pairs with shadcn/ui (see below). |
| Component library | **shadcn/ui** (Radix primitives, Tailwind CSS v4, "Nova" preset — Lucide icons, Geist font) | Reusable, consistent components from the start; owned source code, not an opaque dependency. |
| ORM | **EF Core** | The direct .NET equivalent of Prisma-style "controlled, tracked schema changes" — migrations live in source control. |
| Database | **PostgreSQL** | Free-tier friendly, works cleanly with EF Core. |
| Animation | **Motion** (the current Framer Motion package) | For a non-static, well-designed UI. |
| Auth | Not built yet | Needs a decision on user model first (just the two of you vs. accounts for others later) — see PENDING_IDEAS.md. |

## Repo layout

```
english-c1-platform/   (folder name — placeholder, see NAMING.md)
├── client-frontend/    React + TS + Vite + shadcn — UI shell exists, no real pages yet
├── client-backend/     ASP.NET Core Web API — layered, builds and runs, no domain model yet
├── docs/                this folder
└── scripts/             local dev start/stop helpers
```

A single app for now (not split into separate "client" and "admin" apps).
The client/admin isolation pattern that shows up in some of the reference
notes belongs to a *different* project (an e-commerce platform, seemingly
already in progress elsewhere on this machine — see
[docs/errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md))
and doesn't apply here unless this platform later grows a real
teacher/institution-facing admin surface.

## Backend: layered / Clean Architecture

`client-backend` is split into separate projects in one `.sln` so
dependencies are enforced by the compiler, not just convention:

- **Domain** — entities, value objects, domain logic. No dependencies on
  anything else. Currently empty — no entities defined yet.
- **Application** — use cases, interfaces for infrastructure. Depends only
  on Domain.
- **Infrastructure** — EF Core `DbContext` (`AppDbContext`, currently no
  `DbSet`s), repository implementations. Implements Application's
  interfaces.
- **Api** — controllers/minimal-API endpoints, DI wiring, middleware, CORS.
  The only project that knows about HTTP. Currently exposes only
  `GET /health`.

Rationale: keeps business rules testable without a database or HTTP server,
and keeps "which layer am I editing" explicit.

**Known holdover**: the actual namespaces/project names are still
`NutriBoost.Client.*` (`.sln`, `.csproj` files, C# `namespace`
declarations) — a mechanical rename pass across every file, better done
once alongside adding the real domain model than twice. Tracked in
[PENDING_IDEAS.md](PENDING_IDEAS.md).

## Frontend structure

```
src/
├── app/            App shell: routing, providers (theme, etc.)
├── components/
│   ├── ui/         shadcn primitives (generated — don't hand-edit heavily)
│   └── layout/     Navbar, Footer, and other structural, reused components
├── features/       Feature-based modules, once features are defined
├── i18n/           Translation resources (English active, Spanish scaffolded)
├── lib/            Shared utilities (shadcn's cn() helper, etc.)
└── pages/          Route-level components (currently a placeholder Home)
```

Component rule carried over from the reference notes (still applies
generically): build reusable components from the start, prefer cards over
extra tabs/pages where content allows it, no emoji/text-icons — Lucide SVG
icons only, sized responsively.

## Cross-cutting concerns staged for later (not built yet)

- **Theming**: light mode is the default and only active mode; the full
  dark-mode CSS variable set already exists (`.dark` class, toggled via
  `ThemeProvider`) — flipping the default later is a one-line change.
- **i18n**: English is the only language wired into the UI (the platform's
  primary language, since it's for English practice), but the `i18next`
  setup and a Spanish resource file exist side by side for the app's own
  UI chrome (nav labels etc.) if that's ever needed.
- **Auth**: not designed yet. Needs the user-model decision first (private
  2-person tool vs. something with real accounts).

## Design patterns in play so far

- **Provider pattern** for cross-cutting UI state (`ThemeProvider`).
- **Composition over configuration** for shadcn components (owned source
  code, not an opaque npm dependency).
- **Repository pattern** (planned) for the backend's Infrastructure layer,
  once there's a real domain to persist.
