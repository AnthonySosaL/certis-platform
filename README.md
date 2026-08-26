# English C1 practice platform (working name — see [docs/NAMING.md](docs/NAMING.md))

A platform to practice and evaluate English (reading/writing/listening/
speaking, progress tracking) for you and a friend working toward Cambridge
C1 — currently around B1/B2. Secondary goal: practice building with
C# .NET. Local development only for now. Possibly expanded/offered to
institutions later, but that's not the near-term scope.

**Feature scope is still being defined** — see
[docs/PENDING_IDEAS.md](docs/PENDING_IDEAS.md). What exists today is
infrastructure and a UI shell, not the actual English-practice features.

## Stack

React + TypeScript + Vite + shadcn/ui (frontend) · ASP.NET Core 8 + EF Core
+ SQL Server (backend, hosted free on MonsterASP.NET — see
[docs/HOSTING.md](docs/HOSTING.md)). Full rationale in
[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Repo layout

```
client-frontend/   React app — has a working Navbar/Footer/theme shell, no real pages yet
client-backend/    ASP.NET Core API, layered — builds and runs, no domain model yet, connects to the real cloud DB
docs/              architecture, dependencies, hosting, naming, pending ideas, error log
scripts/           local dev start/stop helpers
widget/            local pending/done task tracker (opens with start-dev.ps1)
docker-compose.yml local PostgreSQL — offline-only fallback, not used day to day (see docs/HOSTING.md)
```

## Quick start

Easiest: double-click the **"Start Project"** shortcut on the Desktop. It
starts the frontend dev server, the backend dev server (connected to the
real MonsterASP.NET database), and the task widget. Safe to double-click
again any time — it skips anything already running instead of duplicating
it, and both dev servers hot-reload on save, so you generally only need
this once per session. **"Stop Project"** shuts it all down.

Manually:

```bash
# 1. Frontend
cd client-frontend
npm install   # first time only
npm run dev   # http://localhost:5173

# 2. Backend (needs ConnectionStrings:AppDb in user-secrets first —
#    see client-backend/README.md)
cd client-backend/src/NutriBoost.Client.Api
dotnet watch run   # http://localhost:5223
```

## Before you read/write code here

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) — the stack and folder
  layout, and *why*.
- [docs/PENDING_IDEAS.md](docs/PENDING_IDEAS.md) — what's actually decided
  vs. still open, including the feature scope itself.
- [docs/GIT_WORKFLOW.md](docs/GIT_WORKFLOW.md) — branch-per-module, and the
  "is there anything sensitive in this diff?" check before every push.
- [docs/AI_WORKFLOW.md](docs/AI_WORKFLOW.md) — how coding sessions vs.
  ideation chats are meant to split.
- [docs/DEPENDENCIES.md](docs/DEPENDENCIES.md) — every package/tool added,
  and why.
- [docs/errors/](docs/errors/) — nontrivial mistakes and their fixes,
  including a scope mix-up on 2026-08-26 worth reading once (an unrelated
  project's notes got mistaken for this project's spec — see
  [docs/errors/2026-08-26-scope-mixup.md](docs/errors/2026-08-26-scope-mixup.md)).
