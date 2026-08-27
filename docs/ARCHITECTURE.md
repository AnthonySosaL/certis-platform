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
| Backend | **ASP.NET Core 8 (C#) Web API** | Explicitly requested — partly to learn C#/.NET, which is widely asked for in job postings. LTS version. Live at `https://english-c1-api.runasp.net` — see [HOSTING.md](HOSTING.md). |
| Frontend | **Angular 22 + TypeScript** | Switched from React on 2026-08-26 — deliberate choice, not a mistake: the user already has "a ton" of React projects and wants portfolio breadth. Angular over Blazor (a C#-frontend option that was also considered and briefly started) because the user explicitly asked for Angular by name after weighing it. |
| Component library | **Angular Material** (Material 3 theming via `mat.theme()`) | Official, best-integrated Angular UI kit — analogous role to what shadcn played for the React version. |
| ORM | **EF Core** | The direct .NET equivalent of Prisma-style "controlled, tracked schema changes" — migrations live in source control. |
| Database | **SQL Server** (MonsterASP.NET, free tier) | Switched from PostgreSQL on 2026-08-26 once a real database was created — see [HOSTING.md](HOSTING.md). |
| Icons | **Inline SVG**, hand-written | `lucide-angular`'s peer dependency range doesn't yet cover Angular 22 (too new); a handful of small inline SVGs (menu, sun/moon) avoids both the version conflict and an icon web font. |
| Auth | Not built yet | Needs a decision on user model first (just the two of you vs. accounts for others later) — see PENDING_IDEAS.md. Per [AI_WORKFLOW.md](AI_WORKFLOW.md), security-sensitive design (registration/login/password reset) gets a Fable 5 pass before implementation. |

## Repo layout

```
english-c1-platform/   (folder name — placeholder, see NAMING.md)
├── client-frontend/    Angular 22 + Material — UI shell exists, no real pages yet
├── client-backend/     ASP.NET Core Web API — layered, builds and runs, live on MonsterASP.NET, no domain model yet
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

Namespaces/project names are `EnglishC1.Client.*` (`.sln`, `.csproj`
files, C# `namespace` declarations) — a placeholder tied to the folder
name, not a final brand (renamed from an earlier `NutriBoost.Client.*`
holdover on 2026-08-26; see
[errors/2026-08-26-scope-mixup.md](errors/2026-08-26-scope-mixup.md)).

## Frontend structure

```
src/app/
├── layout/          Navbar, Footer — structural, reused components
├── pages/           Route-level standalone components (Home, About, Contact — placeholders)
├── core/            Cross-cutting services (Theme, auth later)
├── app.routes.ts    Route table
└── app.ts/.html     App shell (renders Navbar + <router-outlet /> + Footer)
```

Standalone components throughout (no NgModules) — Angular 22's default and
recommended style. Component rule carried over from the original project
notes (still applies generically regardless of framework): build reusable
components from the start, prefer cards over extra tabs/pages where
content allows it, no emoji/text-icons — SVG icons only, sized
responsively.

## Cross-cutting concerns staged for later (not built yet)

- **Theming**: light mode is the default and only active mode; the full
  Material 3 dark theme already exists (`html.dark`, toggled via the
  `Theme` service — `src/app/core/theme.ts`) — flipping the default later
  is a one-line change.
- **i18n**: English only for now, no i18n library wired in yet (the React
  version had `i18next`; Angular's equivalent — `@angular/localize` for
  build-time, or `ngx-translate` for a runtime-switchable setup closer to
  what `i18next` did — isn't set up yet since the platform's primary
  language is English by design). Add if/when a language switcher is
  actually needed.
- **Auth**: not designed yet. Needs the user-model decision first (private
  2-person tool vs. something with real accounts) and a Fable 5 security
  pass before implementation — see AI_WORKFLOW.md.

## Design patterns in play so far

- **Signals** for reactive UI state (`Theme.mode`), Angular's current
  recommended default over RxJS `BehaviorSubject` for simple state.
- **Standalone components** — no NgModules, direct `imports: []` per
  component.
- **Repository pattern** (planned) for the backend's Infrastructure layer,
  once there's a real domain to persist.
